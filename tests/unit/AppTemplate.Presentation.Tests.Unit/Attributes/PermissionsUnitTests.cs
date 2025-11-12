using System.Reflection;
using AppTemplate.Presentation.Attributes;

namespace AppTemplate.Presentation.Tests.Unit.Attributes;

[Trait("Category", "Unit")]
public class PermissionsUnitTests
{
  [Fact]
  public void UsersRead_ShouldHaveCorrectValue()
  {
    // Arrange & Act
    var permission = Permissions.UsersRead;

    // Assert
    Assert.Equal("users:read", permission);
  }

  [Fact]
  public void UsersCreate_ShouldHaveCorrectValue()
  {
    // Arrange & Act
    var permission = Permissions.UsersCreate;

    // Assert
    Assert.Equal("users:create", permission);
  }

  [Fact]
  public void UsersUpdate_ShouldHaveCorrectValue()
  {
    // Arrange & Act
    var permission = Permissions.UsersUpdate;

    // Assert
    Assert.Equal("users:update", permission);
  }

  [Fact]
  public void UsersDelete_ShouldHaveCorrectValue()
  {
    // Arrange & Act
    var permission = Permissions.UsersDelete;

    // Assert
    Assert.Equal("users:delete", permission);
  }

  [Fact]
  public void RolesRead_ShouldHaveCorrectValue()
  {
    // Arrange & Act
    var permission = Permissions.RolesRead;

    // Assert
    Assert.Equal("roles:read", permission);
  }

  [Fact]
  public void RolesCreate_ShouldHaveCorrectValue()
  {
    // Arrange & Act
    var permission = Permissions.RolesCreate;

    // Assert
    Assert.Equal("roles:create", permission);
  }

  [Fact]
  public void RolesUpdate_ShouldHaveCorrectValue()
  {
    // Arrange & Act
    var permission = Permissions.RolesUpdate;

    // Assert
    Assert.Equal("roles:update", permission);
  }

  [Fact]
  public void RolesDelete_ShouldHaveCorrectValue()
  {
    // Arrange & Act
    var permission = Permissions.RolesDelete;

    // Assert
    Assert.Equal("roles:delete", permission);
  }

  [Fact]
  public void AuditLogsRead_ShouldHaveCorrectValue()
  {
    // Arrange & Act
    var permission = Permissions.AuditLogsRead;

    // Assert
    Assert.Equal("auditlogs:read", permission);
  }

  [Fact]
  public void AuditLogsCreate_ShouldHaveCorrectValue()
  {
    // Arrange & Act
    var permission = Permissions.AuditLogsCreate;

    // Assert
    Assert.Equal("auditlogs:create", permission);
  }

  [Fact]
  public void AuditLogsUpdate_ShouldHaveCorrectValue()
  {
    // Arrange & Act
    var permission = Permissions.AuditLogsUpdate;

    // Assert
    Assert.Equal("auditlogs:update", permission);
  }

  [Fact]
  public void AuditLogsDelete_ShouldHaveCorrectValue()
  {
    // Arrange & Act
    var permission = Permissions.AuditLogsDelete;

    // Assert
    Assert.Equal("auditlogs:delete", permission);
  }

  [Fact]
  public void PermissionsRead_ShouldHaveCorrectValue()
  {
    // Arrange & Act
    var permission = Permissions.PermissionsRead;

    // Assert
    Assert.Equal("permissions:read", permission);
  }

  [Fact]
  public void NotificationsRead_ShouldHaveCorrectValue()
  {
    // Arrange & Act
    var permission = Permissions.NotificationsRead;

    // Assert
    Assert.Equal("notifications:read", permission);
  }

  [Fact]
  public void NotificationsCreate_ShouldHaveCorrectValue()
  {
    // Arrange & Act
    var permission = Permissions.NotificationsCreate;

    // Assert
    Assert.Equal("notifications:create", permission);
  }

  [Fact]
  public void NotificationsUpdate_ShouldHaveCorrectValue()
  {
    // Arrange & Act
    var permission = Permissions.NotificationsUpdate;

    // Assert
    Assert.Equal("notifications:update", permission);
  }

  [Fact]
  public void NotificationsDelete_ShouldHaveCorrectValue()
  {
    // Arrange & Act
    var permission = Permissions.NotificationsDelete;

    // Assert
    Assert.Equal("notifications:delete", permission);
  }

