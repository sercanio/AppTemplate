using AppTemplate.Domain.AppUsers;

namespace AppTemplate.Domain.Tests.Unit.AppUsersTests;

public class AppUserErrorsUnitTests
{
  [Fact]
  public void NotFound_ShouldHaveCorrectProperties()
  {
    var error = AppUserErrors.NotFound;
    Assert.Equal("User.NotFound", error.Code);
    Assert.Equal(404, error.StatusCode);
    Assert.Equal("The user with the specified identifier was not found", error.Name);
  }

  [Fact]
  public void InvalidCredentials_ShouldHaveCorrectProperties()
  {
    var error = AppUserErrors.InvalidCredentials;
    Assert.Equal("User.InvalidCredentials", error.Code);
    Assert.Equal(401, error.StatusCode);
    Assert.Equal("The provided credentials were invalid", error.Name);
  }

  [Fact]
  public void IdentityIdNotFound_ShouldHaveCorrectProperties()
  {
    var error = AppUserErrors.IdentityIdNotFound;
    Assert.Equal("User.IdentityIdNotFound", error.Code);
    Assert.Equal(500, error.StatusCode);
    Assert.Equal("The identity id is not accessible", error.Name);
  }
}
