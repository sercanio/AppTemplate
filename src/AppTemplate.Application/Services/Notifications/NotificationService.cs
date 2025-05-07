using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Myrtus.Clarity.Core.Domain.Abstractions;
using Myrtus.Clarity.Core.Infrastructure.SignalR.Hubs;
using AppTemplate.Application.Repositories;
using AppTemplate.Application.Services.AppUsers;
using AppTemplate.Application.Services.Roles;
using AppTemplate.Domain.Notifications;
using AppTemplate.Domain.Roles;

namespace AppTemplate.Application.Services.Notifications;

/// <summary>
/// Service for managing and distributing notifications to users and groups
/// </summary>
public class NotificationsService : INotificationService
{
    private readonly INotificationsRepository _notificationsRepository;
    private readonly IHubContext<NotificationHub> _hubContext;
    private readonly IAppUsersService _usersService;
    private readonly IRolesService _rolesService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMemoryCache _cache;
    private readonly ILogger<NotificationsService> _logger;
    private readonly JsonSerializerSettings _jsonSettings;

    public NotificationsService(
        INotificationsRepository notificationsRepository,
        IHubContext<NotificationHub> hubContext,
        IAppUsersService usersService,
        IRolesService rolesService,
        IUnitOfWork unitOfWork,
        IMemoryCache cache,
        ILogger<NotificationsService> logger)
    {
        _notificationsRepository = notificationsRepository;
        _hubContext = hubContext;
        _usersService = usersService;
        _rolesService = rolesService;
        _unitOfWork = unitOfWork;
        _cache = cache;
        _logger = logger;

        // Initialize JSON serializer settings
        _jsonSettings = new JsonSerializerSettings
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver(),
            NullValueHandling = NullValueHandling.Ignore,
            DateTimeZoneHandling = DateTimeZoneHandling.Utc
        };
    }

    /// <summary>
    /// Sends a system-wide notification to all users
    /// </summary>
    public async Task SendNotificationAsync(string details)
    {
        try
        {
            var notification = new Notification(
                userId: Guid.Empty,
                user: "System",
                action: "System Notification",
                entity: "Global",
                entityId: string.Empty,
                details: SanitizeContent(details)
            );

            await _notificationsRepository.AddAsync(notification);
            await _unitOfWork.SaveChangesAsync();

            InvalidateUserNotificationCache("global");

            // Serialize the notification for SignalR
            string message = JsonConvert.SerializeObject(notification, _jsonSettings);
            await _hubContext.Clients.All.SendAsync("ReceiveNotification", message);

            _logger.LogInformation("System notification sent: {Details}", details);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send system notification: {Details}", details);
            throw new NotificationException("Failed to send system notification", ex);
        }
    }

    /// <summary>
    /// Sends a notification to a specific user
    /// </summary>
    public async Task SendNotificationToUserAsync(string details, Guid userId)
    {
        try
        {
            var user = await _usersService.GetUserByIdAsync(userId);
            if (user == null)
            {
                _logger.LogWarning("Cannot send notification to non-existent user: {UserId}", userId);
                return;
            }

            // Check if user has in-app notifications enabled
            bool canSendInAppNotification = user.NotificationPreference.IsInAppNotificationEnabled;
            if (!canSendInAppNotification)
            {
                _logger.LogInformation("User {UserId} has disabled in-app notifications, skipping SignalR delivery", userId);
            }

            var notification = new Notification(
                userId: userId,
                user: user.IdentityUser?.UserName ?? user.IdentityUser?.Email ?? "Unknown",
                action: "User Notification",
                entity: "User",
                entityId: userId.ToString(),
                details: SanitizeContent(details)
            );

            // Always save the notification even if real-time delivery is disabled
            await _notificationsRepository.AddAsync(notification);
            await _unitOfWork.SaveChangesAsync();

            InvalidateUserNotificationCache(userId.ToString());

            // Only send real-time notification if enabled
            if (canSendInAppNotification)
            {
                string message = JsonConvert.SerializeObject(notification, _jsonSettings);
                await _hubContext.Clients.User(userId.ToString()).SendAsync("ReceiveNotification", message);
                _logger.LogDebug("Real-time notification sent to user {UserId}: {Details}", userId, details);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send notification to user {UserId}: {Details}", userId, details);
            throw new NotificationException($"Failed to send notification to user {userId}", ex);
        }
    }

    /// <summary>
    /// Saves a notification for a user without real-time delivery
    /// </summary>
    public async Task SaveNotificationAsync(string details, Guid userId)
    {
        try
        {
            var user = await _usersService.GetUserByIdAsync(userId);
            if (user == null)
            {
                _logger.LogWarning("Cannot save notification for non-existent user: {UserId}", userId);
                return;
            }

            var notification = new Notification(
                userId: userId,
                user: user.IdentityUser?.UserName ?? user.IdentityUser?.Email ?? "Unknown",
                action: "Saved Notification",
                entity: "User",
                entityId: userId.ToString(),
                details: SanitizeContent(details)
            );

            await _notificationsRepository.AddAsync(notification);
            await _unitOfWork.SaveChangesAsync();

            InvalidateUserNotificationCache(userId.ToString());
            _logger.LogDebug("Notification saved for user {UserId}: {Details}", userId, details);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save notification for user {UserId}: {Details}", userId, details);
            throw new NotificationException($"Failed to save notification for user {userId}", ex);
        }
    }

    /// <summary>
    /// Sends a notification to all users in a specific role/group
    /// </summary>
    public async Task SendNotificationToUserGroupAsync(string details, string groupName)
    {
        try
        {
            _logger.LogInformation("Starting to send notification to group {GroupName}", groupName);

            // For well-known roles, use the static properties to avoid database queries
            Role? role = null;
            if (groupName == Role.Admin.Name.Value)
            {
                _logger.LogDebug("Using static Admin role reference for {GroupName}", groupName);
                role = Role.Admin;
            }
            else if (groupName == Role.DefaultRole.Name.Value)
            {
                _logger.LogDebug("Using static DefaultRole reference for {GroupName}", groupName);
                role = Role.DefaultRole;
            }
            else
            {
                // For other roles, use ToListAsync() to load all roles, then filter in memory
                // This avoids EF Core translation issues with value objects
                _logger.LogDebug("Querying database for role {GroupName}", groupName);
                var roles = await _rolesService.GetAllAsync(
                    size: 100, // Assuming you don't have thousands of roles
                    predicate: null, // Get all roles
                    includeSoftDeleted: false);

                role = roles.Items.FirstOrDefault(r => r.Name.Value == groupName);
            }

            if (role == null)
            {
                _logger.LogWarning("Cannot send notification to non-existent group: {GroupName}", groupName);
                return;
            }

            _logger.LogDebug("Found role with ID {RoleId} for group {GroupName}", role.Id, groupName);

            // Get users with this role
            var usersInGroup = await _usersService.GetAllAsync(
                predicate: null, // We'll filter in memory
                include: [u => u.IdentityUser, u => u.Roles]);

            // Filter in memory to avoid EF Core translation issues
            var filteredUsers = usersInGroup.Items
                .Where(u => u.Roles.Any(r => r.Id == role.Id))
                .ToList();

            if (!filteredUsers.Any())
            {
                _logger.LogInformation("No users found in group {GroupName} for notification", groupName);
                return;
            }

            _logger.LogDebug("Found {Count} users in group {GroupName}", filteredUsers.Count, groupName);

            // Sanitize the notification details
            var sanitizedDetails = SanitizeContent(details);

            // Create notifications for all users
            var notifications = new List<Notification>();
            var usersWithNotificationsEnabled = new List<string>();

            foreach (var user in filteredUsers)
            {
                var notification = new Notification(
                    userId: user.Id,
                    user: user.IdentityUser?.UserName ?? user.IdentityUser?.Email ?? "Unknown",
                    action: $"{groupName} Group Notification",
                    entity: "UserGroup",
                    entityId: groupName,
                    details: sanitizedDetails
                );
                notifications.Add(notification);

                // Add to repository one by one since we don't have AddRangeAsync
                await _notificationsRepository.AddAsync(notification);

                // Invalidate cache for each user
                InvalidateUserNotificationCache(user.Id.ToString());

                // Track users with notifications enabled for real-time delivery
                if (user.NotificationPreference.IsInAppNotificationEnabled)
                {
                    usersWithNotificationsEnabled.Add(user.Id.ToString());
                }
            }

            // Save all changes in a single transaction
            await _unitOfWork.SaveChangesAsync();

            // Send real-time notifications to each user with notifications enabled
            foreach (var notification in notifications)
            {
                if (usersWithNotificationsEnabled.Contains(notification.UserId.ToString()))
                {
                    string message = JsonConvert.SerializeObject(notification, _jsonSettings);
                    await _hubContext.Clients.User(notification.UserId.ToString()).SendAsync("ReceiveNotification", message);
                }
            }

            // Send group notification
            var groupNotification = new
            {
                GroupName = groupName,
                Details = sanitizedDetails,
                Timestamp = DateTime.UtcNow,
                RecipientCount = notifications.Count
            };

            string groupMessage = JsonConvert.SerializeObject(groupNotification, _jsonSettings);
            await _hubContext.Clients.Group(groupName).SendAsync("ReceiveGroupNotification", groupMessage);

            _logger.LogInformation("Successfully sent notification to {Count} users in group {GroupName}: {Details}",
                filteredUsers.Count, groupName, details);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send notification to group {GroupName}: {Details}",
                groupName, details);
            throw new NotificationException($"Failed to send notification to group {groupName}", ex);
        }
    }

    /// <summary>
    /// Gets all notifications for a specific user
    /// </summary>
    public async Task<List<Notification>> GetNotificationsByUserIdAsync(Guid userId)
    {
        string cacheKey = GetCacheKey(CacheKeyType.AllNotifications, userId.ToString());

        if (!_cache.TryGetValue(cacheKey, out List<Notification> notifications))
        {
            _logger.LogDebug("Cache miss for user notifications: {UserId}", userId);
            var results = await _notificationsRepository.GetByPredicateAsync(n => n.UserId == userId);
            notifications = results.OrderByDescending(n => n.Timestamp).ToList();

            // Cache with a reasonable expiration time
            var cacheOptions = new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(TimeSpan.FromMinutes(2))
                .SetSlidingExpiration(TimeSpan.FromMinutes(1))
                .SetPriority(CacheItemPriority.Normal);

            _cache.Set(cacheKey, notifications, cacheOptions);
        }

        return notifications;
    }

    /// <summary>
    /// Gets unread notifications for a specific user with pagination
    /// </summary>
    public async Task<List<Notification>> GetUnreadNotificationsAsync(
        Guid userId,
        int pageIndex = 0,
        int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        string cacheKey = GetCacheKey(CacheKeyType.UnreadNotifications, userId.ToString(), pageIndex, pageSize);

        if (!_cache.TryGetValue(cacheKey, out List<Notification> notifications))
        {
            _logger.LogDebug("Cache miss for unread notifications: {UserId}, Page: {Page}", userId, pageIndex);
            var results = await _notificationsRepository.GetUnreadNotificationsAsync(
                userId,
                pageIndex,
                pageSize,
                cancellationToken);

            notifications = results.ToList();

            // Cache with a shorter expiration for unread items (they change more often)
            var cacheOptions = new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(TimeSpan.FromMinutes(1))
                .SetPriority(CacheItemPriority.High);

            _cache.Set(cacheKey, notifications, cacheOptions);
        }

        return notifications;
    }

    /// <summary>
    /// Gets count of unread notifications for a specific user
    /// </summary>
    public async Task<int> GetUnreadCountAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        string cacheKey = GetCacheKey(CacheKeyType.UnreadCount, userId.ToString());

        if (!_cache.TryGetValue(cacheKey, out int count))
        {
            _logger.LogDebug("Cache miss for unread count: {UserId}", userId);
            count = await _notificationsRepository.GetUnreadCountAsync(userId, cancellationToken);

            // Cache with a short expiration time
            var cacheOptions = new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(TimeSpan.FromMinutes(1))
                .SetPriority(CacheItemPriority.High);

            _cache.Set(cacheKey, count, cacheOptions);
        }

        return count;
    }

    /// <summary>
    /// Marks a specific notification as read
    /// </summary>
    public async Task<bool> MarkNotificationAsReadAsync(Guid notificationId, CancellationToken cancellationToken = default)
    {
        try
        {
            bool success = await _notificationsRepository.MarkAsReadAsync(notificationId, cancellationToken);

            if (success)
            {
                // Find the notification to get its user ID
                var notification = await _notificationsRepository.GetAsync(
                    predicate: n => n.Id == notificationId,
                    cancellationToken: cancellationToken);

                if (notification != null)
                {
                    // Invalidate relevant caches
                    InvalidateUserNotificationCache(notification.UserId.ToString());

                    // Check if user has notifications enabled
                    var user = await _usersService.GetUserByIdAsync(notification.UserId);
                    if (user != null && user.NotificationPreference.IsInAppNotificationEnabled)
                    {
                        // Notify clients via SignalR
                        await _hubContext.Clients.User(notification.UserId.ToString())
                            .SendAsync("NotificationRead", notification.Id.ToString());

                        _logger.LogDebug("Notification {Id} marked as read for user {UserId}",
                            notificationId, notification.UserId);
                    }
                }
            }

            return success;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to mark notification {Id} as read", notificationId);
            throw new NotificationException($"Failed to mark notification {notificationId} as read", ex);
        }
    }

    /// <summary>
    /// Marks all notifications for a user as read
    /// </summary>
    public async Task MarkNotificationsAsReadAsync(Guid userId, CancellationToken cancellationToken)
    {
        try
        {
            await _notificationsRepository.MarkAllAsReadAsync(userId, cancellationToken);

            // Invalidate cache
            InvalidateUserNotificationCache(userId.ToString());

            // Check if user has notifications enabled
            var user = await _usersService.GetUserByIdAsync(userId);
            if (user != null && user.NotificationPreference.IsInAppNotificationEnabled)
            {
                // Notify clients
                await _hubContext.Clients.User(userId.ToString())
                    .SendAsync("NotificationsAllRead");

                _logger.LogInformation("All notifications marked as read for user {UserId}", userId);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to mark all notifications as read for user {UserId}", userId);
            throw new NotificationException($"Failed to mark all notifications as read for user {userId}", ex);
        }
    }

    #region Private Helper Methods

    /// <summary>
    /// Enum for SignalR client types
    /// </summary>
    private enum ClientType
    {
        All,
        User,
        Group
    }

    /// <summary>
    /// Enum for cache key types
    /// </summary>
    private enum CacheKeyType
    {
        AllNotifications,
        UnreadNotifications,
        UnreadCount
    }

    /// <summary>
    /// Generates a standardized cache key
    /// </summary>
    private string GetCacheKey(CacheKeyType type, string identifier, int? pageIndex = null, int? pageSize = null)
    {
        switch (type)
        {
            case CacheKeyType.AllNotifications:
                return $"notifications_user_{identifier}";
            case CacheKeyType.UnreadNotifications:
                return $"unread_notifications_user_{identifier}_{pageIndex}_{pageSize}";
            case CacheKeyType.UnreadCount:
                return $"unread_count_{identifier}";
            default:
                throw new ArgumentOutOfRangeException(nameof(type));
        }
    }

    /// <summary>
    /// Sanitizes content to prevent XSS attacks without HTML-encoding single quotes and other characters
    /// </summary>
    private string SanitizeContent(string content)
    {
        if (string.IsNullOrWhiteSpace(content))
            return content;

        // Modified sanitization approach that doesn't encode single quotes and other non-dangerous characters
        // This keeps quotes readable while still protecting against XSS

        // Replace only the most dangerous characters
        return content
            .Replace("<", "&lt;")
            .Replace(">", "&gt;")
            .Replace("javascript:", "")
            .Replace("script", "scr_ipt"); // Breaks script tags without affecting normal text
    }

    /// <summary>
    /// Invalidates all cache entries related to a user's notifications
    /// </summary>
    private void InvalidateUserNotificationCache(string userId)
    {
        // Remove specific cache keys
        _cache.Remove(GetCacheKey(CacheKeyType.AllNotifications, userId));
        _cache.Remove(GetCacheKey(CacheKeyType.UnreadCount, userId));

        // For paged cache entries, remove some common page sizes
        for (int page = 0; page < 5; page++)
        {
            foreach (var size in new[] { 5, 10, 20, 50 })
            {
                _cache.Remove(GetCacheKey(CacheKeyType.UnreadNotifications, userId, page, size));
            }
        }

        _logger.LogTrace("Invalidated notification cache for user {UserId}", userId);
    }

    #endregion
}

/// <summary>
/// Custom exception for notification-related errors
/// </summary>
public class NotificationException : Exception
{
    public NotificationException(string message) : base(message) { }
    public NotificationException(string message, Exception innerException) : base(message, innerException) { }
}