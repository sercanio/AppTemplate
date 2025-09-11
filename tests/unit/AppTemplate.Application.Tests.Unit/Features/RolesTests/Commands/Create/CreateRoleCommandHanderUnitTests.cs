using AppTemplate.Application.Features.Roles.Commands.Create;
using AppTemplate.Application.Repositories;
using AppTemplate.Core.Application.Abstractions.Caching;
using AppTemplate.Core.Domain.Abstractions;
using AppTemplate.Domain.AppUsers;
using AppTemplate.Domain.Roles;
using Ardalis.Result;
using Microsoft.AspNetCore.Http;
using Moq;
using System.Linq.Expressions;
using System.Security.Claims;

namespace AppTemplate.Application.Tests.Unit.Features.RolesTests.Commands.Create;

[Trait("Category", "Unit")]
public class CreateRoleCommandHanderUnitTests
{
  private readonly Mock<IRolesRepository> _rolesRepositoryMock = new();
  private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
  private readonly Mock<ICacheService> _cacheServiceMock = new();
  private readonly Mock<IAppUsersRepository> _usersRepositoryMock = new();
  private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock = new();
  private readonly CreateRoleCommandHander _handler;

  public CreateRoleCommandHanderUnitTests()
  {
    _handler = new CreateRoleCommandHander(
        _rolesRepositoryMock.Object,
        _unitOfWorkMock.Object,
        _cacheServiceMock.Object,
        _usersRepositoryMock.Object,
        _httpContextAccessorMock.Object);
  }

  [Fact]
  public async Task Handle_ReturnsConflict_WhenRoleNameExists()
  {
    var command = new CreateRoleCommand("Admin", "Administrator");

    _rolesRepositoryMock
        .Setup(r => r.ExistsAsync(It.IsAny<Expression<Func<Role, bool>>>(),
          It.IsAny<bool>(),
          It.IsAny<bool>(),
          It.IsAny<CancellationToken>()))
        .ReturnsAsync(true);

    var result = await _handler.Handle(command, default);

    Assert.Equal(ResultStatus.Conflict, result.Status);
  }

  [Fact]
  public async Task Handle_ReturnsError_WhenUserNotAuthenticated()
  {
    var command = new CreateRoleCommand("Admin", "Administrator");

    _rolesRepositoryMock
        .Setup(r => r.ExistsAsync(It.IsAny<Expression<Func<Role, bool>>>(),
          It.IsAny<bool>(),
          It.IsAny<bool>(),
          It.IsAny<CancellationToken>()))
        .ReturnsAsync(false);

    _httpContextAccessorMock.Setup(x => x.HttpContext).Returns((HttpContext)null);

    var result = await _handler.Handle(command, default);

    Assert.Equal(ResultStatus.Error, result.Status);
    Assert.Contains("Current user not authenticated.", result.Errors);
  }

  [Fact]
  public async Task Handle_ReturnsNotFound_WhenUserNotFound()
  {
    var command = new CreateRoleCommand("Admin", "Administrator");

    _rolesRepositoryMock
        .Setup(r => r.ExistsAsync(It.IsAny<Expression<Func<Role, bool>>>(),
          It.IsAny<bool>(),
          It.IsAny<bool>(),
          It.IsAny<CancellationToken>()))
        .ReturnsAsync(false);

    var httpContext = new DefaultHttpContext();
    httpContext.User = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, "user-1") }));
    _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(httpContext);

    _usersRepositoryMock
        .Setup(r => r.GetUserByIdentityIdWithIdentityAndRolesAsync("user-1", It.IsAny<CancellationToken>()))
        .ReturnsAsync(Result<AppUser>.NotFound("User not found."));

    var result = await _handler.Handle(command, default);

    Assert.Equal(ResultStatus.NotFound, result.Status);
    Assert.Contains("User not found.", result.Errors);
  }

  [Fact]
  public async Task Handle_ReturnsSuccess_WhenRoleCreated()
  {
    var command = new CreateRoleCommand("Admin", "Administrator");

    _rolesRepositoryMock
        .Setup(r => r.ExistsAsync(It.IsAny<Expression<Func<Role, bool>>>(),
          It.IsAny<bool>(),
          It.IsAny<bool>(),
          It.IsAny<CancellationToken>()))
        .ReturnsAsync(false);

    var httpContext = new DefaultHttpContext();
    httpContext.User = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, "user-1") }));
    _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(httpContext);

    var appUser = AppUser.Create();
    appUser.SetIdentityId("user-1");

    _usersRepositoryMock
        .Setup(r => r.GetUserByIdentityIdWithIdentityAndRolesAsync("user-1", It.IsAny<CancellationToken>()))
        .ReturnsAsync(Result.Success(appUser));

    _rolesRepositoryMock
        .Setup(r => r.AddAsync(It.IsAny<Role>()))
        .Returns(Task.CompletedTask);

    _unitOfWorkMock
        .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
        .ReturnsAsync(1);

    _cacheServiceMock
        .Setup(c => c.RemoveAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
        .Returns(Task.CompletedTask);

    var result = await _handler.Handle(command, default);

    Assert.Equal(ResultStatus.Ok, result.Status);
    Assert.NotNull(result.Value);
    Assert.Equal("Admin", result.Value.Name);
  }
}