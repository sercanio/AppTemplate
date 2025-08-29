using AppTemplate.Application.Repositories;
using AppTemplate.Application.Services.AppUsers;
using AppTemplate.Application.Services.Roles;
using AppTemplate.Core.Domain.Abstractions;
using AppTemplate.Core.Infrastructure.Pagination;
using AppTemplate.Domain.Notifications;
using AppTemplate.Domain.Notifications.Enums;
using AppTemplate.Domain.Roles;
using Ganss.Xss;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Linq.Expressions;

namespace AppTemplate.Application.Services.Notifications;

public sealed class NotificationsService(
    INotificationsRepository notificationsRepository,
    IUnitOfWork unitOfWork,
    IAppUsersService usersService,
    IRolesService rolesService,
    IMemoryCache cache,
    ILogger<NotificationsService> logger) : INotificationService
{
  private readonly INotificationsRepository _notificationsRepository = notificationsRepository;
  private readonly IUnitOfWork _unitOfWork = unitOfWork;
  private readonly IAppUsersService _usersService = usersService;
  private readonly IRolesService _rolesService = rolesService;
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

  public async Task<PaginatedList<Notification>> GetAllAsync(
      int index = 0,
      int size = 10,
      Expression<Func<Notification, bool>>? predicate = null,
      bool includeSoftDeleted = false,
      Func<IQueryable<Notification>, IQueryable<Notification>>? include = null,
      CancellationToken cancellationToken = default)
  {
    var notifications = await _notificationsRepository.GetAllAsync(
        pageIndex: index,
        pageSize: size,
        predicate: predicate,
        includeSoftDeleted: includeSoftDeleted,
        include: include,
        cancellationToken: cancellationToken);

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
      Func<IQueryable<Notification>, IQueryable<Notification>>? include = null,
      CancellationToken cancellationToken = default)
  {
    Notification? notification = await _notificationsRepository.GetAsync(
        predicate: predicate,
        includeSoftDeleted: includeSoftDeleted,
        include: include,
        cancellationToken: cancellationToken);

    return notification!;
  }

  public async Task<PaginatedList<Notification>> GetNotificationsByUserIdAsync(Guid userId, CancellationToken cancellationToken)
  {
    return await _notificationsRepository.GetAllAsync(
        predicate: notification => notification.RecipientId == userId,
        includeSoftDeleted: false,
        cancellationToken: cancellationToken);
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
  public async Task SendNotificationAsync(string title, string message, NotificationTypeEnum type = NotificationTypeEnum.System, string? url = null)
  {
    try
    {
      var notification = new Notification(
          recipientId: Guid.Empty,
          title: title,
          message: SanitizeContent(message),
          type: type
      );

      await _notificationsRepository.AddAsync(notification);
      await _unitOfWork.SaveChangesAsync();

      InvalidateUserNotificationCache("global");

      // Serialize the notification for SignalR
      string serializedMessage = JsonConvert.SerializeObject(notification, _jsonSettings);

      _logger.LogInformation("System notification sent: {Details}", message);
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Failed to send system notification: {Details}", message);
      throw new NotificationException("Failed to send system notification", ex);
    }
  }

  /// <summary>
  /// Sends a notification to a specific user
  /// </summary>
  public async Task SendNotificationToUserAsync(string title, string message, NotificationTypeEnum type, Guid userId, string? url = null)
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
          recipientId: userId,
          title: title,
          message: SanitizeContent(message),
          type: type);

      // Always save the notification even if real-time delivery is disabled
      await _notificationsRepository.AddAsync(notification);
      await _unitOfWork.SaveChangesAsync();

      InvalidateUserNotificationCache(userId.ToString());

      // Only send real-time notification if enabled
      if (canSendInAppNotification)
      {
        string serializedMessage = JsonConvert.SerializeObject(notification, _jsonSettings); // Renamed variable to avoid conflict
        _logger.LogDebug("Real-time notification sent to user {UserId}: {SerializedMessage}", userId, serializedMessage);
      }
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Failed to send notification to user {UserId}: {Message}", userId, message);
      throw new NotificationException($"Failed to send notification to user {userId}", ex);
    }
  }

  /// <summary>
  /// Saves a notification for a user without real-time delivery
  /// </summary>
  public async Task SaveNotificationAsync(string title, string message, NotificationTypeEnum type, Guid userId, string? url = null)
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
          recipientId: userId,
          title: title,
          message: SanitizeContent(message),
          type: type
      );

      await _notificationsRepository.AddAsync(notification);
      await _unitOfWork.SaveChangesAsync();

      InvalidateUserNotificationCache(userId.ToString());
      _logger.LogDebug("Notification saved for user {UserId}: {Message}", userId, message);
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Failed to save notification for user {UserId}: {Message}", userId, message);
      throw new NotificationException($"Failed to save notification for user {userId}", ex);
    }
  }

  /// <summary>
  /// Sends a notification to all users in a specific role/group
  /// </summary>
  public async Task SendNotificationToUserGroupAsync(string title, string message, NotificationTypeEnum type, string operatedById, string groupName, string? url = null)
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
          predicate: null,
          include: query => query.
              Include(u => u.IdentityUser).
              Include(u => u.Roles));

      // Filter in memory to avoid EF Core translation issues
      var filteredUsers = usersInGroup.Items
          .Where(u => u.Roles.Any(r => r.Id == role.Id))
          .ToList();

      // Exclude operator from users list
      filteredUsers = filteredUsers.Where(u => u.IdentityId != operatedById).ToList();

      if (!filteredUsers.Any())
      {
        _logger.LogInformation("No users found in group {GroupName} for notification", groupName);
        return;
      }

      _logger.LogDebug("Found {Count} users in group {GroupName}", filteredUsers.Count, groupName);

      // Sanitize the notification details
      var sanitizedMessage = SanitizeContent(message);

      // Create notifications for all users
      var notifications = new List<Notification>();
      var usersWithNotificationsEnabled = new List<string>();

      foreach (var user in filteredUsers)
      {
        var notification = new Notification(
            recipientId: user.Id,
            title: title,
            message: sanitizedMessage,
            type: type
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
        if (usersWithNotificationsEnabled.Contains(notification.RecipientId.ToString()))
        {
          string notificationMessage = JsonConvert.SerializeObject(notification, _jsonSettings);
        }
      }

      // Send group notification
      var groupNotification = new
      {
        GroupName = groupName,
        Details = sanitizedMessage,
        Timestamp = DateTime.UtcNow,
        RecipientCount = notifications.Count
      };

      string groupMessage = JsonConvert.SerializeObject(groupNotification, _jsonSettings);

      _logger.LogInformation("Successfully sent notification to {Count} users in group {GroupName}: {Message}",
          filteredUsers.Count, groupName, message);
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Failed to send notification to group {GroupName}: {Message}",
          groupName, message);
      throw new NotificationException($"Failed to send notification to group {groupName}", ex);
    }
  }

  /// <summary>
  /// Sends a notification to all users in a specific roles/groups
  /// </summary>
  public async Task SendNotificationToUserGroupsAsync(
  string title,
  string message,
  NotificationTypeEnum type,
  string operatedById,
  IReadOnlyList<string> groupNames,
  string? url = null)
  {
    foreach (var groupName in groupNames)
    {
      await SendNotificationToUserGroupAsync(title, message, type, operatedById, groupName, url);
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
  /// Sanitizes content to prevent XSS attacks using HtmlSanitizer
  /// </summary>
  private string SanitizeContent(string content)
  {
    if (string.IsNullOrWhiteSpace(content))
      return content;

    var sanitizer = new HtmlSanitizer();
    sanitizer.AllowedTags.Add("a");
    sanitizer.AllowedTags.Add("b");
    sanitizer.AllowedTags.Add("i");
    sanitizer.AllowedTags.Add("strong");
    sanitizer.AllowedTags.Add("em");
    sanitizer.AllowedTags.Add("u");
    sanitizer.AllowedTags.Add("span");
    sanitizer.AllowedAttributes.Add("href");
    sanitizer.AllowedAttributes.Add("title");
    sanitizer.AllowedAttributes.Add("class");
    sanitizer.AllowedAttributes.Add("target");
    sanitizer.AllowedAttributes.Add("rel");
    // Add more tags/attributes as needed

    return sanitizer.Sanitize(content);
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