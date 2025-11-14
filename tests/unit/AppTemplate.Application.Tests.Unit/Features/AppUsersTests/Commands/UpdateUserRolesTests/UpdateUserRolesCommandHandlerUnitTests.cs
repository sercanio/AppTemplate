using System.Linq.Expressions;
using System.Security.Claims;
using AppTemplate.Application.Enums;
using AppTemplate.Application.Features.AppUsers.Commands.Update.UpdateUserRoles;
using AppTemplate.Application.Repositories;
using AppTemplate.Application.Services.Caching;
using AppTemplate.Application.Services.Roles;
using AppTemplate.Domain;
using AppTemplate.Domain.AppUsers;
using AppTemplate.Domain.Roles;
using Ardalis.Result;
using Microsoft.AspNetCore.Http;
using Moq;

namespace AppTemplate.Application.Tests.Unit.Features.AppUsersTests.Commands.UpdateUserRolesTests;

[Trait("Category", "Unit")]
public class UpdateUserRolesCommandHandlerUnitTests
{
  private readonly Mock<IAppUsersRepository> _userRepositoryMock = new();
  private readonly Mock<IRolesService> _rolesServiceMock = new();
  private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
  private readonly Mock<ICacheService> _cacheServiceMock = new();
  private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock = new();
  private readonly UpdateUserRolesCommandHandler _handler;

  public UpdateUserRolesCommandHandlerUnitTests()
  {
    _handler = new UpdateUserRolesCommandHandler(
        _userRepositoryMock.Object,
        _rolesServiceMock.Object,
        _unitOfWorkMock.Object,
        _cacheServiceMock.Object,
        _httpContextAccessorMock.Object);
  }

  [Fact]
  public async Task Handle_ReturnsNotFound_WhenUserNotFound()
  {
    var command = new UpdateUserRolesCommand(Guid.NewGuid(), Operation.Add, Guid.NewGuid());

    _userRepositoryMock
        .Setup(r => r.GetUserWithRolesAndIdentityByIdAsync(command.UserId, It.IsAny<CancellationToken>()))
        .ReturnsAsync(Result<AppUser>.NotFound("User not found"));

    var result = await _handler.Handle(command, default);

    Assert.Equal(ResultStatus.NotFound, result.Status);
  }

  [Fact]
  public async Task Handle_ReturnsNotFound_WhenRoleNotFound()
  {
    var userId = Guid.NewGuid();
    var roleId = Guid.NewGuid();
    var command = new UpdateUserRolesCommand(userId, Operation.Add, roleId);

    var appUser = AppUser.Create();
    _userRepositoryMock
        .Setup(r => r.GetUserWithRolesAndIdentityByIdAsync(userId, It.IsAny<CancellationToken>()))
        .ReturnsAsync(Result.Success(appUser));

    _rolesServiceMock
        .Setup(s => s.GetAsync(
            It.IsAny<Expression<Func<Role, bool>>>(),
            It.IsAny<bool>(),
            It.IsAny<Func<IQueryable<Role>, IQueryable<Role>>>(),
            It.IsAny<bool>(),
            It.IsAny<CancellationToken>()))
        .ReturnsAsync((Role?)null);

    var result = await _handler.Handle(command, default);

    Assert.Equal(ResultStatus.NotFound, result.Status);
  }

  [Fact]
  public async Task Handle_ReturnsError_WhenHttpContextIsNull()
  {
    // Arrange
    var userId = Guid.NewGuid();
    var roleId = Guid.NewGuid();
    var command = new UpdateUserRolesCommand(userId, Operation.Add, roleId);

    var appUser = AppUser.Create();
    var role = Role.Create("TestRole", "Test Role", Guid.NewGuid());

    _userRepositoryMock
        .Setup(r => r.GetUserWithRolesAndIdentityByIdAsync(userId, It.IsAny<CancellationToken>()))
        .ReturnsAsync(Result.Success(appUser));

    _rolesServiceMock
        .Setup(s => s.GetAsync(
            It.IsAny<Expression<Func<Role, bool>>>(),
            It.IsAny<bool>(),
            It.IsAny<Func<IQueryable<Role>, IQueryable<Role>>>(),
            It.IsAny<bool>(),
            It.IsAny<CancellationToken>()))
        .ReturnsAsync(role);

    _httpContextAccessorMock.Setup(x => x.HttpContext).Returns((HttpContext?)null);

    // Act
    var result = await _handler.Handle(command, default);

    // Assert
    Assert.Equal(ResultStatus.Error, result.Status);
    Assert.Contains("Current user not authenticated.", result.Errors);
  }

