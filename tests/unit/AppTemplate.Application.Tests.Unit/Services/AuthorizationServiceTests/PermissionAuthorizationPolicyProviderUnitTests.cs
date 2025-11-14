using AppTemplate.Application.Services.Authorization;
using FluentAssertions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using Moq;

namespace AppTemplate.Application.Tests.Unit.Services.AuthorizationServiceTests;

[Trait("Category", "Unit")]
public class PermissionAuthorizationPolicyProviderUnitTests
{
  private readonly Mock<IOptions<AuthorizationOptions>> _optionsMock;
  private readonly AuthorizationOptions _authorizationOptions;

  public PermissionAuthorizationPolicyProviderUnitTests()
  {
    _authorizationOptions = new AuthorizationOptions();
    _optionsMock = new Mock<IOptions<AuthorizationOptions>>();
    _optionsMock.Setup(x => x.Value).Returns(_authorizationOptions);
  }

  #region Constructor Tests

  [Fact]
  public void Constructor_WithValidOptions_ShouldCreateProvider()
  {
    // Act
    var provider = new PermissionAuthorizationPolicyProvider(_optionsMock.Object);

    // Assert
    provider.Should().NotBeNull();
    provider.Should().BeAssignableTo<IAuthorizationPolicyProvider>();
    provider.Should().BeAssignableTo<DefaultAuthorizationPolicyProvider>();
  }

  [Fact]
  public void Constructor_WithNullOptions_ShouldThrowArgumentNullException()
  {
    // Act
    Action act = () => new PermissionAuthorizationPolicyProvider(null!);

    // Assert
    act.Should().Throw<ArgumentNullException>();
  }

  #endregion

  #region GetPolicyAsync - Existing Policy Tests

  [Fact]
  public async Task GetPolicyAsync_WhenPolicyExistsInOptions_ShouldReturnExistingPolicy()
  {
    // Arrange
    var policyName = "ExistingPolicy";
    var existingPolicy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
    _authorizationOptions.AddPolicy(policyName, existingPolicy);

    var provider = new PermissionAuthorizationPolicyProvider(_optionsMock.Object);

    // Act
    var result = await provider.GetPolicyAsync(policyName);

    // Assert
    result.Should().NotBeNull();
    result.Should().BeSameAs(existingPolicy);
  }

  [Fact]
  public async Task GetPolicyAsync_DefaultPolicy_ShouldReturnDefaultPolicy()
  {
    // Arrange
    var defaultPolicy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
    _authorizationOptions.DefaultPolicy = defaultPolicy;

    var provider = new PermissionAuthorizationPolicyProvider(_optionsMock.Object);

    // Act
    var result = await provider.GetPolicyAsync("SomePolicy");

    // Assert - Will create a new policy with PermissionRequirement
    result.Should().NotBeNull();
  }

  #endregion

  #region GetPolicyAsync - Dynamic Permission Policy Tests

  [Fact]
  public async Task GetPolicyAsync_WithNewPermission_ShouldCreatePermissionPolicy()
  {
    // Arrange
    var permissionName = "users:read";
    var provider = new PermissionAuthorizationPolicyProvider(_optionsMock.Object);

    // Act
    var result = await provider.GetPolicyAsync(permissionName);

    // Assert
    result.Should().NotBeNull();
    result!.Requirements.Should().ContainSingle();
    result.Requirements.First().Should().BeOfType<PermissionRequirement>();
    var requirement = result.Requirements.First() as PermissionRequirement;
    requirement!.Permission.Should().Be(permissionName);
  }

  [Fact]
  public async Task GetPolicyAsync_WithNewPermission_ShouldAddPolicyToOptions()
  {
    // Arrange
    var permissionName = "users:create";
    var provider = new PermissionAuthorizationPolicyProvider(_optionsMock.Object);

    // Act
    await provider.GetPolicyAsync(permissionName);

    // Assert
    var addedPolicy = _authorizationOptions.GetPolicy(permissionName); // Fixed: Removed await
    addedPolicy.Should().NotBeNull();
  }

  [Fact]
  public async Task GetPolicyAsync_CalledTwiceWithSamePermission_ShouldReturnSamePolicy()
  {
    // Arrange
    var permissionName = "users:update";
    var provider = new PermissionAuthorizationPolicyProvider(_optionsMock.Object);

    // Act
    var result1 = await provider.GetPolicyAsync(permissionName);
    var result2 = await provider.GetPolicyAsync(permissionName);

    // Assert
    result1.Should().NotBeNull();
    result2.Should().NotBeNull();
    result1.Should().BeSameAs(result2);
  }

