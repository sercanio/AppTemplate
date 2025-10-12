using AppTemplate.Domain.Roles;

namespace AppTemplate.Domain.Tests.Unit.RolesTests;

[Trait("Category", "Unit")]
public class RoleErrorsUnitTests
{
  [Fact]
  public void NotFound_ShouldHaveCorrectProperties()
  {
    var error = RoleErrors.NotFound;
    Assert.Equal("Role.NotFound", error.Code);
    Assert.Equal(404, error.StatusCode);
    Assert.Equal("The role with the specified identifier was not found", error.Name);
  }

  [Fact]
  public void Overlap_ShouldHaveCorrectProperties()
  {
    var error = RoleErrors.Overlap;
    Assert.Equal("Role.Overlap", error.Code);
    Assert.Equal(409, error.StatusCode);
    Assert.Equal("The current role is overlapping with an existing one", error.Name);
  }
}
