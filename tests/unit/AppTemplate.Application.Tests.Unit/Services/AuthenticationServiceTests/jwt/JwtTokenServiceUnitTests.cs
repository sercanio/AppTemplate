using AppTemplate.Application.Authentication.Jwt;

namespace AppTemplate.Application.Tests.Unit.Services.AuthenticationServiceTests.jwt;

public class JwtTokenServiceUnitTests
{
  [Fact]
  public void JwtTokenResult_CanBeConstructed_WithRequiredParameters()
  {
    // Arrange
    var accessToken = "access-token";
    var refreshToken = "refresh-token";
    var expiresAt = DateTime.UtcNow.AddHours(1);

    // Act
    var result = new JwtTokenResult(accessToken, refreshToken, expiresAt);

    // Assert
    Assert.Equal(accessToken, result.AccessToken);
    Assert.Equal(refreshToken, result.RefreshToken);
    Assert.Equal(expiresAt, result.ExpiresAt);
    Assert.Equal("Bearer", result.TokenType);
  }

  [Fact]
  public void JwtTokenResult_CanBeConstructed_WithCustomTokenType()
  {
    // Arrange
    var accessToken = "access-token";
    var refreshToken = "refresh-token";
    var expiresAt = DateTime.UtcNow.AddHours(1);
    var tokenType = "CustomType";

    // Act
    var result = new JwtTokenResult(accessToken, refreshToken, expiresAt, tokenType);

    // Assert
    Assert.Equal(accessToken, result.AccessToken);
    Assert.Equal(refreshToken, result.RefreshToken);
    Assert.Equal(expiresAt, result.ExpiresAt);
    Assert.Equal(tokenType, result.TokenType);
  }

  [Fact]
  public void JwtTokenResult_Equality_WorksAsExpected()
  {
    // Arrange
    var expiresAt = DateTime.UtcNow.AddHours(1);
    var result1 = new JwtTokenResult("a", "b", expiresAt, "Bearer");
    var result2 = new JwtTokenResult("a", "b", expiresAt, "Bearer");
    var result3 = new JwtTokenResult("x", "y", expiresAt, "Other");

    // Act & Assert
    Assert.Equal(result1, result2);
    Assert.NotEqual(result1, result3);
  }
}