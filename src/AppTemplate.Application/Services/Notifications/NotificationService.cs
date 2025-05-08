using System.Linq.Expressions;
using AppTemplate.Application.Repositories;
using AppTemplate.Application.Services.AppUsers;
using AppTemplate.Application.Services.Roles;
using AppTemplate.Domain.Notifications;
using AppTemplate.Domain.Roles;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Myrtus.Clarity.Core.Application.Abstractions.Pagination;
using Myrtus.Clarity.Core.Domain.Abstractions;
using Myrtus.Clarity.Core.Infrastructure.Pagination;
using Myrtus.Clarity.Core.Infrastructure.SignalR.Hubs;
using Newtonsoft.Json;

namespace AppTemplate.Application.Services.Notifications;

public sealed class NotificationsService(
    INotificationsRepository notificationsRepository,
    IUnitOfWork unitOfWork,
    IAppUsersService usersService,
    IRolesService rolesService,
    IHubContext<NotificationHub> hubContext,
    IMemoryCache cache,
    ILogger<NotificationsService> logger) : INotificationService
{
    private readonly INotificationsRepository _notificationsRepository = notificationsRepository;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IAppUsersService _usersService = usersService;
    private readonly IRolesService _rolesService = rolesService;
    private readonly IHubContext<NotificationHub> _hubContext = hubContext;
    private readonly ILogger<NotificationsService> _logger = logger;
    private readonly IMemoryCache _cache = cache;

    private readonly JsonSerializerSettings _jsonSettings = new()
    {
        ContractResolver = new Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver(),
        NullValueHandling = NullValueHandling.Ignore,
        DateTimeZoneHandling = DateTimeZoneHandling.Utc,
        ReferenceLoopHandling = ReferenceLoopHandling.Ignore
    };


    public async Task AddAsync(Notification notification)
    {
        await _notificationsRepository.AddAsync(notification);
    }

    public void Delete(Notification notification)
    {
        _notificationsRepository.Delete(notification);
    }

    public void Update(Notification notification)
    {
        _notificationsRepository.Update(notification);
    }

    public async Task<IPaginatedList<Notification>> GetAllAsync(
        int index = 0,
        int size = 10,
        bool includeSoftDeleted = false,
        Expression<Func<Notification, bool>>? predicate = null,
        CancellationToken cancellationToken = default,
        params Expression<Func<Notification, object>>[] include)
    {
        var notifications = await _notificationsRepository.GetAllAsync(
            index,
            size,
            includeSoftDeleted,
            predicate,
            cancellationToken,
            include);

        PaginatedList<Notification> paginatedList = new(
            notifications.Items,
            notifications.TotalCount,
            notifications.PageIndex,
            notifications.PageSize);

        return paginatedList;
    }

    public async Task<Notification> GetAsync(
        Expression<Func<Notification, bool>> predicate,
        bool includeSoftDeleted = false,
        CancellationToken cancellationToken = default,
        params Expression<Func<Notification, object>>[] include)
    {
        Notification? notification = await _notificationsRepository.GetAsync(
            predicate,
            includeSoftDeleted,
            cancellationToken,
            include);

        return notification!;
    }

    public async Task<IPaginatedList<Notification>> GetNotificationsByUserIdAsync(Guid userId)
    {
        return await _notificationsRepository.GetAllAsync(
            predicate: notification => notification.UserId == userId,
            includeSoftDeleted: false,
            cancellationToken: default);
    }

    public async Task<int> GetUnreadCountAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _notificationsRepository.GetUnreadCountAsync(userId, cancellationToken);
    }

    public async Task<List<Notification>> GetUnreadNotificationsAsync(
        Guid userId,
        int pageIndex = 0,
        int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        return (await _notificationsRepository.GetUnreadNotificationsAsync(
            userId,
            pageIndex,
            pageSize,
            cancellationToken)).ToList();
    }

    public async Task<bool> MarkNotificationAsReadAsync(Guid notificationId, CancellationToken cancellationToken = default)
    {
        return await _notificationsRepository.MarkAsReadAsync(notificationId, cancellationToken);
    }

    public async Task MarkNotificationsAsReadAsync(Guid userId, CancellationToken cancellationToken)
    {
        await _notificationsRepository.MarkAllAsReadAsync(userId, cancellationToken);
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
                userName: "System",
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
                userName: user.IdentityUser?.UserName ?? user.IdentityUser?.Email ?? "Unknown",
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
                userName: user.IdentityUser?.UserName ?? user.IdentityUser?.Email ?? "Unknown",
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
                    userName: user.IdentityUser?.UserName ?? user.IdentityUser?.Email ?? "Unknown",
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
}

/// <summary>
/// Custom exception for notification-related errors
/// </summary>
public class NotificationException : Exception
{
    public NotificationException(string message) : base(message) { }
    public NotificationException(string message, Exception innerException) : base(message, innerException) { }
}