  [Fact]
  public async Task Handle_ReturnsError_WhenUserClaimsAreNull()
  {
    // Arrange
    var userId = Guid.NewGuid();
    var roleId = Guid.NewGuid();
    var command = new UpdateUserRolesCommand(userId, Operation.Add, roleId);

    var appUser = AppUser.Create();
    var role = Role.Create("TestRole", "Test Role", Guid.NewGuid());

    _userRepositoryMock
        .Setup(r => r.GetUserWithRolesAndIdentityByIdAsync(userId, It.IsAny<CancellationToken>()))
        .ReturnsAsync(Result.Success(appUser));

    _rolesServiceMock
        .Setup(s => s.GetAsync(
            It.IsAny<Expression<Func<Role, bool>>>(),
            It.IsAny<bool>(),
            It.IsAny<Func<IQueryable<Role>, IQueryable<Role>>>(),
            It.IsAny<bool>(),
            It.IsAny<CancellationToken>()))
        .ReturnsAsync(role);

    var httpContext = new DefaultHttpContext();
    httpContext.User = null;
    _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(httpContext);

    // Act
    var result = await _handler.Handle(command, default);

    // Assert
    Assert.Equal(ResultStatus.Error, result.Status);
    Assert.Contains("Current user not authenticated.", result.Errors);
  }

  [Fact]
  public async Task Handle_ReturnsError_WhenUserIdClaimIsEmpty()
  {
    // Arrange
    var userId = Guid.NewGuid();
    var roleId = Guid.NewGuid();
    var command = new UpdateUserRolesCommand(userId, Operation.Add, roleId);

    var appUser = AppUser.Create();
    var role = Role.Create("TestRole", "Test Role", Guid.NewGuid());

    _userRepositoryMock
        .Setup(r => r.GetUserWithRolesAndIdentityByIdAsync(userId, It.IsAny<CancellationToken>()))
        .ReturnsAsync(Result.Success(appUser));

    _rolesServiceMock
        .Setup(s => s.GetAsync(
            It.IsAny<Expression<Func<Role, bool>>>(),
            It.IsAny<bool>(),
            It.IsAny<Func<IQueryable<Role>, IQueryable<Role>>>(),
            It.IsAny<bool>(),
            It.IsAny<CancellationToken>()))
        .ReturnsAsync(role);

    var httpContext = new DefaultHttpContext();
    httpContext.User = new ClaimsPrincipal(new ClaimsIdentity());
    _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(httpContext);

    // Act
    var result = await _handler.Handle(command, default);

    // Assert
    Assert.Equal(ResultStatus.Error, result.Status);
    Assert.Contains("Current user not authenticated.", result.Errors);
  }

