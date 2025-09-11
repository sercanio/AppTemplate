using AppTemplate.Application.Features.Notifications.Queries.GetAllNotifications;
using AppTemplate.Application.Repositories;
using AppTemplate.Application.Services.AppUsers;
using AppTemplate.Domain.AppUsers;
using Ardalis.Result;
using Microsoft.AspNetCore.Http;
using Moq;
using System.Linq.Expressions;
using System.Security.Claims;

namespace AppTemplate.Application.Tests.Unit.Features.NotificationsTests.Queries.GetAllNotificationsTests;

[Trait("Category", "Unit")]
public class GetAllNotificationsQueryHandlerUnitTests
{
  private readonly Mock<INotificationsRepository> _notificationsRepositoryMock = new();
  private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock = new();
  private readonly Mock<IAppUsersService> _usersServiceMock = new();
  private readonly GetAllNotificationsQueryHandler _handler;

  public GetAllNotificationsQueryHandlerUnitTests()
  {
    _handler = new GetAllNotificationsQueryHandler(
        _notificationsRepositoryMock.Object,
        _httpContextAccessorMock.Object,
        _usersServiceMock.Object);
  }

  [Fact]
  public async Task Handle_ReturnsNotFound_WhenUserIsNull()
  {
    // Arrange
    var httpContext = new DefaultHttpContext();
    httpContext.User = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, "user-1") }));
    _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(httpContext);

    _usersServiceMock
        .Setup(s => s.GetAsync(
            It.IsAny<Expression<Func<AppUser, bool>>>(),
            It.IsAny<bool>(),
            It.IsAny<Func<IQueryable<AppUser>, IQueryable<AppUser>>>(),
            It.IsAny<bool>(),
            It.IsAny<CancellationToken>()))
        .ReturnsAsync((AppUser)null);

    var query = new GetAllNotificationsQuery(0, 10, default);

    // Act
    var result = await _handler.Handle(query, default);

    // Assert
    Assert.Equal(ResultStatus.NotFound, result.Status);
  }

  [Fact]
  public async Task Handle_ReturnsNotifications_WhenUserExists()
  {
    // Arrange
    var httpContext = new DefaultHttpContext();
    httpContext.User = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, "user-1") }));
    _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(httpContext);

    var notifications = new List<Domain.Notifications.Notification>
        {
            new(Guid.NewGuid(), "Title1", "Message1", Domain.Notifications.Enums.NotificationTypeEnum.System) {},
            new(Guid.NewGuid(), "Title2", "Message2", Domain.Notifications.Enums.NotificationTypeEnum.System) {}
        };
    notifications[1].MarkAsRead();
    var appUser = AppUser.Create();
    appUser.SetIdentityId("user-1");
    foreach (var n in notifications)
      appUser.Notifications.Add(n);

    _usersServiceMock
        .Setup(s => s.GetAsync(
            It.IsAny<Expression<Func<AppUser, bool>>>(),
            It.IsAny<bool>(),
            It.IsAny<Func<IQueryable<AppUser>, IQueryable<AppUser>>>(),
            It.IsAny<bool>(),
            It.IsAny<CancellationToken>()))
        .ReturnsAsync(appUser);

    var query = new GetAllNotificationsQuery(0, 10, default);

    // Act
    var result = await _handler.Handle(query, default);

    // Assert
    Assert.Equal(ResultStatus.Ok, result.Status);
    Assert.NotNull(result.Value);
    Assert.Equal(2, result.Value.PaginatedNotifications.Items.Count);
    Assert.Equal(1, result.Value.UnreadCount);
  }
}