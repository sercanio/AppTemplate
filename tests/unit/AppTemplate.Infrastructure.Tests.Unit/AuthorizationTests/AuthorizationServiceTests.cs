using AppTemplate.Application.Services.Authorization;
using AppTemplate.Application.Services.Caching;
using AppTemplate.Application.Services.Clock;
using AppTemplate.Domain.AppUsers;
using AppTemplate.Domain.Roles;
using AppTemplate.Infrastructure.Authorization;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace AppTemplate.Infrastructure.Tests.Unit.AuthorizationTests;

[Trait("Category", "Unit")]
public class AuthorizationServiceTests : IDisposable
{
  private readonly ApplicationDbContext _context;
  private readonly Mock<ICacheService> _cacheServiceMock;
  private readonly AuthorizationService _authorizationService;

  public AuthorizationServiceTests()
  {
    var options = new DbContextOptionsBuilder<ApplicationDbContext>()
        .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
        .Options;

    var dateTimeProvider = new Mock<IDateTimeProvider>();
    dateTimeProvider.Setup(x => x.UtcNow).Returns(DateTime.UtcNow);

    _context = new ApplicationDbContext(options, dateTimeProvider.Object);
    _cacheServiceMock = new Mock<ICacheService>();
    _authorizationService = new AuthorizationService(_context, _cacheServiceMock.Object);
  }

  #region GetRolesForUserAsync Tests

  [Fact]
  public async Task GetRolesForUserAsync_ReturnsCachedRoles_IfPresent()
  {
    // Arrange
    var identityId = "test-id";
    var expectedRoles = new UserRolesResponse
    {
      UserId = Guid.NewGuid(),
      Roles = new List<Role>()
    };

    _cacheServiceMock
        .Setup(c => c.GetAsync<UserRolesResponse>($"auth:roles-{identityId}", default))
        .ReturnsAsync(expectedRoles);

    // Act
    var result = await _authorizationService.GetRolesForUserAsync(identityId);

    // Assert
    Assert.Equal(expectedRoles, result);
    Assert.Equal(expectedRoles.UserId, result.UserId);
    _cacheServiceMock.Verify(
        c => c.GetAsync<UserRolesResponse>($"auth:roles-{identityId}", default),
        Times.Once);
    _cacheServiceMock.Verify(
        c => c.SetAsync(It.IsAny<string>(), It.IsAny<UserRolesResponse>(), It.IsAny<TimeSpan?>(), default),
        Times.Never);
  }

  [Fact]
  public async Task GetRolesForUserAsync_FetchesFromDatabase_WhenCacheMiss()
  {
    // Arrange
    var identityId = "identity-123";
    var userId = Guid.NewGuid();

    var appUser = AppUser.Create();
    appUser.SetIdentityId(identityId);
    appUser.AddRole(Role.DefaultRole);
    appUser.AddRole(Role.Admin);

    _context.AppUsers.Add(appUser);
    await _context.SaveChangesAsync();

    _cacheServiceMock
        .Setup(c => c.GetAsync<UserRolesResponse>($"auth:roles-{identityId}", default))
        .ReturnsAsync((UserRolesResponse?)null);

    // Act
    var result = await _authorizationService.GetRolesForUserAsync(identityId);

    // Assert
    Assert.NotNull(result);
    Assert.Equal(appUser.Id, result.UserId);
    Assert.Equal(2, result.Roles.Count);
    Assert.Contains(result.Roles, r => r.Name.Value == "Registered");
    Assert.Contains(result.Roles, r => r.Name.Value == "Admin");

    _cacheServiceMock.Verify(
        c => c.GetAsync<UserRolesResponse>($"auth:roles-{identityId}", default),
        Times.Once);
    _cacheServiceMock.Verify(
        c => c.SetAsync($"auth:roles-{identityId}", It.IsAny<UserRolesResponse>(), null, default),
        Times.Once);
  }

