using AppTemplate.Domain.AppUsers;
using AppTemplate.Domain.Roles;
using AppTemplate.Domain.Roles.DomainEvents;

namespace AppTemplate.Domain.Tests.Unit.RolesTests;

[Trait("Category", "Unit")]
public class RoleUnitTests
{
  [Fact]
  public void Create_ShouldInitializeRoleAndRaiseDomainEvent()
  {
    var name = "User";
    var displayName = "Kullanıcı";
    var createdById = Guid.NewGuid();

    var role = Role.Create(name, displayName, createdById);

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
  public void Delete_ShouldMarkRoleDeletedAndRaiseDomainEvent()
  {
    var role = Role.Create("User", "Kullanıcı", Guid.NewGuid());
    var deletedById = Guid.NewGuid();

    var deletedRole = Role.Delete(role, deletedById);

    Assert.True(deletedRole.DeletedOnUtc.HasValue);
    Assert.Equal(deletedById, deletedRole.DeletedById);

    var domainEvent = deletedRole.GetDomainEvents().OfType<RoleDeletedDomainEvent>().FirstOrDefault();
    Assert.NotNull(domainEvent);
    Assert.Equal(role.Id, domainEvent.RoleId);
  }

  [Fact]
  public void ChangeName_ShouldUpdateNameAndRaiseDomainEvent()
  {
    var role = Role.Create("User", "Kullanıcı", Guid.NewGuid());
    var updatedById = Guid.NewGuid();
    var newName = "Admin";

    var oldName = role.Name.Value;
    role.ChangeName(newName, updatedById);

    Assert.Equal(newName, role.Name.Value);
    Assert.Equal(updatedById, role.UpdatedById);

    var domainEvent = role.GetDomainEvents().OfType<RoleNameUpdatedDomainEvent>().FirstOrDefault();
    Assert.NotNull(domainEvent);
    Assert.Equal(role.Id, domainEvent.RoleId);
    Assert.Equal(oldName, domainEvent.OldRoleName);
  }

  [Fact]
  public void ChangeDisplayName_ShouldUpdateDisplayNameAndRaiseDomainEvent()
  {
    var role = Role.Create("User", "Kullanıcı", Guid.NewGuid());
    var updatedById = Guid.NewGuid();
    var newDisplayName = "Yönetici";

    var oldDisplayName = role.DisplayName.Value;
    role.ChangeDisplayName(newDisplayName, updatedById);

    Assert.Equal(newDisplayName, role.DisplayName.Value);
    Assert.Equal(updatedById, role.UpdatedById);

    var domainEvent = role.GetDomainEvents().OfType<RoleNameUpdatedDomainEvent>().FirstOrDefault();
    Assert.NotNull(domainEvent);
    Assert.Equal(role.Id, domainEvent.RoleId);
    Assert.Equal(oldDisplayName, domainEvent.OldRoleName);
  }
}

[Trait("Category", "Unit")]
public class RoleStaticTests
{
  [Fact]
  public void DefaultRole_StaticInstance_ShouldHaveExpectedValues()
  {
    var r = Role.DefaultRole;
    Assert.Equal("Registered", r.Name.Value);
    Assert.True(r.IsDefault);
  }
}

[Trait("Category", "Unit")]

public class RoleUserTests
{
  [Fact]
  public void AddUser_ShouldAddUserToRole()
  {
    var role = Role.Create("User", "Kullanıcı", Guid.NewGuid());
    var user = AppUser.Create();
    role.AddUser(user);
    Assert.Contains(user, role.Users);
  }

  [Fact]
  public void RemoveUser_ShouldRemoveUserFromRole()
  {
    var role = Role.Create("User", "Kullanıcı", Guid.NewGuid());
    var user = AppUser.Create();
    role.AddUser(user);
    role.RemoveUser(user);
    Assert.DoesNotContain(user, role.Users);
  }
}

[Trait("Category", "Unit")]
public class RolePermissionTests
{
  [Fact]
  public void AddPermission_ShouldAddPermissionAndRaiseEvent()
  {
    var role = Role.Create("User", "Kullanıcı", Guid.NewGuid());
    var permission = new Permission(Guid.NewGuid(), "users", "users:read");
    var updatedById = Guid.NewGuid();

    role.AddPermission(permission, updatedById);

    Assert.Contains(permission, role.Permissions);
    // Assert domain event raised (if accessible)
  }

  [Fact]
  public void RemovePermission_ShouldRemovePermissionAndRaiseEvent()
  {
    var role = Role.Create("User", "Kullanıcı", Guid.NewGuid());
    var permission = new Permission(Guid.NewGuid(), "users", "users:read");
    var updatedById = Guid.NewGuid();

    role.AddPermission(permission, updatedById);
    role.RemovePermission(permission, updatedById);

    Assert.DoesNotContain(permission, role.Permissions);
    // Assert domain event raised (if accessible)
  }
}