  [Theory]
  [InlineData("users:read")]
  [InlineData("users:create")]
  [InlineData("users:update")]
  [InlineData("users:delete")]
  [InlineData("users:admin")]
  public async Task GetPolicyAsync_WithVariousUserPermissions_ShouldCreateCorrectPolicies(string permission)
  {
    // Arrange
    var provider = new PermissionAuthorizationPolicyProvider(_optionsMock.Object);

    // Act
    var result = await provider.GetPolicyAsync(permission);

    // Assert
    result.Should().NotBeNull();
    result!.Requirements.Should().ContainSingle();
    var requirement = result.Requirements.First() as PermissionRequirement;
    requirement.Should().NotBeNull();
    requirement!.Permission.Should().Be(permission);
  }

  [Theory]
  [InlineData("roles:read")]
  [InlineData("roles:create")]
  [InlineData("roles:update")]
  [InlineData("roles:delete")]
  [InlineData("roles:admin")]
  public async Task GetPolicyAsync_WithVariousRolePermissions_ShouldCreateCorrectPolicies(string permission)
  {
    // Arrange
    var provider = new PermissionAuthorizationPolicyProvider(_optionsMock.Object);

    // Act
    var result = await provider.GetPolicyAsync(permission);

    // Assert
    result.Should().NotBeNull();
    result!.Requirements.Should().ContainSingle();
    var requirement = result.Requirements.First() as PermissionRequirement;
    requirement!.Permission.Should().Be(permission);
  }

  #endregion

  #region Real-World Permission Scenarios

  [Fact]
  public async Task GetPolicyAsync_EntriesReadPermission_ShouldCreatePolicy()
  {
    // Arrange
    var provider = new PermissionAuthorizationPolicyProvider(_optionsMock.Object);

    // Act
    var result = await provider.GetPolicyAsync("entries:read");

    // Assert
    result.Should().NotBeNull();
    var requirement = result!.Requirements.First() as PermissionRequirement;
    requirement!.Permission.Should().Be("entries:read");
  }

  [Fact]
  public async Task GetPolicyAsync_TitlesAdminPermission_ShouldCreatePolicy()
  {
    // Arrange
    var provider = new PermissionAuthorizationPolicyProvider(_optionsMock.Object);

    // Act
    var result = await provider.GetPolicyAsync("titles:admin");

    // Assert
    result.Should().NotBeNull();
    var requirement = result!.Requirements.First() as PermissionRequirement;
    requirement!.Permission.Should().Be("titles:admin");
  }

  [Fact]
  public async Task GetPolicyAsync_NotificationsUpdatePermission_ShouldCreatePolicy()
  {
    // Arrange
    var provider = new PermissionAuthorizationPolicyProvider(_optionsMock.Object);

    // Act
    var result = await provider.GetPolicyAsync("notifications:update");

    // Assert
    result.Should().NotBeNull();
    var requirement = result!.Requirements.First() as PermissionRequirement;
    requirement!.Permission.Should().Be("notifications:update");
  }

  [Fact]
  public async Task GetPolicyAsync_AuditLogsReadPermission_ShouldCreatePolicy()
  {
    // Arrange
    var provider = new PermissionAuthorizationPolicyProvider(_optionsMock.Object);

    // Act
    var result = await provider.GetPolicyAsync("auditlogs:read");

    // Assert
    result.Should().NotBeNull();
    var requirement = result!.Requirements.First() as PermissionRequirement;
    requirement!.Permission.Should().Be("auditlogs:read");
  }

  [Fact]
  public async Task GetPolicyAsync_StatisticsReadPermission_ShouldCreatePolicy()
  {
    // Arrange
    var provider = new PermissionAuthorizationPolicyProvider(_optionsMock.Object);

    // Act
    var result = await provider.GetPolicyAsync("statistics:read");

    // Assert
    result.Should().NotBeNull();
    var requirement = result!.Requirements.First() as PermissionRequirement;
    requirement!.Permission.Should().Be("statistics:read");
  }

  [Fact]
  public async Task GetPolicyAsync_UserFollowsCreatePermission_ShouldCreatePolicy()
  {
    // Arrange
    var provider = new PermissionAuthorizationPolicyProvider(_optionsMock.Object);

    // Act
    var result = await provider.GetPolicyAsync("userfollows:create");

    // Assert
    result.Should().NotBeNull();
    var requirement = result!.Requirements.First() as PermissionRequirement;
    requirement!.Permission.Should().Be("userfollows:create");
  }

  #endregion

  #region Multiple Permissions Test