  [Fact]
  public async Task GetRolesForUserAsync_ExcludesDeletedRoles()
  {
    // Arrange
    var identityId = "identity-deleted-roles";

    var appUser = AppUser.Create();
    appUser.SetIdentityId(identityId);

    // Add active role
    var activeRole = Role.Create("ActiveRole", "Active Role", appUser.Id);
    appUser.AddRole(activeRole);

    // Add deleted role
    var deletedRole = Role.Create("DeletedRole", "Deleted Role", appUser.Id);
    Role.Delete(deletedRole, appUser.Id);
    appUser.AddRole(deletedRole);

    _context.AppUsers.Add(appUser);
    await _context.SaveChangesAsync();

    _cacheServiceMock
        .Setup(c => c.GetAsync<UserRolesResponse>($"auth:roles-{identityId}", default))
        .ReturnsAsync((UserRolesResponse?)null);

    // Act
    var result = await _authorizationService.GetRolesForUserAsync(identityId);

    // Assert
    Assert.NotNull(result);
    Assert.Single(result.Roles);
    Assert.Contains(result.Roles, r => r.Name.Value == "ActiveRole");
    Assert.DoesNotContain(result.Roles, r => r.Name.Value == "DeletedRole");
  }

  [Fact]
  public async Task GetRolesForUserAsync_WithUserHavingNoRoles_ReturnsEmptyRolesList()
  {
    // Arrange
    var identityId = "identity-no-roles";

    var appUser = AppUser.CreateWithoutRolesForSeeding();
    appUser.SetIdentityId(identityId);

    _context.AppUsers.Add(appUser);
    await _context.SaveChangesAsync();

    _cacheServiceMock
        .Setup(c => c.GetAsync<UserRolesResponse>($"auth:roles-{identityId}", default))
        .ReturnsAsync((UserRolesResponse?)null);

    // Act
    var result = await _authorizationService.GetRolesForUserAsync(identityId);

    // Assert
    Assert.NotNull(result);
    Assert.Empty(result.Roles);
  }

  [Fact]
  public async Task GetRolesForUserAsync_CachesResultAfterDatabaseFetch()
  {
    // Arrange
    var identityId = "identity-cache-test";

    var appUser = AppUser.Create();
    appUser.SetIdentityId(identityId);
    appUser.AddRole(Role.DefaultRole);

    _context.AppUsers.Add(appUser);
    await _context.SaveChangesAsync();

    _cacheServiceMock
        .Setup(c => c.GetAsync<UserRolesResponse>($"auth:roles-{identityId}", default))
        .ReturnsAsync((UserRolesResponse?)null);

    UserRolesResponse? cachedValue = null;
    _cacheServiceMock
        .Setup(c => c.SetAsync(
            $"auth:roles-{identityId}",
            It.IsAny<UserRolesResponse>(),
            null,
            default))
        .Callback<string, UserRolesResponse, TimeSpan?, CancellationToken>((key, value, expiration, ct) =>
        {
          cachedValue = value;
        })
        .Returns(Task.CompletedTask);

    // Act
    var result = await _authorizationService.GetRolesForUserAsync(identityId);

    // Assert
    Assert.NotNull(cachedValue);
    Assert.Equal(result.UserId, cachedValue.UserId);
    Assert.Equal(result.Roles.Count, cachedValue.Roles.Count);
  }

  #endregion

  #region GetPermissionsForUserAsync Tests

  [Fact]
  public async Task GetPermissionsForUserAsync_ReturnsCachedPermissions_IfPresent()
  {
    // Arrange
    var identityId = "perm-cached-id";
    var expectedPermissions = new HashSet<string> { "users:read", "users:create", "roles:read" };

    _cacheServiceMock
        .Setup(c => c.GetAsync<HashSet<string>>($"auth:permissions-{identityId}", default))
        .ReturnsAsync(expectedPermissions);

    // Act
    var result = await _authorizationService.GetPermissionsForUserAsync(identityId);

    // Assert
    Assert.Equal(expectedPermissions, result);
    Assert.Equal(3, result.Count);
    Assert.Contains("users:read", result);
    Assert.Contains("users:create", result);
    Assert.Contains("roles:read", result);

    _cacheServiceMock.Verify(
        c => c.GetAsync<HashSet<string>>($"auth:permissions-{identityId}", default),
        Times.Once);
    _cacheServiceMock.Verify(
        c => c.SetAsync(It.IsAny<string>(), It.IsAny<HashSet<string>>(), It.IsAny<TimeSpan?>(), default),
        Times.Never);
  }

