using System;
using System.Linq;
using Xunit;
using AppTemplate.Domain.AppUsers;
using AppTemplate.Domain.Roles;
using AppTemplate.Domain.Roles.DomainEvents;
using AppTemplate.Domain.Roles.ValueObjects;

namespace AppTemplate.Domain.Tests.Unit.RolesTests;

[Trait("Category", "Unit")]
public class RoleUnitTests
{
  [Fact]
  public void Create_ShouldInitializeRoleAndRaiseDomainEvent()
  {
    // Arrange
    var name = "User";
    var displayName = "Kullanıcı";
    var createdById = Guid.NewGuid();

    // Act
    var role = Role.Create(name, displayName, createdById);

    // Assert
    Assert.NotNull(role);
    Assert.Equal(name, role.Name.Value);
    Assert.Equal(displayName, role.DisplayName.Value);
    Assert.Equal(createdById, role.CreatedById);
    Assert.False(role.IsDefault);

    var domainEvent = role.GetDomainEvents().OfType<RoleCreatedDomainEvent>().FirstOrDefault();
    Assert.NotNull(domainEvent);
    Assert.Equal(role.Id, domainEvent.RoleId);
  }

  [Fact]
  public void Create_WithIsDefaultTrue_ShouldSetIsDefaultFlag()
  {
    // Arrange
    var name = "DefaultRole";
    var displayName = "Default Role";
    var createdById = Guid.NewGuid();

    // Act
    var role = Role.Create(name, displayName, createdById, isDefault: true);

    // Assert
    Assert.True(role.IsDefault);
  }

  [Fact]
  public void Delete_ShouldMarkRoleDeletedAndRaiseDomainEvent()
  {
    // Arrange
    var role = Role.Create("User", "Kullanıcı", Guid.NewGuid());
    var deletedById = Guid.NewGuid();

    // Act
    var deletedRole = Role.Delete(role, deletedById);

    // Assert
    Assert.Same(role, deletedRole); // Verify it's the same instance
    Assert.True(deletedRole.DeletedOnUtc.HasValue);
    Assert.Equal(deletedById, deletedRole.DeletedById);

    var domainEvent = deletedRole.GetDomainEvents().OfType<RoleDeletedDomainEvent>().FirstOrDefault();
    Assert.NotNull(domainEvent);
    Assert.Equal(role.Id, domainEvent.RoleId);
  }

  [Fact]
  public void ChangeName_ShouldUpdateNameAndRaiseDomainEvent()
  {
    // Arrange
    var role = Role.Create("User", "Kullanıcı", Guid.NewGuid());
    var updatedById = Guid.NewGuid();
    var newName = "Admin";
    var oldName = role.Name.Value;

    // Act
    role.ChangeName(newName, updatedById);

    // Assert
    Assert.Equal(newName, role.Name.Value);
    Assert.Equal(updatedById, role.UpdatedById);
    Assert.True(role.UpdatedOnUtc.HasValue);

    var domainEvent = role.GetDomainEvents().OfType<RoleNameUpdatedDomainEvent>().FirstOrDefault();
    Assert.NotNull(domainEvent);
    Assert.Equal(role.Id, domainEvent.RoleId);
    Assert.Equal(oldName, domainEvent.OldRoleName);
  }

  [Fact]
  public void ChangeDisplayName_ShouldUpdateDisplayNameAndRaiseDomainEvent()
  {
    // Arrange
    var role = Role.Create("User", "Kullanıcı", Guid.NewGuid());
    var updatedById = Guid.NewGuid();
    var newDisplayName = "Yönetici";
    var oldDisplayName = role.DisplayName.Value;

    // Act
    role.ChangeDisplayName(newDisplayName, updatedById);

    // Assert
    Assert.Equal(newDisplayName, role.DisplayName.Value);
    Assert.Equal(updatedById, role.UpdatedById);
    Assert.True(role.UpdatedOnUtc.HasValue);

    var domainEvent = role.GetDomainEvents().OfType<RoleNameUpdatedDomainEvent>().FirstOrDefault();
    Assert.NotNull(domainEvent);
    Assert.Equal(role.Id, domainEvent.RoleId);
    Assert.Equal(oldDisplayName, domainEvent.OldRoleName);
  }

