using AppTemplate.Domain.AppUsers;

namespace AppTemplate.Domain.Tests.Unit.AppUsersTests;

[Trait("Category", "Unit")]
public class AppUserErrorsUnitTests
{
  [Fact]
  public void NotFound_ShouldHaveCorrectCode()
  {
    // Act
    var error = AppUserErrors.NotFound;

    // Assert
    Assert.Equal("User.NotFound", error.Code);
  }

  [Fact]
  public void NotFound_ShouldHaveCorrectStatusCode()
  {
    // Act
    var error = AppUserErrors.NotFound;

    // Assert
    Assert.Equal(404, error.StatusCode);
  }

  [Fact]
  public void NotFound_ShouldHaveCorrectMessage()
  {
    // Act
    var error = AppUserErrors.NotFound;

    // Assert
    Assert.Equal("The user with the specified identifier was not found", error.Name);
  }

  [Fact]
  public void NotFound_ShouldReturnSameInstanceOnMultipleCalls()
  {
    // Act
    var error1 = AppUserErrors.NotFound;
    var error2 = AppUserErrors.NotFound;

    // Assert
    Assert.Same(error1, error2);
  }

  [Fact]
  public void InvalidCredentials_ShouldHaveCorrectCode()
  {
    // Act
    var error = AppUserErrors.InvalidCredentials;

    // Assert
    Assert.Equal("User.InvalidCredentials", error.Code);
  }

  [Fact]
  public void InvalidCredentials_ShouldHaveCorrectStatusCode()
  {
    // Act
    var error = AppUserErrors.InvalidCredentials;

    // Assert
    Assert.Equal(401, error.StatusCode);
  }

  [Fact]
  public void InvalidCredentials_ShouldHaveCorrectMessage()
  {
    // Act
    var error = AppUserErrors.InvalidCredentials;

    // Assert
    Assert.Equal("The provided credentials were invalid", error.Name);
  }

  [Fact]
  public void InvalidCredentials_ShouldReturnSameInstanceOnMultipleCalls()
  {
    // Act
    var error1 = AppUserErrors.InvalidCredentials;
    var error2 = AppUserErrors.InvalidCredentials;

    // Assert
    Assert.Same(error1, error2);
  }

  [Fact]
  public void IdentityIdNotFound_ShouldHaveCorrectCode()
  {
    // Act
    var error = AppUserErrors.IdentityIdNotFound;

    // Assert
    Assert.Equal("User.IdentityIdNotFound", error.Code);
  }

  [Fact]
  public void IdentityIdNotFound_ShouldHaveCorrectStatusCode()
  {
    // Act
    var error = AppUserErrors.IdentityIdNotFound;

    // Assert
    Assert.Equal(500, error.StatusCode);
  }

  [Fact]
  public void IdentityIdNotFound_ShouldHaveCorrectMessage()
  {
    // Act
    var error = AppUserErrors.IdentityIdNotFound;

    // Assert
    Assert.Equal("The identity id is not accessible", error.Name);
  }

  [Fact]
  public void IdentityIdNotFound_ShouldReturnSameInstanceOnMultipleCalls()
  {
    // Act
    var error1 = AppUserErrors.IdentityIdNotFound;
    var error2 = AppUserErrors.IdentityIdNotFound;

    // Assert
    Assert.Same(error1, error2);
  }

  [Fact]
  public void AllErrors_ShouldBeDistinct()
  {
    // Act
    var notFound = AppUserErrors.NotFound;
    var invalidCredentials = AppUserErrors.InvalidCredentials;
    var identityIdNotFound = AppUserErrors.IdentityIdNotFound;

    // Assert
    Assert.NotSame(notFound, invalidCredentials);
    Assert.NotSame(notFound, identityIdNotFound);
    Assert.NotSame(invalidCredentials, identityIdNotFound);
  }

  [Fact]
  public void AllErrors_ShouldHaveUniqueStatusCodes()
  {
    // Act
    var notFound = AppUserErrors.NotFound;
    var invalidCredentials = AppUserErrors.InvalidCredentials;
    var identityIdNotFound = AppUserErrors.IdentityIdNotFound;

    // Assert
    Assert.NotEqual(notFound.StatusCode, invalidCredentials.StatusCode);
    Assert.NotEqual(notFound.StatusCode, identityIdNotFound.StatusCode);
    Assert.NotEqual(invalidCredentials.StatusCode, identityIdNotFound.StatusCode);
  }