  [Fact]
  public async Task GetPermissionsForUserAsync_FetchesFromDatabase_WhenCacheMiss()
  {
    // Arrange
    var identityId = "perm-identity-123";

    var appUser = AppUser.Create();
    appUser.SetIdentityId(identityId);

    var role = Role.Create("TestRole", "Test Role", appUser.Id);
    role.AddPermission(Permission.UsersRead, appUser.Id);
    role.AddPermission(Permission.UsersCreate, appUser.Id);
    role.AddPermission(Permission.RolesRead, appUser.Id);

    appUser.AddRole(role);

    _context.AppUsers.Add(appUser);
    await _context.SaveChangesAsync();

    _cacheServiceMock
        .Setup(c => c.GetAsync<HashSet<string>>($"auth:permissions-{identityId}", default))
        .ReturnsAsync((HashSet<string>?)null);

    // Act
    var result = await _authorizationService.GetPermissionsForUserAsync(identityId);

    // Assert
    Assert.NotNull(result);
    Assert.Equal(3, result.Count);
    Assert.Contains("users:read", result);
    Assert.Contains("users:create", result);
    Assert.Contains("roles:read", result);

    _cacheServiceMock.Verify(
        c => c.GetAsync<HashSet<string>>($"auth:permissions-{identityId}", default),
        Times.Once);
    _cacheServiceMock.Verify(
        c => c.SetAsync($"auth:permissions-{identityId}", It.IsAny<HashSet<string>>(), null, default),
        Times.Once);
  }

  [Fact]
  public async Task GetPermissionsForUserAsync_WithMultipleRoles_ReturnsAllUniquePermissions()
  {
    // Arrange
    var identityId = "perm-multi-roles";

    var appUser = AppUser.Create();
    appUser.SetIdentityId(identityId);

    var role1 = Role.Create("Role1", "Role 1", appUser.Id);
    role1.AddPermission(Permission.UsersRead, appUser.Id);
    role1.AddPermission(Permission.UsersCreate, appUser.Id);

    var role2 = Role.Create("Role2", "Role 2", appUser.Id);
    role2.AddPermission(Permission.UsersRead, appUser.Id); // Duplicate
    role2.AddPermission(Permission.RolesRead, appUser.Id);

    appUser.AddRole(role1);
    appUser.AddRole(role2);

    _context.AppUsers.Add(appUser);
    await _context.SaveChangesAsync();

    _cacheServiceMock
        .Setup(c => c.GetAsync<HashSet<string>>($"auth:permissions-{identityId}", default))
        .ReturnsAsync((HashSet<string>?)null);

    // Act
    var result = await _authorizationService.GetPermissionsForUserAsync(identityId);

    // Assert
    Assert.NotNull(result);
    Assert.Equal(3, result.Count); // Should deduplicate
    Assert.Contains("users:read", result);
    Assert.Contains("users:create", result);
    Assert.Contains("roles:read", result);
  }

  [Fact]
  public async Task GetPermissionsForUserAsync_ExcludesPermissionsFromDeletedRoles()
  {
    // Arrange
    var identityId = "perm-deleted-roles";

    var appUser = AppUser.Create();
    appUser.SetIdentityId(identityId);

    // Active role with permissions
    var activeRole = Role.Create("ActiveRole", "Active Role", appUser.Id);
    activeRole.AddPermission(Permission.UsersRead, appUser.Id);

    // Deleted role with permissions
    var deletedRole = Role.Create("DeletedRole", "Deleted Role", appUser.Id);
    deletedRole.AddPermission(Permission.UsersCreate, appUser.Id);
    Role.Delete(deletedRole, appUser.Id);

    appUser.AddRole(activeRole);
    appUser.AddRole(deletedRole);

    _context.AppUsers.Add(appUser);
    await _context.SaveChangesAsync();

    _cacheServiceMock
        .Setup(c => c.GetAsync<HashSet<string>>($"auth:permissions-{identityId}", default))
        .ReturnsAsync((HashSet<string>?)null);

    // Act
    var result = await _authorizationService.GetPermissionsForUserAsync(identityId);

    // Assert
    Assert.NotNull(result);
    Assert.Single(result);
    Assert.Contains("users:read", result);
    Assert.DoesNotContain("users:create", result);
  }

