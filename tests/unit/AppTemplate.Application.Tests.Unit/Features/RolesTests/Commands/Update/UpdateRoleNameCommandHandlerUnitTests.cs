using AppTemplate.Application.Data.Pagination;
using AppTemplate.Application.Features.Roles.Commands.Update.UpdateRoleName;
using AppTemplate.Application.Repositories;
using AppTemplate.Application.Services.Caching;
using AppTemplate.Domain;
using AppTemplate.Domain.AppUsers;
using AppTemplate.Domain.Roles;
using Ardalis.Result;
using Microsoft.AspNetCore.Http;
using Moq;
using System.Security.Claims;

namespace AppTemplate.Application.Tests.Unit.Features.RolesTests.Commands.Update;

[Trait("Category", "Unit")]
public class UpdateRoleNameCommandHandlerUnitTests
{
  private readonly Mock<IRolesRepository> _rolesRepositoryMock = new();
  private readonly Mock<IAppUsersRepository> _usersRepositoryMock = new();
  private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock = new();
  private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
  private readonly Mock<ICacheService> _cacheServiceMock = new();
  private readonly UpdateRoleNameCommandHandler _handler;

  public UpdateRoleNameCommandHandlerUnitTests()
  {
    _handler = new UpdateRoleNameCommandHandler(
        _rolesRepositoryMock.Object,
        _usersRepositoryMock.Object,
        _httpContextAccessorMock.Object,
        _unitOfWorkMock.Object,
        _cacheServiceMock.Object);
  }

  [Fact]
  public async Task Handle_ReturnsError_WhenUserNotAuthenticated()
  {
    var command = new UpdateRoleNameCommand(Guid.NewGuid(), "NewName", "NewDisplayName");
    _httpContextAccessorMock.Setup(x => x.HttpContext).Returns((HttpContext)null);

    var result = await _handler.Handle(command, default);

    Assert.Equal(ResultStatus.Error, result.Status);
    Assert.Contains("Current user not authenticated.", result.Errors);
  }

  [Fact]
  public async Task Handle_ReturnsNotFound_WhenUserNotFound()
  {
    var command = new UpdateRoleNameCommand(Guid.NewGuid(), "NewName", "NewDisplayName");
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
    var command = new UpdateRoleNameCommand(Guid.NewGuid(), "NewName", "NewDisplayName");
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
  public async Task Handle_UpdatesRoleName_WhenValid()
  {
    var command = new UpdateRoleNameCommand(Guid.NewGuid(), "NewName", "NewDisplayName");
    var httpContext = new DefaultHttpContext();
    httpContext.User = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, "user-1") }));
    _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(httpContext);

    var appUser = AppUser.Create();
    appUser.SetIdentityId("user-1");
    _usersRepositoryMock
        .Setup(r => r.GetUserByIdentityIdWithIdentityAndRolesAsync("user-1", It.IsAny<CancellationToken>()))
        .ReturnsAsync(Result.Success(appUser));

    var role = Role.Create("OldName", "OldDisplayName", Guid.NewGuid());
    _rolesRepositoryMock
        .Setup(r => r.GetRoleByIdWithPermissionsAsync(command.RoleId, It.IsAny<CancellationToken>()))
        .ReturnsAsync(Result.Success(role));

    _rolesRepositoryMock.Setup(r => r.Update(role));
    _unitOfWorkMock.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
    _cacheServiceMock.Setup(c => c.RemoveAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

    _usersRepositoryMock
    .Setup(r => r.GetAllUsersByRoleIdWithIdentityAndRolesAsync(
        It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
    .ReturnsAsync(Result.Success(new PaginatedList<AppUser>(new List<AppUser>(), 0, 0, 1000)));

    var result = await _handler.Handle(command, default);

    Assert.Equal(ResultStatus.Ok, result.Status);
    Assert.NotNull(result.Value);
    Assert.Equal("NewName", result.Value.Name);
  }
}
