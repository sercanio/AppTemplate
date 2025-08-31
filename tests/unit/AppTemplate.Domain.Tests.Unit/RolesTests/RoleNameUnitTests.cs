using AppTemplate.Domain.Roles.ValueObjects;

namespace AppTemplate.Domain.Tests.Unit.RolesTests;

public class RoleNameUnitTests
{
  [Fact]
  public void Constructor_ShouldSetValue()
  {
    var name = "Admin";
    var roleName = new RoleName(name);

    Assert.Equal(name, roleName.Value);
  }

  [Fact]
  public void Constructor_ShouldThrowForEmptyValue()
  {
    Assert.Throws<ArgumentException>(() => new RoleName(""));
    Assert.Throws<ArgumentException>(() => new RoleName(" "));
  }

  [Fact]
  public void Equality_ShouldWorkForSameValue()
  {
    var name1 = new RoleName("Admin");
    var name2 = new RoleName("Admin");

    Assert.Equal(name1, name2);
  }

  [Fact]
  public void ToString_ShouldReturnValue()
  {
    var name = "Admin";
    var roleName = new RoleName(name);

    Assert.Equal(name, roleName.ToString());
  }
}