  [Fact]
  public async Task Handle_AddsRole_WhenOperationIsAdd()
  {
    // Arrange
    var actorId = "actor-123";

    // Create entities first to get their actual IDs
    var appUser = AppUser.Create();
    appUser.SetIdentityId("user-123");
    var userId = appUser.Id; // Use the actual user ID

    var role = Role.Create("TestRole", "Test Role", Guid.NewGuid());
    var roleId = role.Id; // Use the actual role ID

    var actorUser = AppUser.Create();
    actorUser.SetIdentityId(actorId);

    // Now create the command with actual IDs
    var command = new UpdateUserRolesCommand(userId, Operation.Add, roleId);

    _userRepositoryMock
        .Setup(r => r.GetUserWithRolesAndIdentityByIdAsync(userId, It.IsAny<CancellationToken>()))
        .ReturnsAsync(Result.Success(appUser));

    _userRepositoryMock
        .Setup(r => r.GetUserWithRolesAndIdentityByIdentityIdAsync(actorId, It.IsAny<CancellationToken>()))
        .ReturnsAsync(Result.Success(actorUser));

    _rolesServiceMock
        .Setup(s => s.GetAsync(
            It.IsAny<Expression<Func<Role, bool>>>(),
            It.IsAny<bool>(),
            It.IsAny<Func<IQueryable<Role>, IQueryable<Role>>>(),
            It.IsAny<bool>(),
            It.IsAny<CancellationToken>()))
        .ReturnsAsync(role);

    var claims = new[] { new Claim(ClaimTypes.NameIdentifier, actorId) };
    var httpContext = new DefaultHttpContext();
    httpContext.User = new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuth"));
    _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(httpContext);

    _unitOfWorkMock.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
        .ReturnsAsync(1);

    // Act
    var result = await _handler.Handle(command, default);

    // Assert
    Assert.Equal(ResultStatus.Ok, result.Status);
    Assert.NotNull(result.Value);
    Assert.Equal(roleId, result.Value.RoleId);
    Assert.Equal(userId, result.Value.UserId);

    _userRepositoryMock.Verify(r => r.Update(It.Is<AppUser>(u => u.Id == userId), It.IsAny<CancellationToken>()), Times.Once);
    _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    _cacheServiceMock.Verify(c => c.RemoveAsync($"users-{userId}", It.IsAny<CancellationToken>()), Times.Once);
    _cacheServiceMock.Verify(c => c.RemoveAsync($"auth:roles-user-123", It.IsAny<CancellationToken>()), Times.Once);
    _cacheServiceMock.Verify(c => c.RemoveAsync($"auth:permissions-user-123", It.IsAny<CancellationToken>()), Times.Once);
  }

  [Fact]
  public async Task Handle_RemovesRole_WhenOperationIsRemove()
  {
    // Arrange
    var actorId = "actor-123";

    // Create entities first
    var appUser = AppUser.Create();
    appUser.SetIdentityId("user-123");
    var userId = appUser.Id; // Use actual ID

    var role = Role.Create("TestRole", "Test Role", Guid.NewGuid());
    var roleId = role.Id; // Use actual ID
    appUser.AddRole(role);

    var actorUser = AppUser.Create();
    actorUser.SetIdentityId(actorId);

    // Create command with actual IDs
    var command = new UpdateUserRolesCommand(userId, Operation.Remove, roleId);

    _userRepositoryMock
        .Setup(r => r.GetUserWithRolesAndIdentityByIdAsync(userId, It.IsAny<CancellationToken>()))
        .ReturnsAsync(Result.Success(appUser));

    _userRepositoryMock
        .Setup(r => r.GetUserWithRolesAndIdentityByIdentityIdAsync(actorId, It.IsAny<CancellationToken>()))
        .ReturnsAsync(Result.Success(actorUser));

    _rolesServiceMock
        .Setup(s => s.GetAsync(
            It.IsAny<Expression<Func<Role, bool>>>(),
            It.IsAny<bool>(),
            It.IsAny<Func<IQueryable<Role>, IQueryable<Role>>>(),
            It.IsAny<bool>(),
            It.IsAny<CancellationToken>()))
        .ReturnsAsync(role);

    var claims = new[] { new Claim(ClaimTypes.NameIdentifier, actorId) };
    var httpContext = new DefaultHttpContext();
    httpContext.User = new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuth"));
    _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(httpContext);

    _unitOfWorkMock.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
        .ReturnsAsync(1);

    // Act
    var result = await _handler.Handle(command, default);

    // Assert
    Assert.Equal(ResultStatus.Ok, result.Status);
    Assert.NotNull(result.Value);
    Assert.Equal(roleId, result.Value.RoleId);
    Assert.Equal(userId, result.Value.UserId);

    _userRepositoryMock.Verify(r => r.Update(It.Is<AppUser>(u => u.Id == userId), It.IsAny<CancellationToken>()), Times.Once);
    _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    _cacheServiceMock.Verify(c => c.RemoveAsync($"users-{userId}", It.IsAny<CancellationToken>()), Times.Once);
    _cacheServiceMock.Verify(c => c.RemoveAsync($"auth:roles-user-123", It.IsAny<CancellationToken>()), Times.Once);
    _cacheServiceMock.Verify(c => c.RemoveAsync($"auth:permissions-user-123", It.IsAny<CancellationToken>()), Times.Once);
  }

