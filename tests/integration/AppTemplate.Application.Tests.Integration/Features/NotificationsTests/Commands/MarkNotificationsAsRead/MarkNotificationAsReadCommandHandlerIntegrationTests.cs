using System.Security.Claims;
using AppTemplate.Application.Features.Notifications.Commands.MarkNotificationsAsRead;
using AppTemplate.Application.Services.Clock;
using AppTemplate.Domain.AppUsers;
using AppTemplate.Domain.Notifications;
using AppTemplate.Domain.Notifications.Enums;
using AppTemplate.Infrastructure;
using AppTemplate.Infrastructure.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;


namespace AppTemplate.Application.Tests.Integration.Features.NotificationsTests.Commands.MarkNotificationsAsRead;

[Trait("Category", "Integration")]
public class MarkNotificationAsReadCommandHandlerIntegrationTests
{
  private ApplicationDbContext CreateDbContext()
  {
    var options = new DbContextOptionsBuilder<ApplicationDbContext>()
        .UseInMemoryDatabase(Guid.NewGuid().ToString())
        .Options;
    return new ApplicationDbContext(options, new DateTimeProvider());
  }

  [Fact]
  public async Task Handle_MarksNotificationAsRead_WhenValid()
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

    var notification = new Notification(appUser.Id, "Title", "Message", NotificationTypeEnum.System);
    appUser.Notifications.Add(notification);

    dbContext.AppUsers.Add(appUser);
    await dbContext.SaveChangesAsync();

    var notificationsRepo = new NotificationsRepository(dbContext);
    var appUsersRepo = new AppUsersRepository(dbContext);

    var httpContext = new DefaultHttpContext();
    httpContext.User = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, identityUser.Id) }, "TestAuth"));
    var httpContextAccessor = new HttpContextAccessor { HttpContext = httpContext };

    var handler = new MarkNotificationAsReadCommandHandler(notificationsRepo, httpContextAccessor, appUsersRepo);

    var command = new MarkNotificationAsReadCommand(notification.Id);

    // Act
    var result = await handler.Handle(command, default);

    // Assert
    Assert.Equal(Ardalis.Result.ResultStatus.Ok, result.Status);
    Assert.True(result.Value.Success);

    // Verify notification is marked as read in the database
    var updatedNotification = await dbContext.Notifications.FirstAsync(n => n.Id == notification.Id);
    Assert.True(updatedNotification.IsRead);
  }
}
