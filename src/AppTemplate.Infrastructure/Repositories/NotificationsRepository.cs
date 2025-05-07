using System.Linq.Expressions;
using AppTemplate.Application.Repositories;
using AppTemplate.Domain.Notifications;
using Microsoft.EntityFrameworkCore;

namespace AppTemplate.Infrastructure.Repositories;

internal sealed class NotificationsRepository(ApplicationDbContext dbContext) 
    : Repository<Notification>(dbContext), INotificationsRepository
{
    public async Task<IEnumerable<Notification>> GetByPredicateAsync(
        Expression<Func<Notification, bool>> predicate,
        CancellationToken cancellationToken = default)
    {
        return await DbContext.Notifications
            .AsNoTracking()
            .Where(predicate)
            .OrderByDescending(n => n.Timestamp)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> GetUnreadCountAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await DbContext.Notifications
            .AsNoTracking()
            .Where(n => n.UserId == userId && !n.IsRead)
            .CountAsync(cancellationToken);
    }

    public async Task<IEnumerable<Notification>> GetUnreadNotificationsAsync(
        Guid userId,
        int pageIndex = 0,
        int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        return await DbContext.Notifications
            .AsNoTracking()
            .Where(n => n.UserId == userId && !n.IsRead)
            .OrderByDescending(n => n.Timestamp)
            .Skip(pageIndex * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> MarkAsReadAsync(Guid notificationId, CancellationToken cancellationToken = default)
    {
        var notification = await DbContext.Notifications.FindAsync([notificationId], cancellationToken: cancellationToken);
        if (notification == null) return false;

        notification.IsRead = true;
        return await DbContext.SaveChangesAsync(cancellationToken) > 0;
    }

    public async Task MarkAllAsReadAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        // Batch update for better performance
        await DbContext.Notifications
            .Where(n => n.UserId == userId && !n.IsRead)
            .ExecuteUpdateAsync(s => s.SetProperty(n => n.IsRead, true), cancellationToken);
    }
}