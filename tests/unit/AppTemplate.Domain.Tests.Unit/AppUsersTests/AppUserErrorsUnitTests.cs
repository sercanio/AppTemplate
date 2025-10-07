using AppTemplate.Domain.AppUsers;

namespace AppTemplate.Domain.Tests.Unit.AppUsersTests;

[Trait("Category", "Unit")]
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

  [Fact]
  public void AllErrors_ShouldHaveUniqueErrorCodes()
  {
    // Arrange
    var errors = new[]
    {
        AppUserErrors.NotFound,
        AppUserErrors.InvalidCredentials,
        AppUserErrors.IdentityIdNotFound
    };

    // Act
    var codes = errors.Select(e => e.Code).ToList();

    // Assert
    Assert.Equal(codes.Count, codes.Distinct().Count());
  }

  [Fact]
  public void AllErrors_ShouldHaveValidStatusCodes()
  {
    // Arrange
    var errors = new[]
    {
        AppUserErrors.NotFound,
        AppUserErrors.InvalidCredentials,
        AppUserErrors.IdentityIdNotFound
    };

    // Act & Assert
    foreach (var error in errors)
    {
        Assert.True(error.StatusCode >= 100 && error.StatusCode < 600, 
            $"Error {error.Code} has invalid status code: {error.StatusCode}");
    }
  }

  [Fact]
  public void AllErrors_ShouldHaveNonEmptyNames()
  {
    // Arrange
    var errors = new[]
    {
        AppUserErrors.NotFound,
        AppUserErrors.InvalidCredentials,
        AppUserErrors.IdentityIdNotFound
    };

    // Act & Assert
    foreach (var error in errors)
    {
        Assert.False(string.IsNullOrWhiteSpace(error.Name), 
            $"Error {error.Code} has empty or null name");
    }
  }

  [Fact]
  public void AllErrors_ShouldHaveNonEmptyCodes()
  {
    // Arrange
    var errors = new[]
    {
        AppUserErrors.NotFound,
        AppUserErrors.InvalidCredentials,
        AppUserErrors.IdentityIdNotFound
    };

    // Act & Assert
    foreach (var error in errors)
    {
        Assert.False(string.IsNullOrWhiteSpace(error.Code), 
            $"Error has empty or null code");
    }
  }

  [Fact]
  public void NotFound_ShouldHaveCorrectErrorType()
  {
    // Arrange & Act
    var error = AppUserErrors.NotFound;

    // Assert
    Assert.Equal("User.NotFound", error.Code);
    Assert.StartsWith("User.", error.Code);
  }

  [Fact]
  public void InvalidCredentials_ShouldHaveCorrectErrorType()
  {
    // Arrange & Act
    var error = AppUserErrors.InvalidCredentials;

    // Assert
    Assert.Equal("User.InvalidCredentials", error.Code);
    Assert.StartsWith("User.", error.Code);
  }

  [Fact]
  public void IdentityIdNotFound_ShouldHaveCorrectErrorType()
  {
    // Arrange & Act
    var error = AppUserErrors.IdentityIdNotFound;

    // Assert
    Assert.Equal("User.IdentityIdNotFound", error.Code);
    Assert.StartsWith("User.", error.Code);
  }

  [Theory]
  [InlineData(404)]
  [InlineData(401)]
  [InlineData(500)]
  public void Errors_ShouldHaveAppropriateStatusCodes(int expectedStatusCode)
  {
    // Arrange
    var errors = new[]
    {
        AppUserErrors.NotFound,
        AppUserErrors.InvalidCredentials,
        AppUserErrors.IdentityIdNotFound
    };

    // Act
    var errorWithExpectedStatus = errors.FirstOrDefault(e => e.StatusCode == expectedStatusCode);

    // Assert
    Assert.NotNull(errorWithExpectedStatus);
  }

  [Fact]
  public void ErrorMessages_ShouldBeDescriptiveAndMeaningful()
  {
    // Arrange & Act
    var notFoundError = AppUserErrors.NotFound;
    var invalidCredentialsError = AppUserErrors.InvalidCredentials;
    var identityNotFoundError = AppUserErrors.IdentityIdNotFound;

    // Assert
    Assert.Contains("user", notFoundError.Name.ToLower());
    Assert.Contains("not found", notFoundError.Name.ToLower());
    
    Assert.Contains("credentials", invalidCredentialsError.Name.ToLower());
    Assert.Contains("invalid", invalidCredentialsError.Name.ToLower());
    
    Assert.Contains("identity", identityNotFoundError.Name.ToLower());
    Assert.Contains("accessible", identityNotFoundError.Name.ToLower());
  }
}
