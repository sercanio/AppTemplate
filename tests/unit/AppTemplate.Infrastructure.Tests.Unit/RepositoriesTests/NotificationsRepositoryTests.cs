using Microsoft.EntityFrameworkCore;
using AppTemplate.Core.Infrastructure.Clock;
using AppTemplate.Domain.Notifications;
using AppTemplate.Domain.Notifications.Enums;
using AppTemplate.Infrastructure.Repositories;

namespace AppTemplate.Infrastructure.Tests.Unit.RepositoriesTests;

[Trait("Category", "Unit")]
public class NotificationsRepositoryTests
{
  private ApplicationDbContext CreateDbContext()
  {
    var options = new DbContextOptionsBuilder<ApplicationDbContext>()
        .UseInMemoryDatabase(Guid.NewGuid().ToString())
        .Options;
    var dateTimeProvider = new DateTimeProvider();
    return new ApplicationDbContext(options, dateTimeProvider);
  }

  [Fact]
  public async Task GetUnreadCountAsync_ReturnsZero_WhenNoUnreadNotifications()
  {
    var dbContext = CreateDbContext();
    var repo = new NotificationsRepository(dbContext);

    var userId = Guid.NewGuid();
    var count = await repo.GetUnreadCountAsync(userId);

    Assert.Equal(0, count);
  }

  [Fact]
  public async Task GetUnreadCountAsync_ReturnsCorrectCount()
  {
    var dbContext = CreateDbContext();
    var userId = Guid.NewGuid();

    dbContext.Notifications.Add(new Notification(userId, "Title1", "Message1", NotificationTypeEnum.System));
    dbContext.Notifications.Add(new Notification(userId, "Title2", "Message2", NotificationTypeEnum.System));
    var readNotification = new Notification(userId, "Title3", "Message3", NotificationTypeEnum.System);
    readNotification.MarkAsRead();
    dbContext.Notifications.Add(readNotification);

    await dbContext.SaveChangesAsync();

    var repo = new NotificationsRepository(dbContext);

    var count = await repo.GetUnreadCountAsync(userId);

    Assert.Equal(2, count);
  }

  [Fact]
  public async Task MarkAsReadAsync_MarksNotificationAsRead()
  {
    var dbContext = CreateDbContext();
    var userId = Guid.NewGuid();
    var notification = new Notification(userId, "Title", "Message", NotificationTypeEnum.System);
    dbContext.Notifications.Add(notification);
    await dbContext.SaveChangesAsync();

    var repo = new NotificationsRepository(dbContext);

    var result = await repo.MarkAsReadAsync(notification.Id);

    Assert.True(result);
    var updated = await dbContext.Notifications.FindAsync(notification.Id);
    Assert.True(updated.IsRead);
  }

  [Fact]
  public async Task GetUnreadNotificationsAsync_ReturnsUnreadNotifications()
  {
    var dbContext = CreateDbContext();
    var userId = Guid.NewGuid();
    var unread1 = new Notification(userId, "Title1", "Message1", NotificationTypeEnum.System);
    var unread2 = new Notification(userId, "Title2", "Message2", NotificationTypeEnum.System);
    var read = new Notification(userId, "Title3", "Message3", NotificationTypeEnum.System);
    read.MarkAsRead();

    dbContext.Notifications.Add(unread1);
    dbContext.Notifications.Add(unread2);
    dbContext.Notifications.Add(read);
    await dbContext.SaveChangesAsync();

    var repo = new NotificationsRepository(dbContext);

    var unreadNotifications = (await repo.GetUnreadNotificationsAsync(userId)).ToList();

    Assert.Equal(2, unreadNotifications.Count);
    Assert.All(unreadNotifications, n => Assert.False(n.IsRead));
  }

  [Fact]
  public async Task MarkAllAsReadAsync_MarksAllUnreadNotificationsAsRead()
  {
    var dbContext = CreateDbContext();
    var userId = Guid.NewGuid();
    var unread1 = new Notification(userId, "Title1", "Message1", NotificationTypeEnum.System);
    var unread2 = new Notification(userId, "Title2", "Message2", NotificationTypeEnum.System);
    dbContext.Notifications.Add(unread1);
    dbContext.Notifications.Add(unread2);
    await dbContext.SaveChangesAsync();

    var repo = new NotificationsRepository(dbContext);

    await repo.MarkAllAsReadAsync(userId);

    var allNotifications = dbContext.Notifications.Where(n => n.RecipientId == userId).ToList();
    Assert.All(allNotifications, n => Assert.True(n.IsRead));
  }
}
