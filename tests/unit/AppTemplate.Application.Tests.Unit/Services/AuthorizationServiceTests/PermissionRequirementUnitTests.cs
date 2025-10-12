using AppTemplate.Application.Services.Authorization;
using FluentAssertions;
using Microsoft.AspNetCore.Authorization;

namespace AppTemplate.Application.Tests.Unit.Services.AuthorizationServiceTests;

[Trait("Category", "Unit")]
public class PermissionRequirementUnitTests
{
  #region Constructor Tests

  [Fact]
  public void Constructor_WithValidPermission_ShouldCreateRequirement()
  {
    // Arrange
    var permission = "users:read";

    // Act
    var requirement = new PermissionRequirement(permission);

    // Assert
    requirement.Should().NotBeNull();
    requirement.Permission.Should().Be(permission);
  }

  [Fact]
  public void Constructor_WithEmptyPermission_ShouldCreateRequirement()
  {
    // Arrange
    var permission = string.Empty;

    // Act
    var requirement = new PermissionRequirement(permission);

    // Assert
    requirement.Should().NotBeNull();
    requirement.Permission.Should().BeEmpty();
  }

  [Fact]
  public void Constructor_WithNullPermission_ShouldCreateRequirement()
  {
    // Arrange
    string permission = null!;

    // Act
    var requirement = new PermissionRequirement(permission);

    // Assert
    requirement.Should().NotBeNull();
    requirement.Permission.Should().BeNull();
  }

  #endregion

  #region Permission Property Tests

  [Fact]
  public void Permission_ShouldBeReadable()
  {
    // Arrange
    var expectedPermission = "roles:admin";
    var requirement = new PermissionRequirement(expectedPermission);

    // Act
    var actualPermission = requirement.Permission;

    // Assert
    actualPermission.Should().Be(expectedPermission);
  }

  [Theory]
  [InlineData("users:read")]
  [InlineData("users:create")]
  [InlineData("users:update")]
  [InlineData("users:delete")]
  [InlineData("users:admin")]
  public void Permission_WithVariousUserPermissions_ShouldPreserveValue(string permission)
  {
    // Act
    var requirement = new PermissionRequirement(permission);

    // Assert
    requirement.Permission.Should().Be(permission);
  }

  [Theory]
  [InlineData("roles:read")]
  [InlineData("roles:create")]
  [InlineData("roles:update")]
  [InlineData("roles:delete")]
  [InlineData("roles:admin")]
  public void Permission_WithVariousRolePermissions_ShouldPreserveValue(string permission)
  {
    // Act
    var requirement = new PermissionRequirement(permission);

    // Assert
    requirement.Permission.Should().Be(permission);
  }

  [Theory]
  [InlineData("entries:read")]
  [InlineData("entries:create")]
  [InlineData("titles:update")]
  [InlineData("notifications:read")]
  [InlineData("auditlogs:read")]
  public void Permission_WithVariousEntityPermissions_ShouldPreserveValue(string permission)
  {
    // Act
    var requirement = new PermissionRequirement(permission);

    // Assert
    requirement.Permission.Should().Be(permission);
  }

  #endregion

  #region Interface Implementation Tests

  [Fact]
  public void PermissionRequirement_ShouldImplementIAuthorizationRequirement()
  {
    // Arrange
    var requirement = new PermissionRequirement("test:permission");

    // Act & Assert
    requirement.Should().BeAssignableTo<IAuthorizationRequirement>();
  }

  #endregion

  #region Special Characters and Edge Cases

  [Theory]
  [InlineData("permission:with:multiple:colons")]
  [InlineData("permission-with-dashes")]
  [InlineData("permission_with_underscores")]
  [InlineData("permission.with.dots")]
  [InlineData("PermissionWithCamelCase")]
  [InlineData("PERMISSION_WITH_UPPERCASE")]
  public void Permission_WithSpecialCharacters_ShouldPreserveValue(string permission)
  {
    // Act
    var requirement = new PermissionRequirement(permission);

    // Assert
    requirement.Permission.Should().Be(permission);
  }