  [Fact]
  public void AllErrors_ShouldHaveUniqueCodes()
  {
    // Act
    var notFound = AppUserErrors.NotFound;
    var invalidCredentials = AppUserErrors.InvalidCredentials;
    var identityIdNotFound = AppUserErrors.IdentityIdNotFound;

    // Assert
    Assert.NotEqual(notFound.Code, invalidCredentials.Code);
    Assert.NotEqual(notFound.Code, identityIdNotFound.Code);
    Assert.NotEqual(invalidCredentials.Code, identityIdNotFound.Code);
  }

  [Fact]
  public void AllErrors_ShouldHaveUniqueMessages()
  {
    // Act
    var notFound = AppUserErrors.NotFound;
    var invalidCredentials = AppUserErrors.InvalidCredentials;
    var identityIdNotFound = AppUserErrors.IdentityIdNotFound;

    // Assert
    Assert.NotEqual(notFound.Name, invalidCredentials.Name);
    Assert.NotEqual(notFound.Name, identityIdNotFound.Name);
    Assert.NotEqual(invalidCredentials.Name, identityIdNotFound.Name);
  }

  [Fact]
  public void NotFound_ShouldHaveHttpNotFoundStatusCode()
  {
    // Act
    var error = AppUserErrors.NotFound;

    // Assert
    Assert.Equal(404, error.StatusCode);
    Assert.True(error.StatusCode >= 400 && error.StatusCode < 500);
  }

  [Fact]
  public void InvalidCredentials_ShouldHaveHttpUnauthorizedStatusCode()
  {
    // Act
    var error = AppUserErrors.InvalidCredentials;

    // Assert
    Assert.Equal(401, error.StatusCode);
    Assert.True(error.StatusCode >= 400 && error.StatusCode < 500);
  }

  [Fact]
  public void IdentityIdNotFound_ShouldHaveHttpInternalServerErrorStatusCode()
  {
    // Act
    var error = AppUserErrors.IdentityIdNotFound;

    // Assert
    Assert.Equal(500, error.StatusCode);
    Assert.True(error.StatusCode >= 500);
  }

  [Fact]
  public void AllErrors_CodesShouldStartWithUserPrefix()
  {
    // Act
    var notFound = AppUserErrors.NotFound;
    var invalidCredentials = AppUserErrors.InvalidCredentials;
    var identityIdNotFound = AppUserErrors.IdentityIdNotFound;

    // Assert
    Assert.StartsWith("User.", notFound.Code);
    Assert.StartsWith("User.", invalidCredentials.Code);
    Assert.StartsWith("User.", identityIdNotFound.Code);
  }

  [Fact]
  public void AllErrors_ShouldNotBeNull()
  {
    // Act & Assert
    Assert.NotNull(AppUserErrors.NotFound);
    Assert.NotNull(AppUserErrors.InvalidCredentials);
    Assert.NotNull(AppUserErrors.IdentityIdNotFound);
  }

  [Fact]
  public void AllErrors_ShouldHaveNonEmptyCode()
  {
    // Act
    var notFound = AppUserErrors.NotFound;
    var invalidCredentials = AppUserErrors.InvalidCredentials;
    var identityIdNotFound = AppUserErrors.IdentityIdNotFound;

    // Assert
    Assert.False(string.IsNullOrWhiteSpace(notFound.Code));
    Assert.False(string.IsNullOrWhiteSpace(invalidCredentials.Code));
    Assert.False(string.IsNullOrWhiteSpace(identityIdNotFound.Code));
  }

  [Fact]
  public void AllErrors_ShouldHaveNonEmptyMessage()
  {
    // Act
    var notFound = AppUserErrors.NotFound;
    var invalidCredentials = AppUserErrors.InvalidCredentials;
    var identityIdNotFound = AppUserErrors.IdentityIdNotFound;

    // Assert
    Assert.False(string.IsNullOrWhiteSpace(notFound.Name));
    Assert.False(string.IsNullOrWhiteSpace(invalidCredentials.Name));
    Assert.False(string.IsNullOrWhiteSpace(identityIdNotFound.Name));
  }

  [Fact]
  public void AllErrors_ShouldHavePositiveStatusCode()
  {
    // Act
    var notFound = AppUserErrors.NotFound;
    var invalidCredentials = AppUserErrors.InvalidCredentials;
    var identityIdNotFound = AppUserErrors.IdentityIdNotFound;

    // Assert
    Assert.True(notFound.StatusCode > 0);
    Assert.True(invalidCredentials.StatusCode > 0);
    Assert.True(identityIdNotFound.StatusCode > 0);
  }

