using AppTemplate.Domain.Roles.ValueObjects;

namespace AppTemplate.Domain.Tests.Unit.RolesTests;

[Trait("Category", "Unit")]
public class RoleDefaultFlagUnitTests
{
  [Fact]
  public void Constructor_ShouldSetValue()
  {
    var flag = new RoleDefaultFlag(true);
    Assert.True(flag.Value);

    var flag2 = new RoleDefaultFlag(false);
    Assert.False(flag2.Value);
  }

  [Fact]
  public void Equality_ShouldWorkForSameValue()
  {
    var flag1 = new RoleDefaultFlag(true);
    var flag2 = new RoleDefaultFlag(true);

    Assert.Equal(flag1, flag2);
  }

  [Fact]
  public void ToString_ShouldReturnValueAsString()
  {
    var flag = new RoleDefaultFlag(true);
    Assert.Equal("True", flag.ToString());
  }
}