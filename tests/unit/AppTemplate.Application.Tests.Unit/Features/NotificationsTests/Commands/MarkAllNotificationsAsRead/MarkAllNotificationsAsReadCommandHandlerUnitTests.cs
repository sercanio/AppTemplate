using AppTemplate.Application.Features.Notifications.Commands.MarkAllNotificationsAsRead;
using AppTemplate.Application.Repositories;
using AppTemplate.Domain.AppUsers;
using Ardalis.Result;
using Microsoft.AspNetCore.Http;
using Moq;
using System.Security.Claims;

namespace AppTemplate.Application.Tests.Unit.Features.NotificationsTests.Commands.MarkAllNotificationsAsRead;

[Trait("Category", "Unit")]
public class MarkAllNotificationsAsReadCommandHandlerUnitTests
{
  private readonly Mock<INotificationsRepository> _notificationsRepositoryMock = new();
  private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock = new();
  private readonly Mock<IAppUsersRepository> _appUsersRepositoryMock = new();
  private readonly MarkAllNotificationsAsReadCommandHandler _handler;

  public MarkAllNotificationsAsReadCommandHandlerUnitTests()
  {
    _handler = new MarkAllNotificationsAsReadCommandHandler(
        _notificationsRepositoryMock.Object,
        _httpContextAccessorMock.Object,
        _appUsersRepositoryMock.Object);
  }

  [Fact]
  public async Task Handle_ReturnsNotFound_WhenIdentityIdIsNull()
  {
    _httpContextAccessorMock.Setup(x => x.HttpContext).Returns((HttpContext)null);

    var command = new MarkAllNotificationsAsReadCommand(default);

    var result = await _handler.Handle(command, default);

    Assert.Equal(ResultStatus.NotFound, result.Status);
    Assert.Contains("IdentityId is null or empty string.", result.Errors);
  }

  [Fact]
  public async Task Handle_ReturnsNotFound_WhenUserNotFound()
  {
    var httpContext = new DefaultHttpContext();
    httpContext.User = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, "user-1") }));
    _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(httpContext);

    _appUsersRepositoryMock
        .Setup(r => r.GetUserByIdentityIdWithIdentityAndRolesAsync("user-1", It.IsAny<CancellationToken>()))
        .ReturnsAsync(Result<AppUser>.NotFound("User not found."));

    var command = new MarkAllNotificationsAsReadCommand(default);

    var result = await _handler.Handle(command, default);

    Assert.Equal(ResultStatus.NotFound, result.Status);
    Assert.Contains("User not found.", result.Errors);
  }

  [Fact]
  public async Task Handle_ReturnsSuccess_WhenNotificationsMarkedAsRead()
  {
    var httpContext = new DefaultHttpContext();
    httpContext.User = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, "user-1") }));
    _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(httpContext);

    var appUser = AppUser.Create();
    appUser.SetIdentityId("user-1");

    _appUsersRepositoryMock
        .Setup(r => r.GetUserByIdentityIdWithIdentityAndRolesAsync("user-1", It.IsAny<CancellationToken>()))
        .ReturnsAsync(Result.Success(appUser));

    _notificationsRepositoryMock
        .Setup(r => r.MarkAllAsReadAsync(appUser.Id, It.IsAny<CancellationToken>()))
        .Returns(Task.CompletedTask);

    var command = new MarkAllNotificationsAsReadCommand(default);

    var result = await _handler.Handle(command, default);

    Assert.Equal(ResultStatus.Ok, result.Status);
    Assert.True(result.Value.Success);
  }
}