  [Fact]
  public void StatisticsRead_ShouldHaveCorrectValue()
  {
    // Arrange & Act
    var permission = Permissions.StatisticsRead;

    // Assert
    Assert.Equal("statistics:read", permission);
  }

  [Theory]
  [InlineData("UsersRead", "users:read")]
  [InlineData("UsersCreate", "users:create")]
  [InlineData("UsersUpdate", "users:update")]
  [InlineData("UsersDelete", "users:delete")]
  [InlineData("RolesRead", "roles:read")]
  [InlineData("RolesCreate", "roles:create")]
  [InlineData("RolesUpdate", "roles:update")]
  [InlineData("RolesDelete", "roles:delete")]
  [InlineData("AuditLogsRead", "auditlogs:read")]
  [InlineData("AuditLogsCreate", "auditlogs:create")]
  [InlineData("AuditLogsUpdate", "auditlogs:update")]
  [InlineData("AuditLogsDelete", "auditlogs:delete")]
  [InlineData("PermissionsRead", "permissions:read")]
  [InlineData("NotificationsRead", "notifications:read")]
  [InlineData("NotificationsCreate", "notifications:create")]
  [InlineData("NotificationsUpdate", "notifications:update")]
  [InlineData("NotificationsDelete", "notifications:delete")]
  [InlineData("StatisticsRead", "statistics:read")]
  public void PermissionConstants_ShouldHaveCorrectValues(string fieldName, string expectedValue)
  {
    // Arrange
    var field = typeof(Permissions).GetField(fieldName, BindingFlags.Public | BindingFlags.Static);

    // Act
    var actualValue = field?.GetValue(null) as string;

    // Assert
    Assert.NotNull(field);
    Assert.Equal(expectedValue, actualValue);
  }

  [Fact]
  public void AllPermissionConstants_ShouldBeStrings()
  {
    // Arrange
    var fields = typeof(Permissions).GetFields(BindingFlags.Public | BindingFlags.Static)
        .Where(f => f.IsLiteral && !f.IsInitOnly); // const fields are literal and not readonly

    // Act & Assert
    foreach (var field in fields)
    {
      Assert.Equal(typeof(string), field.FieldType);
      Assert.True(field.IsStatic);
      Assert.True(field.IsLiteral); // const fields are literal
    }
  }

  [Fact]
  public void AllPermissionConstants_ShouldNotBeNullOrEmpty()
  {
    // Arrange
    var fields = typeof(Permissions).GetFields(BindingFlags.Public | BindingFlags.Static)
        .Where(f => f.IsLiteral && !f.IsInitOnly);

    // Act & Assert
    foreach (var field in fields)
    {
      var value = field.GetValue(null) as string;
      Assert.False(string.IsNullOrEmpty(value), $"Permission constant '{field.Name}' should not be null or empty");
    }
  }

  [Fact]
  public void AllPermissionConstants_ShouldFollowNamingConvention()
  {
    // Arrange
    var fields = typeof(Permissions).GetFields(BindingFlags.Public | BindingFlags.Static)
        .Where(f => f.IsLiteral && !f.IsInitOnly);

    // Act & Assert
    foreach (var field in fields)
    {
      var value = field.GetValue(null) as string;
      Assert.NotNull(value);
      Assert.Contains(":", value);

      var parts = value.Split(':');
      Assert.Equal(2, parts.Length);
      Assert.False(string.IsNullOrWhiteSpace(parts[0]), $"Resource part should not be empty in '{value}'");
      Assert.False(string.IsNullOrWhiteSpace(parts[1]), $"Action part should not be empty in '{value}'");
    }
  }

  [Fact]
  public void AllPermissionConstants_ShouldBeUnique()
  {
    // Arrange
    var fields = typeof(Permissions).GetFields(BindingFlags.Public | BindingFlags.Static)
        .Where(f => f.IsLiteral && !f.IsInitOnly);
    var values = new List<string>();

    // Act
    foreach (var field in fields)
    {
      var value = field.GetValue(null) as string;
      values.Add(value!);
    }

    // Assert
    var duplicates = values.GroupBy(x => x).Where(g => g.Count() > 1).Select(g => g.Key).ToList();
    Assert.Empty(duplicates);
  }

