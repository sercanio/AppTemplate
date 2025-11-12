using System.Security.Claims;
using AppTemplate.Application.Features.Notifications.Queries.GetAllNotifications;
using AppTemplate.Application.Services.AppUsers;
using AppTemplate.Application.Services.Clock;
using AppTemplate.Application.Services.Roles;
using AppTemplate.Domain.AppUsers;
using AppTemplate.Domain.Notifications;
using AppTemplate.Domain.Notifications.Enums;
using AppTemplate.Infrastructure;
using AppTemplate.Infrastructure.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace AppTemplate.Application.Tests.Integration.Features.NotificationsTests.Queries.GetAllNotificationsTests;

public class GetAllNotificationsQueryHandlerIntegrationTests
{
  private ApplicationDbContext CreateDbContext()
  {
    var options = new DbContextOptionsBuilder<ApplicationDbContext>()
        .UseInMemoryDatabase(Guid.NewGuid().ToString())
        .Options;
    return new ApplicationDbContext(options, new DateTimeProvider());
  }

  [Fact]
  public async Task Handle_ReturnsNotificationsAndUnreadCount_WhenUserExists()
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

    var notification1 = new Notification(appUser.Id, "Title1", "Message1", NotificationTypeEnum.System) { };
    var notification2 = new Notification(appUser.Id, "Title2", "Message2", NotificationTypeEnum.System) { };

    notification1.MarkAsRead();

    appUser.Notifications.Add(notification1);
    appUser.Notifications.Add(notification2);

    dbContext.AppUsers.Add(appUser);
    await dbContext.SaveChangesAsync();

    var notificationsRepo = new NotificationsRepository(dbContext);
    var rolesService = new RolesService(new RolesRepository(dbContext));
    var usersService = new AppUsersService(new AppUsersRepository(dbContext), rolesService);
    var httpContext = new DefaultHttpContext();
    httpContext.User = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, identityUser.Id) }, "TestAuth"));
    var httpContextAccessor = new HttpContextAccessor { HttpContext = httpContext };

    var handler = new GetAllNotificationsQueryHandler(notificationsRepo, httpContextAccessor, usersService);

    var query = new GetAllNotificationsQuery(0, 10, default);

    // Act
    var result = await handler.Handle(query, default);

    // Assert
    Assert.Equal(Ardalis.Result.ResultStatus.Ok, result.Status);
    Assert.NotNull(result.Value);
    Assert.Equal(2, result.Value.PaginatedNotifications.Items.Count);
    Assert.Equal(1, result.Value.UnreadCount);
  }
}