  [Fact]
  public async Task GetPermissionsForUserAsync_WithNoPermissions_ReturnsEmptySet()
  {
    // Arrange
    var identityId = "perm-no-permissions";

    var appUser = AppUser.Create();
    appUser.SetIdentityId(identityId);

    var role = Role.Create("EmptyRole", "Empty Role", appUser.Id);
    // Don't add any permissions

    appUser.AddRole(role);

    _context.AppUsers.Add(appUser);
    await _context.SaveChangesAsync();

    _cacheServiceMock
        .Setup(c => c.GetAsync<HashSet<string>>($"auth:permissions-{identityId}", default))
        .ReturnsAsync((HashSet<string>?)null);

    // Act
    var result = await _authorizationService.GetPermissionsForUserAsync(identityId);

    // Assert
    Assert.NotNull(result);
    Assert.Empty(result);
  }

  [Fact]
  public async Task GetPermissionsForUserAsync_WithUserHavingNoRoles_ReturnsEmptySet()
  {
    // Arrange
    var identityId = "perm-no-roles";

    var appUser = AppUser.CreateWithoutRolesForSeeding();
    appUser.SetIdentityId(identityId);

    _context.AppUsers.Add(appUser);
    await _context.SaveChangesAsync();

    _cacheServiceMock
        .Setup(c => c.GetAsync<HashSet<string>>($"auth:permissions-{identityId}", default))
        .ReturnsAsync((HashSet<string>?)null);

    // Act
    var result = await _authorizationService.GetPermissionsForUserAsync(identityId);

    // Assert
    Assert.NotNull(result);
    Assert.Empty(result);
  }

  [Fact]
  public async Task GetPermissionsForUserAsync_CachesResultAfterDatabaseFetch()
  {
    // Arrange
    var identityId = "perm-cache-test";

    var appUser = AppUser.Create();
    appUser.SetIdentityId(identityId);

    var role = Role.Create("CacheTestRole", "Cache Test Role", appUser.Id);
    role.AddPermission(Permission.UsersRead, appUser.Id);

    appUser.AddRole(role);

    _context.AppUsers.Add(appUser);
    await _context.SaveChangesAsync();

    _cacheServiceMock
        .Setup(c => c.GetAsync<HashSet<string>>($"auth:permissions-{identityId}", default))
        .ReturnsAsync((HashSet<string>?)null);

    HashSet<string>? cachedValue = null;
    _cacheServiceMock
        .Setup(c => c.SetAsync(
            $"auth:permissions-{identityId}",
            It.IsAny<HashSet<string>>(),
            null,
            default))
        .Callback<string, HashSet<string>, TimeSpan?, CancellationToken>((key, value, expiration, ct) =>
        {
          cachedValue = value;
        })
        .Returns(Task.CompletedTask);

    // Act
    var result = await _authorizationService.GetPermissionsForUserAsync(identityId);

    // Assert
    Assert.NotNull(cachedValue);
    Assert.Equal(result.Count, cachedValue.Count);
    Assert.Contains("users:read", cachedValue);
  }

  [Fact]
  public async Task GetPermissionsForUserAsync_WithManyPermissions_ReturnsAllCorrectly()
  {
    // Arrange
    var identityId = "perm-many-permissions";

    var appUser = AppUser.Create();
    appUser.SetIdentityId(identityId);

    var role = Role.Create("AdminRole", "Admin Role", appUser.Id);
    role.AddPermission(Permission.UsersRead, appUser.Id);
    role.AddPermission(Permission.UsersCreate, appUser.Id);
    role.AddPermission(Permission.UsersUpdate, appUser.Id);
    role.AddPermission(Permission.UsersDelete, appUser.Id);
    role.AddPermission(Permission.RolesRead, appUser.Id);
    role.AddPermission(Permission.RolesCreate, appUser.Id);
    role.AddPermission(Permission.RolesUpdate, appUser.Id);
    role.AddPermission(Permission.RolesDelete, appUser.Id);

    appUser.AddRole(role);

    _context.AppUsers.Add(appUser);
    await _context.SaveChangesAsync();

    _cacheServiceMock
        .Setup(c => c.GetAsync<HashSet<string>>($"auth:permissions-{identityId}", default))
        .ReturnsAsync((HashSet<string>?)null);

    // Act
    var result = await _authorizationService.GetPermissionsForUserAsync(identityId);

    // Assert
    Assert.NotNull(result);
    Assert.Equal(8, result.Count);
    Assert.Contains("users:read", result);
    Assert.Contains("users:create", result);
    Assert.Contains("users:update", result);
    Assert.Contains("users:delete", result);
    Assert.Contains("roles:read", result);
    Assert.Contains("roles:create", result);
    Assert.Contains("roles:update", result);
    Assert.Contains("roles:delete", result);
  }

