using AppTemplate.Domain.Roles.DomainEvents;

namespace AppTemplate.Domain.Tests.Unit.RolesTests;

public class RoleDomainEventTypesUnitTests
{
  [Fact]
  public void RoleCreatedDomainEvent_ShouldSetProperties()
  {
    var roleId = Guid.NewGuid();
    var evt = new RoleCreatedDomainEvent(roleId);

    Assert.Equal(roleId, evt.RoleId);
  }

  [Fact]
  public void RoleDeletedDomainEvent_ShouldSetProperties()
  {
    var roleId = Guid.NewGuid();
    var evt = new RoleDeletedDomainEvent(roleId);

    Assert.Equal(roleId, evt.RoleId);
  }

  [Fact]
  public void RoleNameUpdatedDomainEvent_ShouldSetProperties()
  {
    var roleId = Guid.NewGuid();
    var oldName = "OldName";
    var evt = new RoleNameUpdatedDomainEvent(roleId, oldName);

    Assert.Equal(roleId, evt.RoleId);
    Assert.Equal(oldName, evt.OldRoleName);
  }

  [Fact]
  public void RolePermissionAddedDomainEvent_ShouldSetProperties()
  {
    var roleId = Guid.NewGuid();
    var permissionId = Guid.NewGuid();
    var evt = new RolePermissionAddedDomainEvent(roleId, permissionId);

    Assert.Equal(roleId, evt.RoleId);
    Assert.Equal(permissionId, evt.PermissionId);
  }

  [Fact]
  public void RolePermissionRemovedDomainEvent_ShouldSetProperties()
  {
    var roleId = Guid.NewGuid();
    var permissionId = Guid.NewGuid();
    var evt = new RolePermissionRemovedDomainEvent(roleId, permissionId);

    Assert.Equal(roleId, evt.RoleId);
    Assert.Equal(permissionId, evt.PermissionId);
  }
}