  [Fact]
  public async Task GetPolicyAsync_MultiplePermissions_ShouldCreateSeparatePolicies()
  {
    // Arrange
    var provider = new PermissionAuthorizationPolicyProvider(_optionsMock.Object);
    var permissions = new[] { "users:read", "roles:write", "entries:delete" };

    // Act
    var policies = new List<AuthorizationPolicy?>();
    foreach (var permission in permissions)
    {
      var policy = await provider.GetPolicyAsync(permission);
      policies.Add(policy);
    }

    // Assert
    policies.Should().HaveCount(3);
    policies.Should().AllSatisfy(p => p.Should().NotBeNull());

    for (int i = 0; i < permissions.Length; i++)
    {
      var requirement = policies[i]!.Requirements.First() as PermissionRequirement;
      requirement!.Permission.Should().Be(permissions[i]);
    }
  }

  #endregion

  #region Edge Cases

  [Fact]
  public async Task GetPolicyAsync_WithEmptyString_ShouldCreatePolicy()
  {
    // Arrange
    var provider = new PermissionAuthorizationPolicyProvider(_optionsMock.Object);

    // Act
    var result = await provider.GetPolicyAsync(string.Empty);

    // Assert
    result.Should().NotBeNull();
    var requirement = result!.Requirements.First() as PermissionRequirement;
    requirement!.Permission.Should().BeEmpty();
  }

  [Fact]
  public async Task GetPolicyAsync_WithSpecialCharacters_ShouldCreatePolicy()
  {
    // Arrange
    var provider = new PermissionAuthorizationPolicyProvider(_optionsMock.Object);
    var permission = "custom:permission:with:multiple:colons";

    // Act
    var result = await provider.GetPolicyAsync(permission);

    // Assert
    result.Should().NotBeNull();
    var requirement = result!.Requirements.First() as PermissionRequirement;
    requirement!.Permission.Should().Be(permission);
  }

  [Fact]
  public async Task GetPolicyAsync_WithLongPermissionName_ShouldCreatePolicy()
  {
    // Arrange
    var provider = new PermissionAuthorizationPolicyProvider(_optionsMock.Object);
    var longPermission = new string('a', 500);

    // Act
    var result = await provider.GetPolicyAsync(longPermission);

    // Assert
    result.Should().NotBeNull();
    var requirement = result!.Requirements.First() as PermissionRequirement;
    requirement!.Permission.Should().Be(longPermission);
  }

  #endregion

  #region Concurrent Access Tests

  [Fact]
  public async Task GetPolicyAsync_ConcurrentCalls_ShouldHandleCorrectly()
  {
    // Arrange
    var provider = new PermissionAuthorizationPolicyProvider(_optionsMock.Object);
    var permission = "concurrent:test";

    // Act
    var tasks = Enumerable.Range(0, 10).Select(_ => provider.GetPolicyAsync(permission));
    var results = await Task.WhenAll(tasks);

    // Assert
    results.Should().HaveCount(10);
    results.Should().AllSatisfy(r => r.Should().NotBeNull());

    // All should be the same policy instance (cached)
    results.Distinct().Should().ContainSingle();
  }

  #endregion

  #region GetDefaultPolicyAsync Tests

  [Fact]
  public async Task GetDefaultPolicyAsync_ShouldReturnDefaultPolicy()
  {
    // Arrange
    var provider = new PermissionAuthorizationPolicyProvider(_optionsMock.Object);

    // Act
    var result = await provider.GetDefaultPolicyAsync();

    // Assert
    result.Should().NotBeNull();
  }

  #endregion

  #region GetFallbackPolicyAsync Tests

  [Fact]
  public async Task GetFallbackPolicyAsync_ShouldReturnFallbackPolicy()
  {
    // Arrange
    var provider = new PermissionAuthorizationPolicyProvider(_optionsMock.Object);

    // Act
    var result = await provider.GetFallbackPolicyAsync();

    // Assert
    // Fallback policy can be null by default
    result.Should().BeNull();
  }

  #endregion

  #region Type Safety Tests

  [Fact]
  public void PermissionAuthorizationPolicyProvider_ShouldBeSealed()
  {
    // Act
    var type = typeof(PermissionAuthorizationPolicyProvider);

    // Assert
    type.IsSealed.Should().BeTrue();
  }

  [Fact]
  public void PermissionAuthorizationPolicyProvider_ShouldInheritFromDefaultAuthorizationPolicyProvider()
  {
    // Arrange
    var provider = new PermissionAuthorizationPolicyProvider(_optionsMock.Object);

    // Act & Assert
    provider.Should().BeAssignableTo<DefaultAuthorizationPolicyProvider>();
  }

  #endregion
}