  [Fact]
  public async Task Handle_InvalidatesCacheCorrectly()
  {
    // Arrange
    var actorId = "actor-123";
    var identityId = "user-identity-456";

    // Create entities first
    var appUser = AppUser.Create();
    appUser.SetIdentityId(identityId);
    var userId = appUser.Id; // Use actual ID

    var role = Role.Create("TestRole", "Test Role", Guid.NewGuid());
    var roleId = role.Id; // Use actual ID

    var actorUser = AppUser.Create();
    actorUser.SetIdentityId(actorId);

    // Create command with actual IDs
    var command = new UpdateUserRolesCommand(userId, Operation.Add, roleId);

    _userRepositoryMock
        .Setup(r => r.GetUserWithRolesAndIdentityByIdAsync(userId, It.IsAny<CancellationToken>()))
        .ReturnsAsync(Result.Success(appUser));

    _userRepositoryMock
        .Setup(r => r.GetUserWithRolesAndIdentityByIdentityIdAsync(actorId, It.IsAny<CancellationToken>()))
        .ReturnsAsync(Result.Success(actorUser));

    _rolesServiceMock
        .Setup(s => s.GetAsync(
            It.IsAny<Expression<Func<Role, bool>>>(),
            It.IsAny<bool>(),
            It.IsAny<Func<IQueryable<Role>, IQueryable<Role>>>(),
            It.IsAny<bool>(),
            It.IsAny<CancellationToken>()))
        .ReturnsAsync(role);

    var claims = new[] { new Claim(ClaimTypes.NameIdentifier, actorId) };
    var httpContext = new DefaultHttpContext();
    httpContext.User = new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuth"));
    _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(httpContext);

    _unitOfWorkMock.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
        .ReturnsAsync(1);

    // Act
    var result = await _handler.Handle(command, default);

    // Assert
    _cacheServiceMock.Verify(c => c.RemoveAsync($"users-{userId}", It.IsAny<CancellationToken>()), Times.Once);
    _cacheServiceMock.Verify(c => c.RemoveAsync($"auth:roles-{identityId}", It.IsAny<CancellationToken>()), Times.Once);
    _cacheServiceMock.Verify(c => c.RemoveAsync($"auth:permissions-{identityId}", It.IsAny<CancellationToken>()), Times.Once);
  }

  [Fact]
  public async Task Handle_ThrowsArgumentOutOfRangeException_WhenInvalidOperation()
  {
    // Arrange
    var actorId = "actor-123";

    var appUser = AppUser.Create();
    appUser.SetIdentityId("user-123");
    var userId = appUser.Id;

    var role = Role.Create("TestRole", "Test Role", Guid.NewGuid());
    var roleId = role.Id;

    var actorUser = AppUser.Create();
    actorUser.SetIdentityId(actorId);

    // Create command with an invalid operation value (cast from an invalid int)
    var command = new UpdateUserRolesCommand(userId, (Operation)999, roleId);

    _userRepositoryMock
        .Setup(r => r.GetUserWithRolesAndIdentityByIdAsync(userId, It.IsAny<CancellationToken>()))
        .ReturnsAsync(Result.Success(appUser));

    _userRepositoryMock
        .Setup(r => r.GetUserWithRolesAndIdentityByIdentityIdAsync(actorId, It.IsAny<CancellationToken>()))
        .ReturnsAsync(Result.Success(actorUser));

    _rolesServiceMock
        .Setup(s => s.GetAsync(
            It.IsAny<Expression<Func<Role, bool>>>(),
            It.IsAny<bool>(),
            It.IsAny<Func<IQueryable<Role>, IQueryable<Role>>>(),
            It.IsAny<bool>(),
            It.IsAny<CancellationToken>()))
        .ReturnsAsync(role);

    var claims = new[] { new Claim(ClaimTypes.NameIdentifier, actorId) };
    var httpContext = new DefaultHttpContext();
    httpContext.User = new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuth"));
    _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(httpContext);

    // Act & Assert
    await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => _handler.Handle(command, default));
  }
}