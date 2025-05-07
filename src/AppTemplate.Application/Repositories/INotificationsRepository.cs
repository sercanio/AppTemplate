using AppTemplate.Domain.Notifications;
using Myrtus.Clarity.Core.Application.Repositories;
using System.Linq.Expressions;

namespace AppTemplate.Application.Repositories;

public interface INotificationsRepository : IRepository<Notification>
{
    Task<IEnumerable<Notification>> GetByPredicateAsync(
        Expression<Func<Notification, bool>> predicate,
        CancellationToken cancellationToken = default);

    Task<int> GetUnreadCountAsync(
        Guid userId,
        CancellationToken cancellationToken = default);

    Task<IEnumerable<Notification>> GetUnreadNotificationsAsync(
        Guid userId,
        int pageIndex = 0,
        int pageSize = 10,
        CancellationToken cancellationToken = default);

    Task<bool> MarkAsReadAsync(
        Guid notificationId,
        CancellationToken cancellationToken = default);

    Task MarkAllAsReadAsync(
        Guid userId,
        CancellationToken cancellationToken = default);
}