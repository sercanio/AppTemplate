using AppTemplate.Domain.Roles.DomainEvents;

namespace AppTemplate.Domain.Tests.Unit.RolesTests;

public class RoleDomainEventsUnitTests
{
  [Fact]
  public void StaticEventNames_ShouldHaveExpectedValues()
  {
    Assert.Equal("RoleCreated", RoleDomainEvents.Created);
    Assert.Equal("RoleDeleted", RoleDomainEvents.Deleted);
    Assert.Equal("RoleNameUpdated", RoleDomainEvents.UpdatedName);
    Assert.Equal("RolePermissionAdded", RoleDomainEvents.AddedPermission);
    Assert.Equal("RolePermissionRemoved", RoleDomainEvents.RemovedPermission);
  }
}
