using AppTemplate.Application.Features.AppUsers.Queries.GetLoggedInUser;
using AppTemplate.Application.Repositories;
using AppTemplate.Domain.AppUsers;
using Ardalis.Result;
using Microsoft.AspNetCore.Http;
using Moq;
using System.Security.Claims;

namespace AppTemplate.Application.Tests.Unit.Features.AppUsersTests.Queries.GetLoggedInUserTests;

[Trait("Category", "Unit")]
public class GetLoggedInUserQueryHandlerUnitTests
{
  private readonly Mock<IAppUsersRepository> _userRepositoryMock;
  private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock;
  private readonly GetLoggedInUserQueryHandler _handler;

  public GetLoggedInUserQueryHandlerUnitTests()
  {
    _userRepositoryMock = new Mock<IAppUsersRepository>();
    _httpContextAccessorMock = new Mock<IHttpContextAccessor>();
    _handler = new GetLoggedInUserQueryHandler(_userRepositoryMock.Object, _httpContextAccessorMock.Object);
  }

  [Fact]
  public async Task Handle_ReturnsNotFound_WhenUserIdNotInClaims()
  {
    var httpContext = new DefaultHttpContext();
    httpContext.User = new ClaimsPrincipal(new ClaimsIdentity());
    _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(httpContext);

    var result = await _handler.Handle(new GetLoggedInUserQuery(), default);

    Assert.Equal(ResultStatus.NotFound, result.Status);
    Assert.Null(result.Value);
  }

  [Fact]
  public async Task Handle_ReturnsNotFound_WhenUserNotFoundInRepository()
  {
    var userId = "user-123";
    var claims = new[] { new Claim(ClaimTypes.NameIdentifier, userId) };
    var httpContext = new DefaultHttpContext();
    httpContext.User = new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuth"));
    _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(httpContext);

    _userRepositoryMock
        .Setup(r => r.GetUserByIdentityIdWithIdentityAndRolesAsync(userId, It.IsAny<CancellationToken>()))
        .ReturnsAsync(Result<AppUser>.NotFound("User not found"));

    var result = await _handler.Handle(new GetLoggedInUserQuery(), default);

    Assert.Equal(ResultStatus.NotFound, result.Status);
    Assert.Null(result.Value);
  }

  [Fact]
  public async Task Handle_ReturnsNotFound_WhenRepositoryReturnsNullUser()
  {
    var userId = "user-123";
    var claims = new[] { new Claim(ClaimTypes.NameIdentifier, userId) };
    var httpContext = new DefaultHttpContext();
    httpContext.User = new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuth"));
    _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(httpContext);

    _userRepositoryMock
        .Setup(r => r.GetUserByIdentityIdWithIdentityAndRolesAsync(userId, It.IsAny<CancellationToken>()))
        .ReturnsAsync(Result.Success<AppUser>(null));

    var result = await _handler.Handle(new GetLoggedInUserQuery(), default);

    Assert.Equal(ResultStatus.NotFound, result.Status);
    Assert.Null(result.Value);
  }
}