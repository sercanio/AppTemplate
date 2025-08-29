using AppTemplate.Domain.Notifications;
using AppTemplate.Domain.Notifications.Enums;
using Myrtus.Clarity.Core.Infrastructure.Pagination;
using System.Linq.Expressions;

namespace AppTemplate.Application.Services.Notifications;

public interface INotificationService
{
  Task<Notification> GetAsync(
      Expression<Func<Notification, bool>> predicate,
      bool includeSoftDeleted = false,
      Func<IQueryable<Notification>, IQueryable<Notification>>? include = null,
      CancellationToken cancellationToken = default);

  Task<PaginatedList<Notification>> GetAllAsync(
      int index = 0,
      int size = 10,
      Expression<Func<Notification, bool>>? predicate = null,
      bool includeSoftDeleted = false,
      Func<IQueryable<Notification>, IQueryable<Notification>>? include = null,
      CancellationToken cancellationToken = default);

  Task AddAsync(Notification notification);

  void Update(Notification notification);

  void Delete(Notification notification);

  Task<PaginatedList<Notification>> GetNotificationsByUserIdAsync(Guid userId, CancellationToken cancellation);

  Task<int> GetUnreadCountAsync(Guid userId, CancellationToken cancellationToken = default);

  Task<List<Notification>> GetUnreadNotificationsAsync(
      Guid userId,
      int pageIndex = 0,
      int pageSize = 10,
      CancellationToken cancellationToken = default);

  Task<bool> MarkNotificationAsReadAsync(Guid notificationId, CancellationToken cancellationToken = default);

  Task MarkNotificationsAsReadAsync(Guid userId, CancellationToken cancellationToken);

  Task SendNotificationAsync(string title, string message, NotificationTypeEnum type = NotificationTypeEnum.System, string? url = null);
  Task SendNotificationToUserAsync(string title, string message, NotificationTypeEnum type, Guid userId, string? url = null);
  Task SaveNotificationAsync(string title, string message, NotificationTypeEnum type, Guid userId, string? url = null);
  Task SendNotificationToUserGroupAsync(string title, string message, NotificationTypeEnum type, string operatedById, string groupName, string? url = null);
  Task SendNotificationToUserGroupsAsync(string title, string message, NotificationTypeEnum type, string operatedById, IReadOnlyList<string> groupNames, string? url = null);
}
