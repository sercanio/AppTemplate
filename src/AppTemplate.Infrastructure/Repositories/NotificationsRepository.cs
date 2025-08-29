using System.Linq.Expressions;
using AppTemplate.Application.Repositories;
using AppTemplate.Domain.Notifications;
using Microsoft.EntityFrameworkCore;

namespace AppTemplate.Infrastructure.Repositories;

internal sealed class NotificationsRepository
    : Repository<Notification, Guid>, INotificationsRepository
{
  public NotificationsRepository(ApplicationDbContext dbContext) : base(dbContext) { }

  public async Task<IEnumerable<Notification>> GetByPredicateAsync(
      Expression<Func<Notification, bool>> predicate,
      CancellationToken cancellationToken = default)
  {
    return await DbContext.Notifications
        .AsNoTracking()
        .Where(predicate)
        .OrderByDescending(n => n.CreatedOnUtc)
        .ToListAsync(cancellationToken);
  }

  public async Task<int> GetUnreadCountAsync(Guid userId, CancellationToken cancellationToken = default)
      => await DbContext.Notifications
             .AsNoTracking()
             .CountAsync(n => n.RecipientId == userId && !n.IsRead, cancellationToken);

  public async Task<IEnumerable<Notification>> GetUnreadNotificationsAsync(
      Guid userId,
      int pageIndex = 0,
      int pageSize = 10,
      CancellationToken cancellationToken = default)
  {
    return await DbContext.Notifications
        .AsNoTracking()
        .Where(n => n.RecipientId == userId && !n.IsRead)
        .OrderByDescending(n => n.CreatedOnUtc)
        .Skip(pageIndex * pageSize)
        .Take(pageSize)
        .ToListAsync(cancellationToken);
  }

  public async Task<bool> MarkAsReadAsync(Guid notificationId, CancellationToken cancellationToken = default)
  {
    var notification = await DbContext.Notifications.FindAsync(new object[] { notificationId }, cancellationToken);
    if (notification == null) return false;

    notification.MarkAsRead();
    return await DbContext.SaveChangesAsync(cancellationToken) > 0;
  }

  public async Task MarkAllAsReadAsync(Guid userId, CancellationToken cancellationToken = default)
  {
    await DbContext.Notifications
        .Where(n => n.RecipientId == userId && !n.IsRead)
        .ExecuteUpdateAsync(s => s.SetProperty(n => n.IsRead, true), cancellationToken);
  }
}