  #endregion

  #region Cache Key Tests

  [Fact]
  public async Task GetRolesForUserAsync_UsesDifferentCacheKeys_ForDifferentUsers()
  {
    // Arrange
    var identityId1 = "user-1";
    var identityId2 = "user-2";

    var cachedRoles1 = new UserRolesResponse { UserId = Guid.NewGuid(), Roles = new List<Role>() };
    var cachedRoles2 = new UserRolesResponse { UserId = Guid.NewGuid(), Roles = new List<Role>() };

    _cacheServiceMock
        .Setup(c => c.GetAsync<UserRolesResponse>($"auth:roles-{identityId1}", default))
        .ReturnsAsync(cachedRoles1);
    _cacheServiceMock
        .Setup(c => c.GetAsync<UserRolesResponse>($"auth:roles-{identityId2}", default))
        .ReturnsAsync(cachedRoles2);

    // Act
    var result1 = await _authorizationService.GetRolesForUserAsync(identityId1);
    var result2 = await _authorizationService.GetRolesForUserAsync(identityId2);

    // Assert
    Assert.NotEqual(result1.UserId, result2.UserId);
    _cacheServiceMock.Verify(
        c => c.GetAsync<UserRolesResponse>($"auth:roles-{identityId1}", default),
        Times.Once);
    _cacheServiceMock.Verify(
        c => c.GetAsync<UserRolesResponse>($"auth:roles-{identityId2}", default),
        Times.Once);
  }

  [Fact]
  public async Task GetPermissionsForUserAsync_UsesDifferentCacheKeys_ForDifferentUsers()
  {
    // Arrange
    var identityId1 = "user-perm-1";
    var identityId2 = "user-perm-2";

    var cachedPerms1 = new HashSet<string> { "users:read" };
    var cachedPerms2 = new HashSet<string> { "roles:read" };

    _cacheServiceMock
        .Setup(c => c.GetAsync<HashSet<string>>($"auth:permissions-{identityId1}", default))
        .ReturnsAsync(cachedPerms1);
    _cacheServiceMock
        .Setup(c => c.GetAsync<HashSet<string>>($"auth:permissions-{identityId2}", default))
        .ReturnsAsync(cachedPerms2);

    // Act
    var result1 = await _authorizationService.GetPermissionsForUserAsync(identityId1);
    var result2 = await _authorizationService.GetPermissionsForUserAsync(identityId2);

    // Assert
    Assert.NotEqual(result1, result2);
    Assert.Contains("users:read", result1);
    Assert.Contains("roles:read", result2);
  }

  #endregion

  #region Integration Tests

  [Fact]
  public async Task AuthorizationService_EndToEnd_RolesAndPermissions()
  {
    // Arrange
    var identityId = "e2e-user";

    var appUser = AppUser.Create();
    appUser.SetIdentityId(identityId);

    var adminRole = Role.Admin;
    adminRole.AddPermission(Permission.UsersRead, appUser.Id);
    adminRole.AddPermission(Permission.UsersCreate, appUser.Id);
    adminRole.AddPermission(Permission.RolesRead, appUser.Id);

    appUser.AddRole(adminRole);

    _context.AppUsers.Add(appUser);
    await _context.SaveChangesAsync();

    _cacheServiceMock
        .Setup(c => c.GetAsync<UserRolesResponse>(It.IsAny<string>(), default))
        .ReturnsAsync((UserRolesResponse?)null);
    _cacheServiceMock
        .Setup(c => c.GetAsync<HashSet<string>>(It.IsAny<string>(), default))
        .ReturnsAsync((HashSet<string>?)null);

    // Act
    var roles = await _authorizationService.GetRolesForUserAsync(identityId);
    var permissions = await _authorizationService.GetPermissionsForUserAsync(identityId);

    // Assert - Roles
    Assert.NotNull(roles);
    Assert.Single(roles.Roles);
    Assert.Contains(roles.Roles, r => r.Name.Value == "Admin");

    // Assert - Permissions
    Assert.NotNull(permissions);
    Assert.Equal(3, permissions.Count);
    Assert.Contains("users:read", permissions);
    Assert.Contains("users:create", permissions);
    Assert.Contains("roles:read", permissions);
  }

  #endregion

  public void Dispose()
  {
    _context?.Dispose();
  }
}