  [Fact]
  public void ParameterlessConstructor_ShouldInitializeWithDefaults()
  {
    // Act
    var role = new Role();

    // Assert
    Assert.NotEqual(Guid.Empty, role.Id);
    Assert.Equal("DefaultRoleName", role.Name.Value);
    Assert.Equal("DefaultRoleDisplayName", role.DisplayName.Value);
    Assert.False(role.IsDefault);
    Assert.Null(role.CreatedById);
    Assert.Null(role.UpdatedById);
    Assert.Null(role.DeletedById);
    Assert.Empty(role.Users);
    Assert.Empty(role.Permissions);
  }

  [Fact]
  public void Constructor_WithParameters_ShouldSetProperties()
  {
    // Arrange
    var id = Guid.NewGuid();
    var name = new RoleName("TestRole");
    var displayName = new RoleName("Test Role");
    var createdById = Guid.NewGuid();

    // Act
    var role = new Role(id, name, displayName, createdById, true);

    // Assert
    Assert.Equal(id, role.Id);
    Assert.Equal(name.Value, role.Name.Value);
    Assert.Equal(displayName.Value, role.DisplayName.Value);
    Assert.Equal(createdById, role.CreatedById);
    Assert.True(role.IsDefault);
  }

  [Fact]
  public void PropertyGetters_ShouldReturnCorrectValues()
  {
    // Arrange
    var createdById = Guid.NewGuid();
    var role = Role.Create("TestRole", "Test Role", createdById);

    // Act & Assert - Test all property getters explicitly
    var name = role.Name;
    var displayName = role.DisplayName;
    var users = role.Users;
    var permissions = role.Permissions;
    var isDefault = role.IsDefault;
    var createdBy = role.CreatedBy;
    var updatedById = role.UpdatedById;
    var updatedBy = role.UpdatedBy;
    var deletedById = role.DeletedById;
    var deletedBy = role.DeletedBy;

    Assert.NotNull(name);
    Assert.NotNull(displayName);
    Assert.NotNull(users);
    Assert.NotNull(permissions);
    Assert.Equal(createdById, role.CreatedById);
    Assert.Null(createdBy);
    Assert.Null(updatedById);
    Assert.Null(updatedBy);
    Assert.Null(deletedById);
    Assert.Null(deletedBy);
    Assert.False(isDefault);
  }
}

[Trait("Category", "Unit")]
public class RoleStaticTests
{
  [Fact]
  public void DefaultRole_StaticInstance_ShouldHaveExpectedValues()
  {
    // Act
    var r = Role.DefaultRole;

    // Assert
    Assert.Equal("Registered", r.Name.Value);
    Assert.Equal("kayıtlı", r.DisplayName.Value);
    Assert.True(r.IsDefault);
    Assert.Equal(Guid.Parse("5dc6ec47-5b7c-4c2b-86cd-3a671834e56e"), r.Id);
    Assert.Null(r.CreatedById);
  }

  [Fact]
  public void AdminRole_StaticInstance_ShouldHaveExpectedValues()
  {
    // Act
    var r = Role.Admin;

    // Assert
    Assert.Equal("Admin", r.Name.Value);
    Assert.Equal("yönetici", r.DisplayName.Value);
    Assert.False(r.IsDefault);
    Assert.Equal(Guid.Parse("4b606d86-3537-475a-aa20-26aadd8f5cfd"), r.Id);
    Assert.Null(r.CreatedById);
  }

  [Fact]
  public void StaticConstructor_ShouldInitializeStaticInstances()
  {
    // Act & Assert - This test ensures the static constructor is called
    var defaultRole = Role.DefaultRole;
    var adminRole = Role.Admin;

    Assert.NotNull(defaultRole);
    Assert.NotNull(adminRole);
    Assert.NotEqual(defaultRole.Id, adminRole.Id);
  }
}

