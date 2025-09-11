using AppTemplate.Application.Features.Notifications.Commands.MarkAllNotificationsAsRead;
using AppTemplate.Core.Infrastructure.Clock;
using AppTemplate.Domain.AppUsers;
using AppTemplate.Domain.Notifications;
using AppTemplate.Domain.Notifications.Enums;
using AppTemplate.Infrastructure;
using AppTemplate.Infrastructure.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace AppTemplate.Application.Tests.Integration.Features.NotificationsTests.Commands.MarkAllNotificationsAsRead;

public class MarkAllNotificationsAsReadCommandHandlerIntegrationTests
{
  private ApplicationDbContext CreateDbContext()
  {
    var options = new DbContextOptionsBuilder<ApplicationDbContext>()
        .UseInMemoryDatabase(Guid.NewGuid().ToString())
        .Options;
    return new ApplicationDbContext(options, new DateTimeProvider());
  }

  [Fact]
  public async Task Handle_MarksAllNotificationsAsRead_WhenValid()
  {
    // Arrange
    var dbContext = CreateDbContext();

    var identityUser = new IdentityUser
    {
      Id = "user-1",
      UserName = "testuser",
      Email = "testuser@example.com"
    };
    dbContext.Users.Add(identityUser);

    var appUser = AppUser.Create();
    appUser.SetIdentityId(identityUser.Id);

    var notification1 = new Notification(appUser.Id, "Title1", "Message1", NotificationTypeEnum.System);
    var notification2 = new Notification(appUser.Id, "Title2", "Message2", NotificationTypeEnum.System);
    appUser.Notifications.Add(notification1);
    appUser.Notifications.Add(notification2);

    dbContext.AppUsers.Add(appUser);
    await dbContext.SaveChangesAsync();

    var notificationsRepo = new NotificationsRepository(dbContext);
    var appUsersRepo = new AppUsersRepository(dbContext);

    var httpContext = new DefaultHttpContext();
    httpContext.User = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, identityUser.Id) }, "TestAuth"));
    var httpContextAccessor = new HttpContextAccessor { HttpContext = httpContext };

    var handler = new MarkAllNotificationsAsReadCommandHandler(notificationsRepo, httpContextAccessor, appUsersRepo);

    var command = new MarkAllNotificationsAsReadCommand(default);

    // Act
    var result = await handler.Handle(command, default);

    // Assert
    Assert.Equal(Ardalis.Result.ResultStatus.Ok, result.Status);
    Assert.True(result.Value.Success);

    // Verify all notifications are marked as read in the database
    var updatedNotifications = await dbContext.Notifications.Where(n => n.RecipientId == appUser.Id).ToListAsync();
    Assert.All(updatedNotifications, n => Assert.True(n.IsRead));
  }
}