  [Fact]
  public void Permission_WithWhitespace_ShouldPreserveWhitespace()
  {
    // Arrange
    var permission = "  permission with spaces  ";

    // Act
    var requirement = new PermissionRequirement(permission);

    // Assert
    requirement.Permission.Should().Be(permission);
  }

  [Fact]
  public void Permission_WithUnicodeCharacters_ShouldPreserveValue()
  {
    // Arrange
    var permission = "权限:读取"; // Chinese characters

    // Act
    var requirement = new PermissionRequirement(permission);

    // Assert
    requirement.Permission.Should().Be(permission);
  }

  [Fact]
  public void Permission_WithLongString_ShouldPreserveValue()
  {
    // Arrange
    var longPermission = new string('a', 1000);

    // Act
    var requirement = new PermissionRequirement(longPermission);

    // Assert
    requirement.Permission.Should().Be(longPermission);
    requirement.Permission.Should().HaveLength(1000);
  }

  #endregion

  #region Real-World Permission Scenarios

  [Fact]
  public void PermissionRequirement_UsersAdminScenario_ShouldCreateCorrectly()
  {
    // Arrange
    var permission = "users:admin";

    // Act
    var requirement = new PermissionRequirement(permission);

    // Assert
    requirement.Permission.Should().Be("users:admin");
  }

  [Fact]
  public void PermissionRequirement_EntriesReadScenario_ShouldCreateCorrectly()
  {
    // Arrange
    var permission = "entries:read";

    // Act
    var requirement = new PermissionRequirement(permission);

    // Assert
    requirement.Permission.Should().Be("entries:read");
  }

  [Fact]
  public void PermissionRequirement_TitlesDeleteScenario_ShouldCreateCorrectly()
  {
    // Arrange
    var permission = "titles:delete";

    // Act
    var requirement = new PermissionRequirement(permission);

    // Assert
    requirement.Permission.Should().Be("titles:delete");
  }

  [Fact]
  public void PermissionRequirement_NotificationsUpdateScenario_ShouldCreateCorrectly()
  {
    // Arrange
    var permission = "notifications:update";

    // Act
    var requirement = new PermissionRequirement(permission);

    // Assert
    requirement.Permission.Should().Be("notifications:update");
  }

  [Fact]
  public void PermissionRequirement_AuditLogsReadScenario_ShouldCreateCorrectly()
  {
    // Arrange
    var permission = "auditlogs:read";

    // Act
    var requirement = new PermissionRequirement(permission);

    // Assert
    requirement.Permission.Should().Be("auditlogs:read");
  }

  #endregion

  #region Multiple Instance Tests

  [Fact]
  public void PermissionRequirement_MultipleInstances_ShouldBeIndependent()
  {
    // Arrange & Act
    var requirement1 = new PermissionRequirement("users:read");
    var requirement2 = new PermissionRequirement("users:write");

    // Assert
    requirement1.Permission.Should().Be("users:read");
    requirement2.Permission.Should().Be("users:write");
    requirement1.Permission.Should().NotBe(requirement2.Permission);
  }

  [Fact]
  public void PermissionRequirement_SamePermissionValue_ShouldCreateSeparateInstances()
  {
    // Arrange & Act
    var requirement1 = new PermissionRequirement("users:read");
    var requirement2 = new PermissionRequirement("users:read");

    // Assert
    requirement1.Should().NotBeSameAs(requirement2);
    requirement1.Permission.Should().Be(requirement2.Permission);
  }

  #endregion

  #region Type Safety Tests

  [Fact]
  public void PermissionRequirement_ShouldBeSealed()
  {
    // Act
    var type = typeof(PermissionRequirement);

    // Assert
    type.IsSealed.Should().BeTrue();
  }

  [Fact]
  public void PermissionRequirement_Permission_ShouldBeReadOnly()
  {
    // Arrange
    var requirement = new PermissionRequirement("test:permission");
    var propertyInfo = typeof(PermissionRequirement).GetProperty("Permission");

    // Act & Assert
    propertyInfo.Should().NotBeNull();
    propertyInfo!.CanRead.Should().BeTrue();
    propertyInfo.CanWrite.Should().BeFalse();
  }

  #endregion
}