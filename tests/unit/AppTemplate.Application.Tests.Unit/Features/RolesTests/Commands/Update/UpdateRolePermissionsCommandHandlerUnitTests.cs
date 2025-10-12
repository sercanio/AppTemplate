using AppTemplate.Application.Data.Pagination;
using AppTemplate.Application.Enums;
using AppTemplate.Application.Features.Roles.Commands.Update.UpdatePermissions;
using AppTemplate.Application.Repositories;
using AppTemplate.Application.Services.Caching;
using AppTemplate.Domain;
using AppTemplate.Domain.AppUsers;
using AppTemplate.Domain.Roles;
using AppTemplate.Domain.Roles.ValueObjects;
using Ardalis.Result;
using Microsoft.AspNetCore.Http;
using Moq;
using System.Security.Claims;

namespace AppTemplate.Application.Tests.Unit.Features.RolesTests.Commands.Update;

[Trait("Category", "Unit")]
public class UpdateRolePermissionsCommandHandlerUnitTests
{
  private readonly Mock<IRolesRepository> _rolesRepositoryMock = new();
  private readonly Mock<IPermissionsRepository> _permissionsRepositoryMock = new();
  private readonly Mock<IAppUsersRepository> _usersRepositoryMock = new();
  private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
  private readonly Mock<ICacheService> _cacheServiceMock = new();
  private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock = new();
  private readonly UpdateRolePermissionsCommandHandler _handler;

  public UpdateRolePermissionsCommandHandlerUnitTests()
  {
    _handler = new UpdateRolePermissionsCommandHandler(
        _rolesRepositoryMock.Object,
        _permissionsRepositoryMock.Object,
        _usersRepositoryMock.Object,
        _unitOfWorkMock.Object,
        _cacheServiceMock.Object,
        _httpContextAccessorMock.Object);
  }

  [Fact]
  public async Task Handle_ReturnsError_WhenUserNotAuthenticated()
  {
    var command = new UpdateRolePermissionsCommand(Guid.NewGuid(), Guid.NewGuid(), Operation.Add);
    _httpContextAccessorMock.Setup(x => x.HttpContext).Returns((HttpContext)null);

    var result = await _handler.Handle(command, default);

    Assert.Equal(ResultStatus.Error, result.Status);
    Assert.Contains("Current user not authenticated.", result.Errors);
  }

  [Fact]
  public async Task Handle_ReturnsNotFound_WhenUserNotFound()
  {
    var command = new UpdateRolePermissionsCommand(Guid.NewGuid(), Guid.NewGuid(), Operation.Add);
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
  public async Task Handle_ReturnsNotFound_WhenRoleNotFound()
  {
    var command = new UpdateRolePermissionsCommand(Guid.NewGuid(), Guid.NewGuid(), Operation.Add);
    var httpContext = new DefaultHttpContext();
    httpContext.User = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, "user-1") }));
    _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(httpContext);

    var appUser = AppUser.Create();
    appUser.SetIdentityId("user-1");
    _usersRepositoryMock
        .Setup(r => r.GetUserByIdentityIdWithIdentityAndRolesAsync("user-1", It.IsAny<CancellationToken>()))
        .ReturnsAsync(Result.Success(appUser));

    _rolesRepositoryMock
        .Setup(r => r.GetRoleByIdWithPermissionsAsync(command.RoleId, It.IsAny<CancellationToken>()))
        .ReturnsAsync(Result<Role>.NotFound("Role not found."));

    var result = await _handler.Handle(command, default);

    Assert.Equal(ResultStatus.NotFound, result.Status);
  }

  [Fact]
  public async Task Handle_ReturnsNotFound_WhenPermissionToAddNotFound()
  {
    var command = new UpdateRolePermissionsCommand(Guid.NewGuid(), Guid.NewGuid(), Operation.Add);
    var httpContext = new DefaultHttpContext();
    httpContext.User = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, "user-1") }));
    _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(httpContext);

    var appUser = AppUser.Create();
    appUser.SetIdentityId("user-1");
    _usersRepositoryMock
        .Setup(r => r.GetUserByIdentityIdWithIdentityAndRolesAsync("user-1", It.IsAny<CancellationToken>()))
        .ReturnsAsync(Result.Success(appUser));

    var role = new Role(command.RoleId, new RoleName("Role"), new RoleName("Role"), Guid.NewGuid(), false);
    _rolesRepositoryMock
        .Setup(r => r.GetRoleByIdWithPermissionsAsync(command.RoleId, It.IsAny<CancellationToken>()))
        .ReturnsAsync(Result.Success(role));

    _permissionsRepositoryMock
        .Setup(r => r.GetAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Permission, bool>>>(), false, null, true, It.IsAny<CancellationToken>()))
        .ReturnsAsync((Permission)null);

    var result = await _handler.Handle(command, default);

    Assert.Equal(ResultStatus.NotFound, result.Status);
    Assert.Contains("Permission with ID", result.Errors.First());
  }

  [Fact]
  public async Task Handle_AddsPermission_WhenValid()
  {
    var command = new UpdateRolePermissionsCommand(Guid.NewGuid(), Guid.NewGuid(), Operation.Add);
    var httpContext = new DefaultHttpContext();
    httpContext.User = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, "user-1") }));
    _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(httpContext);

    var appUser = AppUser.Create();
    appUser.SetIdentityId("user-1");
    _usersRepositoryMock
        .Setup(r => r.GetUserByIdentityIdWithIdentityAndRolesAsync("user-1", It.IsAny<CancellationToken>()))
        .ReturnsAsync(Result.Success(appUser));

    var role = new Role(command.RoleId, new RoleName("Role"), new RoleName("Role"), Guid.NewGuid(), false);
    _rolesRepositoryMock
        .Setup(r => r.GetRoleByIdWithPermissionsAsync(command.RoleId, It.IsAny<CancellationToken>()))
        .ReturnsAsync(Result.Success(role));

    var permission = new Permission(Guid.NewGuid(), "Perm", "Feature");
    _permissionsRepositoryMock
        .Setup(r => r.GetAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Permission, bool>>>(), false, null, true, It.IsAny<CancellationToken>()))
        .ReturnsAsync(permission);

    _rolesRepositoryMock.Setup(r => r.Update(role, It.IsAny<CancellationToken>()));
    _unitOfWorkMock.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
    _cacheServiceMock.Setup(c => c.RemoveAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

    _usersRepositoryMock
        .Setup(r => r.GetAllUsersByRoleIdWithIdentityAndRolesAsync(
            It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
        .ReturnsAsync(Result.Success(new PaginatedList<AppUser>(new List<AppUser>(), 0, 0, 1000)));

    var result = await _handler.Handle(command, default);

    Assert.Equal(ResultStatus.Ok, result.Status);
    Assert.NotNull(result.Value);
    Assert.Equal(command.RoleId, result.Value.RoleId);
    Assert.Equal(command.PermissionId, result.Value.PermissionId);
  }
}
