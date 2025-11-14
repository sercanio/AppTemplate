using System.Security.Claims;
using AppTemplate.Application.Features.Roles.Commands.Delete;
using AppTemplate.Application.Repositories;
using AppTemplate.Application.Services.Caching;
using AppTemplate.Domain;
using AppTemplate.Domain.AppUsers;
using AppTemplate.Domain.Roles;
using Ardalis.Result;
using Microsoft.AspNetCore.Http;
using Moq;

namespace AppTemplate.Application.Tests.Unit.Features.RolesTests.Commands.Delete;

[Trait("Category", "Unit")]
public class DeleteRoleCommandHandlerUnitTests
{
  private readonly Mock<IRolesRepository> _rolesRepositoryMock = new();
  private readonly Mock<IAppUsersRepository> _usersRepositoryMock = new();
  private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
  private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock = new();
  private readonly Mock<ICacheService> _cacheServiceMock = new();
  private readonly DeleteRoleCommandHandler _handler;

  public DeleteRoleCommandHandlerUnitTests()
  {
    _handler = new DeleteRoleCommandHandler(
        _rolesRepositoryMock.Object,
        _usersRepositoryMock.Object,
        _unitOfWorkMock.Object,
        _httpContextAccessorMock.Object,
        _cacheServiceMock.Object);
  }

  [Fact]
  public async Task Handle_ReturnsNotFound_WhenRoleNotFound()
  {
    var command = new DeleteRoleCommand(Guid.NewGuid());
    _rolesRepositoryMock
        .Setup(r => r.GetRoleByIdWithPermissionsAsync(command.RoleId, It.IsAny<CancellationToken>()))
        .ReturnsAsync(Result<Role>.NotFound("Role not found."));

    var result = await _handler.Handle(command, default);

    Assert.Equal(ResultStatus.NotFound, result.Status);
  }

  [Fact]
  public async Task Handle_ReturnsError_WhenUserNotAuthenticated()
  {
    var command = new DeleteRoleCommand(Guid.NewGuid());
    var role = new Role(command.RoleId, new("Role"), new("Role"), Guid.NewGuid(), false);

    _rolesRepositoryMock
        .Setup(r => r.GetRoleByIdWithPermissionsAsync(command.RoleId, It.IsAny<CancellationToken>()))
        .ReturnsAsync(Result.Success(role));

    _httpContextAccessorMock.Setup(x => x.HttpContext).Returns((HttpContext)null);

    var result = await _handler.Handle(command, default);

    Assert.Equal(ResultStatus.Error, result.Status);
    Assert.Contains("Current user not authenticated.", result.Errors);
  }

  [Fact]
  public async Task Handle_ReturnsNotFound_WhenUserNotFound()
  {
    var command = new DeleteRoleCommand(Guid.NewGuid());
    var role = new Role(command.RoleId, new("Role"), new("Role"), Guid.NewGuid(), false);

    _rolesRepositoryMock
        .Setup(r => r.GetRoleByIdWithPermissionsAsync(command.RoleId, It.IsAny<CancellationToken>()))
        .ReturnsAsync(Result.Success(role));

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
  public async Task Handle_DeletesRole_WhenValid()
  {
    var command = new DeleteRoleCommand(Guid.NewGuid());
    var role = new Role(command.RoleId, new("Role"), new("Role"), Guid.NewGuid(), false);

    _rolesRepositoryMock
        .Setup(r => r.GetRoleByIdWithPermissionsAsync(command.RoleId, It.IsAny<CancellationToken>()))
        .ReturnsAsync(Result.Success(role));

    var httpContext = new DefaultHttpContext();
    httpContext.User = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, "user-1") }));
    _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(httpContext);

    var appUser = AppUser.Create();
    appUser.SetIdentityId("user-1");
    _usersRepositoryMock
        .Setup(r => r.GetUserByIdentityIdWithIdentityAndRolesAsync("user-1", It.IsAny<CancellationToken>()))
        .ReturnsAsync(Result.Success(appUser));

    _unitOfWorkMock.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
    _cacheServiceMock.Setup(c => c.RemoveAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

    var result = await _handler.Handle(command, default);

    Assert.Equal(ResultStatus.Ok, result.Status);
    Assert.NotNull(result.Value);
    Assert.Equal(command.RoleId, result.Value.Id);
    Assert.Equal("Role", result.Value.Name);
  }
}