  [Fact]
  public void NotFound_AccessedMultipleTimes_ShouldMaintainConsistentState()
  {
    // Act
    var error1 = AppUserErrors.NotFound;
    var code1 = error1.Code;
    var statusCode1 = error1.StatusCode;
    var message1 = error1.Name;

    var error2 = AppUserErrors.NotFound;
    var code2 = error2.Code;
    var statusCode2 = error2.StatusCode;
    var message2 = error2.Name;

    // Assert
    Assert.Equal(code1, code2);
    Assert.Equal(statusCode1, statusCode2);
    Assert.Equal(message1, message2);
  }

  [Fact]
  public void InvalidCredentials_AccessedMultipleTimes_ShouldMaintainConsistentState()
  {
    // Act
    var error1 = AppUserErrors.InvalidCredentials;
    var code1 = error1.Code;
    var statusCode1 = error1.StatusCode;
    var message1 = error1.Name;

    var error2 = AppUserErrors.InvalidCredentials;
    var code2 = error2.Code;
    var statusCode2 = error2.StatusCode;
    var message2 = error2.Name;

    // Assert
    Assert.Equal(code1, code2);
    Assert.Equal(statusCode1, statusCode2);
    Assert.Equal(message1, message2);
  }

  [Fact]
  public void IdentityIdNotFound_AccessedMultipleTimes_ShouldMaintainConsistentState()
  {
    // Act
    var error1 = AppUserErrors.IdentityIdNotFound;
    var code1 = error1.Code;
    var statusCode1 = error1.StatusCode;
    var message1 = error1.Name;

    var error2 = AppUserErrors.IdentityIdNotFound;
    var code2 = error2.Code;
    var statusCode2 = error2.StatusCode;
    var message2 = error2.Name;

    // Assert
    Assert.Equal(code1, code2);
    Assert.Equal(statusCode1, statusCode2);
    Assert.Equal(message1, message2);
  }

  [Theory]
  [InlineData("User.NotFound", 404, "The user with the specified identifier was not found")]
  [InlineData("User.InvalidCredentials", 401, "The provided credentials were invalid")]
  [InlineData("User.IdentityIdNotFound", 500, "The identity id is not accessible")]
  public void AllErrors_ShouldMatchExpectedValues(string expectedCode, int expectedStatusCode, string expectedMessage)
  {
    // Act
    var error = expectedCode switch
    {
      "User.NotFound" => AppUserErrors.NotFound,
      "User.InvalidCredentials" => AppUserErrors.InvalidCredentials,
      "User.IdentityIdNotFound" => AppUserErrors.IdentityIdNotFound,
      _ => throw new ArgumentException("Unknown error code")
    };

    // Assert
    Assert.Equal(expectedCode, error.Code);
    Assert.Equal(expectedStatusCode, error.StatusCode);
    Assert.Equal(expectedMessage, error.Name);
  }

  [Fact]
  public void AllErrorFields_ShouldBeReadonly()
  {
    // This test verifies that errors are readonly by ensuring they maintain their values
    // Act
    var notFound1 = AppUserErrors.NotFound;
    var notFound2 = AppUserErrors.NotFound;

    var invalidCreds1 = AppUserErrors.InvalidCredentials;
    var invalidCreds2 = AppUserErrors.InvalidCredentials;

    var identityNotFound1 = AppUserErrors.IdentityIdNotFound;
    var identityNotFound2 = AppUserErrors.IdentityIdNotFound;

    // Assert - Same instance confirms they are readonly static fields
    Assert.Same(notFound1, notFound2);
    Assert.Same(invalidCreds1, invalidCreds2);
    Assert.Same(identityNotFound1, identityNotFound2);
  }

  [Fact]
  public void AllErrors_ShouldBeAccessibleFromStaticClass()
  {
    // Act - Verify all errors can be accessed without instantiation
    var notFound = AppUserErrors.NotFound;
    var invalidCredentials = AppUserErrors.InvalidCredentials;
    var identityIdNotFound = AppUserErrors.IdentityIdNotFound;

    // Assert
    Assert.NotNull(notFound);
    Assert.NotNull(invalidCredentials);
    Assert.NotNull(identityIdNotFound);
    Assert.IsType<DomainError>(notFound);
    Assert.IsType<DomainError>(invalidCredentials);
    Assert.IsType<DomainError>(identityIdNotFound);
  }
}
