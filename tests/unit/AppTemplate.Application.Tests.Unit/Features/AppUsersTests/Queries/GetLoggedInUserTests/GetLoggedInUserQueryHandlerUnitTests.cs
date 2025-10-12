using AppTemplate.Application.Features.AppUsers.Queries.GetLoggedInUser;
using AppTemplate.Application.Repositories;
using AppTemplate.Domain.AppUsers;
using AppTemplate.Domain.AppUsers.ValueObjects;
using AppTemplate.Domain.Roles;
using Ardalis.Result;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
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

  [Fact]
  public async Task Handle_ReturnsNotFound_WhenHttpContextIsNull()
  {
    // Arrange - HttpContext is null
    _httpContextAccessorMock.Setup(x => x.HttpContext).Returns((HttpContext?)null);

    // Act
    var result = await _handler.Handle(new GetLoggedInUserQuery(), default);

    // Assert
    Assert.Equal(ResultStatus.NotFound, result.Status);
    Assert.Null(result.Value);
    _userRepositoryMock.Verify(
      r => r.GetUserByIdentityIdWithIdentityAndRolesAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), 
      Times.Never);
  }

  [Fact]
  public async Task Handle_ReturnsNotFound_WhenUserIsNull()
  {
    // Arrange - HttpContext.User is null
    var httpContext = new DefaultHttpContext();
    httpContext.User = null;
    _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(httpContext);

    // Act
    var result = await _handler.Handle(new GetLoggedInUserQuery(), default);

    // Assert
    Assert.Equal(ResultStatus.NotFound, result.Status);
    Assert.Null(result.Value);
  }

  [Fact]
  public async Task Handle_ReturnsUserResponse_WhenUserFoundWithSingleRole()
  {
    // Arrange
    var userId = "user-123";
    var claims = new[] { new Claim(ClaimTypes.NameIdentifier, userId) };
    var httpContext = new DefaultHttpContext();
    httpContext.User = new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuth"));
    _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(httpContext);

    var identityUser = new IdentityUser
    {
      Id = userId,
      UserName = "testuser",
      Email = "test@example.com",
      EmailConfirmed = true
    };

    var role = Role.Create("Admin", "Administrator", Guid.NewGuid());
    var appUser = AppUser.Create();
    appUser.SetIdentityId(userId);
    appUser.AddRole(role);
    
    // Use reflection to set IdentityUser
    var identityUserProperty = typeof(AppUser).GetProperty("IdentityUser");
    identityUserProperty?.SetValue(appUser, identityUser);

    _userRepositoryMock
        .Setup(r => r.GetUserByIdentityIdWithIdentityAndRolesAsync(userId, It.IsAny<CancellationToken>()))
        .ReturnsAsync(Result.Success(appUser));

    // Act
    var result = await _handler.Handle(new GetLoggedInUserQuery(), default);

    // Assert
    Assert.Equal(ResultStatus.Ok, result.Status);
    Assert.NotNull(result.Value);
    Assert.Equal("test@example.com", result.Value.Email);
    Assert.Equal("testuser", result.Value.UserName);
    Assert.True(result.Value.EmailConfirmed);
    Assert.Single(result.Value.Roles);
    Assert.Equal("Admin", result.Value.Roles.First().Name);
  }

  [Fact]
  public async Task Handle_ReturnsUserResponse_WhenUserFoundWithMultipleRoles()
  {
    // Arrange
    var userId = "user-456";
    var claims = new[] { new Claim(ClaimTypes.NameIdentifier, userId) };
    var httpContext = new DefaultHttpContext();
    httpContext.User = new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuth"));
    _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(httpContext);

    var identityUser = new IdentityUser
    {
      Id = userId,
      UserName = "multiroleuser",
      Email = "multi@example.com",
      EmailConfirmed = false
    };

    var role1 = Role.Create("Admin", "Administrator", Guid.NewGuid());
    var role2 = Role.Create("User", "Standard User", Guid.NewGuid());
    
    var appUser = AppUser.Create();
    appUser.SetIdentityId(userId);
    appUser.AddRole(role1);
    appUser.AddRole(role2);
    
    var identityUserProperty = typeof(AppUser).GetProperty("IdentityUser");
    identityUserProperty?.SetValue(appUser, identityUser);

    _userRepositoryMock
        .Setup(r => r.GetUserByIdentityIdWithIdentityAndRolesAsync(userId, It.IsAny<CancellationToken>()))
        .ReturnsAsync(Result.Success(appUser));

    // Act
    var result = await _handler.Handle(new GetLoggedInUserQuery(), default);

    // Assert
    Assert.Equal(ResultStatus.Ok, result.Status);
    Assert.NotNull(result.Value);
    Assert.Equal("multi@example.com", result.Value.Email);
    Assert.Equal("multiroleuser", result.Value.UserName);
    Assert.False(result.Value.EmailConfirmed);
    Assert.Equal(2, result.Value.Roles.Count);
  }

  [Fact]
  public async Task Handle_ReturnsUserResponse_WithNotificationPreferences()
  {
    // Arrange
    var userId = "user-789";
    var claims = new[] { new Claim(ClaimTypes.NameIdentifier, userId) };
    var httpContext = new DefaultHttpContext();
    httpContext.User = new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuth"));
    _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(httpContext);

    var identityUser = new IdentityUser
    {
      Id = userId,
      UserName = "notifuser",
      Email = "notif@example.com",
      EmailConfirmed = true
    };

    var appUser = AppUser.Create();
    appUser.SetIdentityId(userId);
    
    // Set custom notification preferences
    var notificationPreference = new NotificationPreference(false, true, false);
    appUser.SetNotificationPreference(notificationPreference);
    
    var identityUserProperty = typeof(AppUser).GetProperty("IdentityUser");
    identityUserProperty?.SetValue(appUser, identityUser);

    _userRepositoryMock
        .Setup(r => r.GetUserByIdentityIdWithIdentityAndRolesAsync(userId, It.IsAny<CancellationToken>()))
        .ReturnsAsync(Result.Success(appUser));

    // Act
    var result = await _handler.Handle(new GetLoggedInUserQuery(), default);

    // Assert
    Assert.Equal(ResultStatus.Ok, result.Status);
    Assert.NotNull(result.Value);
    Assert.NotNull(result.Value.NotificationPreference);
    Assert.False(result.Value.NotificationPreference.IsInAppNotificationEnabled);
    Assert.True(result.Value.NotificationPreference.IsEmailNotificationEnabled);
    Assert.False(result.Value.NotificationPreference.IsPushNotificationEnabled);
  }

  [Fact]
  public async Task Handle_FiltersOutDeletedRoles()
  {
    // Arrange
    var userId = "user-deleted-roles";
    var claims = new[] { new Claim(ClaimTypes.NameIdentifier, userId) };
    var httpContext = new DefaultHttpContext();
    httpContext.User = new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuth"));
    _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(httpContext);

    var identityUser = new IdentityUser
    {
      Id = userId,
      UserName = "deletedroleuser",
      Email = "deleted@example.com",
      EmailConfirmed = true
    };

    var activeRole = Role.Create("ActiveRole", "Active Role", Guid.NewGuid());
    var deletedRole = Role.Create("DeletedRole", "Deleted Role", Guid.NewGuid());
    
    // Soft delete the role using the domain method
    Role.Delete(deletedRole, deletedById: Guid.NewGuid());
    
    var appUser = AppUser.Create();
    appUser.SetIdentityId(userId);
    appUser.AddRole(activeRole);
    appUser.AddRole(deletedRole);
    
    var identityUserProperty = typeof(AppUser).GetProperty("IdentityUser");
    identityUserProperty?.SetValue(appUser, identityUser);

    _userRepositoryMock
        .Setup(r => r.GetUserByIdentityIdWithIdentityAndRolesAsync(userId, It.IsAny<CancellationToken>()))
        .ReturnsAsync(Result.Success(appUser));

    // Act
    var result = await _handler.Handle(new GetLoggedInUserQuery(), default);

    // Assert
    Assert.Equal(ResultStatus.Ok, result.Status);
    Assert.NotNull(result.Value);
    Assert.Single(result.Value.Roles); // Only active role should be included
    Assert.Equal("ActiveRole", result.Value.Roles.First().Name);
  }

  [Fact]
  public async Task Handle_ReturnsEmptyRolesList_WhenAllRolesAreDeleted()
  {
    // Arrange
    var userId = "user-all-deleted";
    var claims = new[] { new Claim(ClaimTypes.NameIdentifier, userId) };
    var httpContext = new DefaultHttpContext();
    httpContext.User = new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuth"));
    _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(httpContext);

    var identityUser = new IdentityUser
    {
      Id = userId,
      UserName = "norolesuser",
      Email = "noroles@example.com",
      EmailConfirmed = true
    };

    var deletedRole = Role.Create("DeletedRole", "Deleted Role", Guid.NewGuid());
    // Soft delete the role using the domain method
    Role.Delete(deletedRole, deletedById: Guid.NewGuid());
    
    var appUser = AppUser.Create();
    appUser.SetIdentityId(userId);
    appUser.AddRole(deletedRole);
    
    var identityUserProperty = typeof(AppUser).GetProperty("IdentityUser");
    identityUserProperty?.SetValue(appUser, identityUser);

    _userRepositoryMock
        .Setup(r => r.GetUserByIdentityIdWithIdentityAndRolesAsync(userId, It.IsAny<CancellationToken>()))
        .ReturnsAsync(Result.Success(appUser));

    // Act
    var result = await _handler.Handle(new GetLoggedInUserQuery(), default);

    // Assert
    Assert.Equal(ResultStatus.Ok, result.Status);
    Assert.NotNull(result.Value);
    Assert.Empty(result.Value.Roles);
  }
}