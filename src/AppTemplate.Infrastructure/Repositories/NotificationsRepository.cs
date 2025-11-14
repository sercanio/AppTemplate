using System.Linq.Expressions;
using AppTemplate.Application.Data.Pagination;
using AppTemplate.Application.Repositories;
using AppTemplate.Domain.Notifications;

namespace AppTemplate.Infrastructure.Repositories;

public sealed class NotificationsRepository
    : Repository<Notification, Guid>, INotificationsRepository
{
  public NotificationsRepository(ApplicationDbContext dbContext) : base(dbContext) { }

  public async Task<IEnumerable<Notification>> GetByPredicateAsync(
      Expression<Func<Notification, bool>> predicate,
      CancellationToken cancellationToken = default)
  {
    var paginated = await GetAllAsync(
        predicate: predicate,
        includeSoftDeleted: false,
        asNoTracking: true,
        cancellationToken: cancellationToken);

    // Return all items (no paging)
    return paginated.Items.OrderByDescending(n => n.CreatedOnUtc);
  }

  public async Task<int> GetUnreadCountAsync(Guid userId, CancellationToken cancellationToken = default)
  {
    return await ExistsAsync(
        predicate: n => n.RecipientId == userId && !n.IsRead,
        includeSoftDeleted: false,
        asNoTracking: true,
        cancellationToken: cancellationToken)
        ? await GetAllAsync(
            predicate: n => n.RecipientId == userId && !n.IsRead,
            includeSoftDeleted: false,
            asNoTracking: true,
            cancellationToken: cancellationToken)
            .ContinueWith(t => t.Result.TotalCount, cancellationToken)
        : 0;
  }

  public async Task<IEnumerable<Notification>> GetUnreadNotificationsAsync(
      Guid userId,
      int pageIndex = 0,
      int pageSize = 10,
      CancellationToken cancellationToken = default)
  {
    var paginated = await GetAllAsync(
        predicate: n => n.RecipientId == userId && !n.IsRead,
        pageIndex: pageIndex,
        pageSize: pageSize,
        includeSoftDeleted: false,
        asNoTracking: true,
        cancellationToken: cancellationToken);

    return paginated.Items.OrderByDescending(n => n.CreatedOnUtc);
  }

  public async Task<bool> MarkAsReadAsync(Guid notificationId, CancellationToken cancellationToken = default)
  {
    var notification = await GetAsync(
        predicate: n => n.Id == notificationId,
        includeSoftDeleted: false,
        asNoTracking: false,
        cancellationToken: cancellationToken);

    if (notification == null)
      return false;

    notification.MarkAsRead();
    Update(notification);
    return await DbContext.SaveChangesAsync(cancellationToken) > 0;
  }

  public async Task MarkAllAsReadAsync(Guid userId, CancellationToken cancellationToken = default)
  {
    var paginated = await GetAllAsync(
        predicate: n => n.RecipientId == userId && !n.IsRead,
        includeSoftDeleted: false,
        asNoTracking: false,
        cancellationToken: cancellationToken);

    foreach (var notification in paginated.Items)
    {
      notification.MarkAsRead();
      Update(notification);
    }

    await DbContext.SaveChangesAsync(cancellationToken);
  }

  public async Task<PaginatedList<Notification>> GetNotificationsByUserIdAsync(
      Guid userId,
      int pageIndex = 0,
      int pageSize = 10,
      CancellationToken cancellationToken = default)
  {
    return await GetAllAsync(
        predicate: n => n.RecipientId == userId,
        pageIndex: pageIndex,
        pageSize: pageSize,
        includeSoftDeleted: false,
        asNoTracking: true,
        cancellationToken: cancellationToken);
  }
}
