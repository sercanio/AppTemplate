using AppTemplate.Domain.Roles;

namespace AppTemplate.Domain.Tests.Unit.RolesTests;

[Trait("Category", "Unit")]
public class PermissionUnitTests
{
    [Fact]
    public void UsersAdmin_StaticInstance_ShouldHaveExpectedValues()
    {
        var p = Permission.UsersAdmin;
        Assert.Equal("users", p.Feature);
        Assert.Equal("users:admin", p.Name);
        Assert.Equal(Guid.Parse("c8a25b63-74ee-4375-98c8-e64107bb6d76"), p.Id);
    }
}