[Trait("Category", "Unit")]
public class RoleUserTests
{
  [Fact]
  public void AddUser_ShouldAddUserToRole()
  {
    // Arrange
    var role = Role.Create("User", "Kullanıcı", Guid.NewGuid());
    var user = AppUser.Create();

    // Act
    role.AddUser(user);

    // Assert
    Assert.Contains(user, role.Users);
    Assert.Single(role.Users);
  }

  [Fact]
  public void AddUser_WhenUserAlreadyExists_ShouldNotAddDuplicate()
  {
    // Arrange
    var role = Role.Create("User", "Kullanıcı", Guid.NewGuid());
    var user = AppUser.Create();

    // Act
    role.AddUser(user);
    role.AddUser(user); // Add same user again

    // Assert
    Assert.Single(role.Users);
    Assert.Contains(user, role.Users);
  }

  [Fact]
  public void RemoveUser_ShouldRemoveUserFromRole()
  {
    // Arrange
    var role = Role.Create("User", "Kullanıcı", Guid.NewGuid());
    var user = AppUser.Create();
    role.AddUser(user);
    Assert.Single(role.Users);

    // Act
    role.RemoveUser(user);

    // Assert
    Assert.DoesNotContain(user, role.Users);
    Assert.Empty(role.Users);
  }

  [Fact]
  public void RemoveUser_WhenUserNotInRole_ShouldNotThrow()
  {
    // Arrange
    var role = Role.Create("User", "Kullanıcı", Guid.NewGuid());
    var user = AppUser.Create();

    // Act & Assert - Should not throw when removing user that wasn't added
    role.RemoveUser(user);
    Assert.Empty(role.Users);
  }

  [Fact]
  public void Users_Property_ShouldReturnReadOnlyCollection()
  {
    // Arrange
    var role = Role.Create("User", "Kullanıcı", Guid.NewGuid());

    // Act
    var users = role.Users;

    // Assert
    Assert.IsAssignableFrom<IReadOnlyCollection<AppUser>>(users);
  }
}

[Trait("Category", "Unit")]
public class RolePermissionTests
{
  [Fact]
  public void AddPermission_ShouldAddPermissionAndRaiseEvent()
  {
    // Arrange
    var role = Role.Create("User", "Kullanıcı", Guid.NewGuid());
    var permission = Permission.UsersRead; // Use static permission
    var updatedById = Guid.NewGuid();

    // Act
    role.AddPermission(permission, updatedById);

    // Assert
    Assert.Contains(permission, role.Permissions);
    Assert.Single(role.Permissions);
    Assert.Equal(updatedById, role.UpdatedById);
    Assert.True(role.UpdatedOnUtc.HasValue);

    var domainEvent = role.GetDomainEvents().OfType<RolePermissionAddedDomainEvent>().FirstOrDefault();
    Assert.NotNull(domainEvent);
    Assert.Equal(role.Id, domainEvent.RoleId);
    Assert.Equal(permission.Id, domainEvent.PermissionId);
  }

  [Fact]
  public void AddPermission_WhenPermissionAlreadyExists_ShouldNotAddDuplicate()
  {
    // Arrange
    var role = Role.Create("User", "Kullanıcı", Guid.NewGuid());
    var permission = Permission.UsersRead;
    var updatedById = Guid.NewGuid();

    // Act
    role.AddPermission(permission, updatedById);
    var eventsCountAfterFirst = role.GetDomainEvents().Count;
    role.AddPermission(permission, updatedById); // Add same permission again

    // Assert
    Assert.Single(role.Permissions);
    Assert.Contains(permission, role.Permissions);
    // Should not add another domain event for duplicate
    Assert.Equal(eventsCountAfterFirst, role.GetDomainEvents().Count);
  }