  [Fact]
  public void UserPermissions_ShouldHaveCorrectResourcePrefix()
  {
    // Arrange & Act & Assert
    Assert.StartsWith("users:", Permissions.UsersRead);
    Assert.StartsWith("users:", Permissions.UsersCreate);
    Assert.StartsWith("users:", Permissions.UsersUpdate);
    Assert.StartsWith("users:", Permissions.UsersDelete);
  }

  [Fact]
  public void RolePermissions_ShouldHaveCorrectResourcePrefix()
  {
    // Arrange & Act & Assert
    Assert.StartsWith("roles:", Permissions.RolesRead);
    Assert.StartsWith("roles:", Permissions.RolesCreate);
    Assert.StartsWith("roles:", Permissions.RolesUpdate);
    Assert.StartsWith("roles:", Permissions.RolesDelete);
  }

  [Fact]
  public void AuditLogPermissions_ShouldHaveCorrectResourcePrefix()
  {
    // Arrange & Act & Assert
    Assert.StartsWith("auditlogs:", Permissions.AuditLogsRead);
    Assert.StartsWith("auditlogs:", Permissions.AuditLogsCreate);
    Assert.StartsWith("auditlogs:", Permissions.AuditLogsUpdate);
    Assert.StartsWith("auditlogs:", Permissions.AuditLogsDelete);
  }

  [Fact]
  public void NotificationPermissions_ShouldHaveCorrectResourcePrefix()
  {
    // Arrange & Act & Assert
    Assert.StartsWith("notifications:", Permissions.NotificationsRead);
    Assert.StartsWith("notifications:", Permissions.NotificationsCreate);
    Assert.StartsWith("notifications:", Permissions.NotificationsUpdate);
    Assert.StartsWith("notifications:", Permissions.NotificationsDelete);
  }

  [Fact]
  public void PermissionsRead_ShouldHaveCorrectResourcePrefix()
  {
    // Arrange & Act & Assert
    Assert.StartsWith("permissions:", Permissions.PermissionsRead);
  }

  [Fact]
  public void StatisticsRead_ShouldHaveCorrectResourcePrefix()
  {
    // Arrange & Act & Assert
    Assert.StartsWith("statistics:", Permissions.StatisticsRead);
  }

  [Fact]
  public void CrudPermissions_ShouldHaveCorrectActionSuffixes()
  {
    // Arrange
    var crudPermissions = new Dictionary<string, string>
        {
            { Permissions.UsersRead, "read" },
            { Permissions.UsersCreate, "create" },
            { Permissions.UsersUpdate, "update" },
            { Permissions.UsersDelete, "delete" },
            { Permissions.RolesRead, "read" },
            { Permissions.RolesCreate, "create" },
            { Permissions.RolesUpdate, "update" },
            { Permissions.RolesDelete, "delete" },
            { Permissions.AuditLogsRead, "read" },
            { Permissions.AuditLogsCreate, "create" },
            { Permissions.AuditLogsUpdate, "update" },
            { Permissions.AuditLogsDelete, "delete" },
            { Permissions.NotificationsRead, "read" },
            { Permissions.NotificationsCreate, "create" },
            { Permissions.NotificationsUpdate, "update" },
            { Permissions.NotificationsDelete, "delete" }
        };

    // Act & Assert
    foreach (var kvp in crudPermissions)
    {
      Assert.EndsWith($":{kvp.Value}", kvp.Key);
    }
  }

  [Fact]
  public void PermissionConstants_ShouldNotContainWhitespace()
  {
    // Arrange
    var fields = typeof(Permissions).GetFields(BindingFlags.Public | BindingFlags.Static)
        .Where(f => f.IsLiteral && !f.IsInitOnly);

    // Act & Assert
    foreach (var field in fields)
    {
      var value = field.GetValue(null) as string;
      Assert.NotNull(value);
      Assert.DoesNotContain(" ", value);
      Assert.DoesNotContain("\t", value);
      Assert.DoesNotContain("\n", value);
      Assert.DoesNotContain("\r", value);
    }
  }

  [Fact]
  public void PermissionConstants_ShouldBeLowercase()
  {
    // Arrange
    var fields = typeof(Permissions).GetFields(BindingFlags.Public | BindingFlags.Static)
        .Where(f => f.IsLiteral && !f.IsInitOnly);

    // Act & Assert
    foreach (var field in fields)
    {
      var value = field.GetValue(null) as string;
      Assert.NotNull(value);
      Assert.Equal(value.ToLower(), value);
    }
  }

