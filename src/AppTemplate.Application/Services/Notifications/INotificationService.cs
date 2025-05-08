using System.Linq.Expressions;
using AppTemplate.Domain.Notifications;
using Myrtus.Clarity.Core.Application.Abstractions.Pagination;

namespace AppTemplate.Application.Services.Notifications;

public interface INotificationService
{
    Task<Notification> GetAsync(
        Expression<Func<Notification, bool>> predicate,
        bool includeSoftDeleted = false,
        CancellationToken cancellationToken = default,
        params Expression<Func<Notification, object>>[] include);

    Task<IPaginatedList<Notification>> GetAllAsync(
        int index = 0,
        int size = 10,
        bool includeSoftDeleted = false,
        Expression<Func<Notification, bool>>? predicate = null,
        CancellationToken cancellationToken = default,
        params Expression<Func<Notification, object>>[] include);

    Task AddAsync(Notification notification);

    void Update(Notification notification);

    void Delete(Notification notification);

    Task<IPaginatedList<Notification>> GetNotificationsByUserIdAsync(Guid userId);

    Task<int> GetUnreadCountAsync(Guid userId, CancellationToken cancellationToken = default);

    Task<List<Notification>> GetUnreadNotificationsAsync(
        Guid userId,
        int pageIndex = 0,
        int pageSize = 10,
        CancellationToken cancellationToken = default);

    Task<bool> MarkNotificationAsReadAsync(Guid notificationId, CancellationToken cancellationToken = default);

    Task MarkNotificationsAsReadAsync(Guid userId, CancellationToken cancellationToken);

    Task SendNotificationAsync(string details);
    Task SendNotificationToUserAsync(string details, Guid userId);
    Task SaveNotificationAsync(string details, Guid userId);
    Task SendNotificationToUserGroupAsync(string details, string groupName);
}