  [Fact]
  public void RemovePermission_ShouldRemovePermissionAndRaiseEvent()
  {
    // Arrange
    var role = Role.Create("User", "Kullanıcı", Guid.NewGuid());
    var permission = Permission.UsersRead;
    var updatedById = Guid.NewGuid();
    role.AddPermission(permission, updatedById);
    Assert.Single(role.Permissions);

    // Act
    role.RemovePermission(permission, updatedById);

    // Assert
    Assert.DoesNotContain(permission, role.Permissions);
    Assert.Empty(role.Permissions);
    Assert.Equal(updatedById, role.UpdatedById);
    Assert.True(role.UpdatedOnUtc.HasValue);

    var domainEvents = role.GetDomainEvents();
    var removeEvent = domainEvents.OfType<RolePermissionRemovedDomainEvent>().FirstOrDefault();
    Assert.NotNull(removeEvent);
    Assert.Equal(role.Id, removeEvent.RoleId);
    Assert.Equal(permission.Id, removeEvent.PermissionId);
  }

  [Fact]
  public void RemovePermission_WhenPermissionNotInRole_ShouldNotRaiseEvent()
  {
    // Arrange
    var role = Role.Create("User", "Kullanıcı", Guid.NewGuid());
    var permission = Permission.UsersRead;
    var updatedById = Guid.NewGuid();

    // Clear any events from creation
    role.ClearDomainEvents();
    var initialUpdatedById = role.UpdatedById;
    var initialUpdatedOn = role.UpdatedOnUtc;

    // Act
    role.RemovePermission(permission, updatedById);

    // Assert
    Assert.Empty(role.Permissions);
    Assert.Equal(initialUpdatedById, role.UpdatedById);
    Assert.Equal(initialUpdatedOn, role.UpdatedOnUtc);

    var removeEvent = role.GetDomainEvents().OfType<RolePermissionRemovedDomainEvent>().FirstOrDefault();
    Assert.Null(removeEvent);
  }

  [Fact]
  public void AddMultiplePermissions_ShouldAddAllPermissions()
  {
    // Arrange
    var role = Role.Create("User", "Kullanıcı", Guid.NewGuid());
    var permission1 = Permission.UsersRead;
    var permission2 = Permission.UsersCreate;
    var updatedById = Guid.NewGuid();

    // Act
    role.AddPermission(permission1, updatedById);
    role.AddPermission(permission2, updatedById);

    // Assert
    Assert.Equal(2, role.Permissions.Count);
    Assert.Contains(permission1, role.Permissions);
    Assert.Contains(permission2, role.Permissions);
  }

  [Fact]
  public void Permissions_Property_ShouldReturnReadOnlyCollection()
  {
    // Arrange
    var role = Role.Create("User", "Kullanıcı", Guid.NewGuid());

    // Act
    var permissions = role.Permissions;

    // Assert
    Assert.IsAssignableFrom<IReadOnlyCollection<Permission>>(permissions);
  }

  [Fact]
  public void AddPermission_WithDifferentPermissions_ShouldCallAllBranches()
  {
    // Arrange
    var role = Role.Create("User", "Kullanıcı", Guid.NewGuid());
    var updatedById = Guid.NewGuid();

    // Act & Assert - Test multiple static permissions to ensure coverage
    var permissions = new[]
    {
      Permission.UsersRead,
      Permission.UsersCreate,
      Permission.UsersUpdate,
      Permission.UsersDelete,
      Permission.RolesRead
    };

    foreach (var permission in permissions)
    {
      role.AddPermission(permission, updatedById);
    }

    Assert.Equal(permissions.Length, role.Permissions.Count);
    foreach (var permission in permissions)
    {
      Assert.Contains(permission, role.Permissions);
    }
  }

  [Fact]
  public void RemovePermission_WithDifferentPermissions_ShouldCallAllBranches()
  {
    // Arrange
    var role = Role.Create("User", "Kullanıcı", Guid.NewGuid());
    var updatedById = Guid.NewGuid();
    var permissions = new[]
    {
      Permission.UsersRead,
      Permission.UsersCreate,
      Permission.UsersUpdate
    };

    // Add permissions first
    foreach (var permission in permissions)
    {
      role.AddPermission(permission, updatedById);
    }

    // Act & Assert - Remove permissions
    foreach (var permission in permissions)
    {
      role.RemovePermission(permission, updatedById);
    }

    Assert.Empty(role.Permissions);
  }
}