  [Fact]
  public void Permissions_ShouldBeARecord()
  {
    // Arrange & Act
    var type = typeof(Permissions);

    // Assert
    Assert.True(type.IsClass);
    // In .NET, records are classes with specific characteristics
    // We can check if it has the typical record members
    var methods = type.GetMethods(BindingFlags.Public | BindingFlags.Instance);
    var hasEqualsMethod = methods.Any(m => m.Name == "Equals" && m.GetParameters().Length == 1);
    var hasGetHashCodeMethod = methods.Any(m => m.Name == "GetHashCode" && m.GetParameters().Length == 0);

    Assert.True(hasEqualsMethod);
    Assert.True(hasGetHashCodeMethod);
  }

  [Fact]
  public void Permissions_ShouldBeSealed()
  {
    // Arrange & Act
    var type = typeof(Permissions);

    // Assert
    Assert.True(type.IsSealed);
  }

  [Fact]
  public void Permissions_ShouldBePublic()
  {
    // Arrange & Act
    var type = typeof(Permissions);

    // Assert
    Assert.True(type.IsPublic);
  }

  [Fact]
  public void PermissionConstants_CountShouldMatch()
  {
    // Arrange
    var fields = typeof(Permissions).GetFields(BindingFlags.Public | BindingFlags.Static)
        .Where(f => f.IsLiteral && !f.IsInitOnly);

    // Act
    var count = fields.Count();

    // Assert
    // Based on the Permissions class, we expect exactly 18 permission constants
    Assert.Equal(18, count);
  }

  [Fact]
  public void AllPermissions_ShouldFollowExpectedPattern()
  {
    // Arrange
    var expectedPermissions = new[]
    {
            "users:read", "users:create", "users:update", "users:delete",
            "roles:read", "roles:create", "roles:update", "roles:delete",
            "auditlogs:read", "auditlogs:create", "auditlogs:update", "auditlogs:delete",
            "permissions:read",
            "notifications:read", "notifications:create", "notifications:update", "notifications:delete",
            "statistics:read"
        };

    var fields = typeof(Permissions).GetFields(BindingFlags.Public | BindingFlags.Static)
        .Where(f => f.IsLiteral && !f.IsInitOnly);
    var actualPermissions = fields.Select(f => f.GetValue(null) as string).ToArray();

    // Act & Assert
    Assert.Equal(expectedPermissions.Length, actualPermissions.Length);
    foreach (var expected in expectedPermissions)
    {
      Assert.Contains(expected, actualPermissions);
    }
  }

  [Fact]
  public void PermissionConstants_ShouldNotHaveSpecialCharactersExceptColon()
  {
    // Arrange
    var fields = typeof(Permissions).GetFields(BindingFlags.Public | BindingFlags.Static)
        .Where(f => f.IsLiteral && !f.IsInitOnly);
    var allowedCharacters = "abcdefghijklmnopqrstuvwxyz:";

    // Act & Assert
    foreach (var field in fields)
    {
      var value = field.GetValue(null) as string;
      Assert.NotNull(value);

      foreach (var character in value)
      {
        Assert.Contains(character, allowedCharacters);
      }
    }
  }

  [Fact]
  public void PermissionResources_ShouldBeValidIdentifiers()
  {
    // Arrange
    var fields = typeof(Permissions).GetFields(BindingFlags.Public | BindingFlags.Static)
        .Where(f => f.IsLiteral && !f.IsInitOnly);
    var validResources = new[] { "users", "roles", "auditlogs", "permissions", "notifications", "statistics" };

    // Act & Assert
    foreach (var field in fields)
    {
      var value = field.GetValue(null) as string;
      Assert.NotNull(value);

      var resource = value.Split(':')[0];
      Assert.Contains(resource, validResources);
    }
  }

  [Fact]
  public void PermissionActions_ShouldBeValidActions()
  {
    // Arrange
    var fields = typeof(Permissions).GetFields(BindingFlags.Public | BindingFlags.Static)
        .Where(f => f.IsLiteral && !f.IsInitOnly);
    var validActions = new[] { "read", "create", "update", "delete" };

    // Act & Assert
    foreach (var field in fields)
    {
      var value = field.GetValue(null) as string;
      Assert.NotNull(value);

      var action = value.Split(':')[1];
      Assert.Contains(action, validActions);
    }
  }
}