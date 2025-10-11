using AppTemplate.Application.Services.AppUsers;
using AppTemplate.Application.Services.Authentication;
using AppTemplate.Application.Services.Authentication.Models;
using AppTemplate.Application.Services.Roles;
using AppTemplate.Infrastructure;
using AppTemplate.Infrastructure.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;

namespace AppTemplate.Application.Tests.Unit.Services.AuthenticationServiceTests;

public class JwtTokenServiceUnitTests : IDisposable
{
  private readonly ApplicationDbContext _context;
  private readonly JwtTokenService _jwtTokenService;
  private readonly Mock<UserManager<IdentityUser>> _mockUserManager;
  private readonly Mock<IConfiguration> _mockConfiguration;
  private readonly Mock<ILogger<JwtTokenService>> _mockLogger;
  private readonly Mock<IAppUsersService> _mockAppUsersService;
  private readonly Mock<IRolesService> _mockRolesService;

  public JwtTokenServiceUnitTests()
  {
    // Setup in-memory database
    var options = new DbContextOptionsBuilder<ApplicationDbContext>()
        .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
        .Options;

    var mockDateTimeProvider = new Mock<AppTemplate.Application.Services.Clock.IDateTimeProvider>();
    mockDateTimeProvider.Setup(x => x.UtcNow).Returns(DateTime.UtcNow);

    _context = new ApplicationDbContext(options, mockDateTimeProvider.Object);

    // Setup mocks
    _mockUserManager = CreateMockUserManager();
    _mockConfiguration = new Mock<IConfiguration>();
    _mockLogger = new Mock<ILogger<JwtTokenService>>();
    _mockAppUsersService = new Mock<IAppUsersService>();
    _mockRolesService = new Mock<IRolesService>();

    // Setup configuration
    SetupConfiguration();

    // Create the service
    _jwtTokenService = new JwtTokenService(
        _mockUserManager.Object,
        _mockConfiguration.Object,
        _context,
        _mockLogger.Object,
        _mockAppUsersService.Object,
        _mockRolesService.Object
    );
  }

  private void SetupConfiguration()
  {
    _mockConfiguration.Setup(x => x["Jwt:Secret"]).Returns("ThisIsAVeryLongSecretKeyForTestingPurposesOnly123456789");
    _mockConfiguration.Setup(x => x["Jwt:Issuer"]).Returns("TestIssuer");
    _mockConfiguration.Setup(x => x["Jwt:Audience"]).Returns("TestAudience");
    _mockConfiguration.Setup(x => x["Jwt:ExpiryInMinutes"]).Returns("15");
    _mockConfiguration.Setup(x => x["Jwt:RefreshTokenExpiryInDays"]).Returns("7");
  }

  private static Mock<UserManager<IdentityUser>> CreateMockUserManager()
  {
    var store = new Mock<IUserStore<IdentityUser>>();
    var mgr = new Mock<UserManager<IdentityUser>>(
        store.Object, null, null, null, null, null, null, null, null);
    mgr.Object.UserValidators.Add(new UserValidator<IdentityUser>());
    mgr.Object.PasswordValidators.Add(new PasswordValidator<IdentityUser>());
    return mgr;
  }

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

  [Fact]
  public async Task RevokeOtherUserRefreshTokensAsync_ShouldRevokeOtherTokens_ExceptCurrentJti()
  {
    // Arrange
    var userId = "user123";
    var currentJti = "current-jti-123";
    var otherJti1 = "other-jti-456";
    var otherJti2 = "other-jti-789";

    var currentToken = new RefreshToken
    {
      Token = "current-token",
      UserId = userId,
      AccessTokenJti = currentJti,
      IsRevoked = false,
      ExpiresAt = DateTime.UtcNow.AddDays(1),
      CreatedAt = DateTime.UtcNow,
      LastUsedAt = DateTime.UtcNow,
      IsCurrent = true
    };

    var otherToken1 = new RefreshToken
    {
      Token = "other-token-1",
      UserId = userId,
      AccessTokenJti = otherJti1,
      IsRevoked = false,
      ExpiresAt = DateTime.UtcNow.AddDays(1),
      CreatedAt = DateTime.UtcNow,
      LastUsedAt = DateTime.UtcNow,
      IsCurrent = true
    };

    var otherToken2 = new RefreshToken
    {
      Token = "other-token-2",
      UserId = userId,
      AccessTokenJti = otherJti2,
      IsRevoked = false,
      ExpiresAt = DateTime.UtcNow.AddDays(1),
      CreatedAt = DateTime.UtcNow,
      LastUsedAt = DateTime.UtcNow,
      IsCurrent = true
    };

    var differentUserToken = new RefreshToken
    {
      Token = "different-user-token",
      UserId = "different-user",
      AccessTokenJti = "different-jti",
      IsRevoked = false,
      ExpiresAt = DateTime.UtcNow.AddDays(1),
      CreatedAt = DateTime.UtcNow,
      LastUsedAt = DateTime.UtcNow,
      IsCurrent = true
    };

    _context.RefreshTokens.AddRange(currentToken, otherToken1, otherToken2, differentUserToken);
    await _context.SaveChangesAsync();

    // Act
    await _jwtTokenService.RevokeOtherUserRefreshTokensAsync(userId, currentJti);

    // Assert
    var tokens = _context.RefreshTokens.ToList();
    
    // Current token should remain active
    var currentTokenResult = tokens.First(t => t.AccessTokenJti == currentJti);
    Assert.False(currentTokenResult.IsRevoked);
    Assert.True(currentTokenResult.IsCurrent);

    // Other tokens for same user should be revoked
    var otherToken1Result = tokens.First(t => t.AccessTokenJti == otherJti1);
    Assert.True(otherToken1Result.IsRevoked);
    Assert.Equal("Other tokens revoked", otherToken1Result.RevokedReason);
    Assert.False(otherToken1Result.IsCurrent);

    var otherToken2Result = tokens.First(t => t.AccessTokenJti == otherJti2);
    Assert.True(otherToken2Result.IsRevoked);
    Assert.Equal("Other tokens revoked", otherToken2Result.RevokedReason);
    Assert.False(otherToken2Result.IsCurrent);

    // Different user's token should remain unchanged
    var differentUserTokenResult = tokens.First(t => t.UserId == "different-user");
    Assert.False(differentUserTokenResult.IsRevoked);
  }

  [Fact]
  public async Task RevokeOtherUserRefreshTokensAsync_ShouldNotRevokeAlreadyRevokedTokens()
  {
    // Arrange
    var userId = "user123";
    var currentJti = "current-jti-123";
    var otherJti = "other-jti-456";

    var currentToken = new RefreshToken
    {
      Token = "current-token",
      UserId = userId,
      AccessTokenJti = currentJti,
      IsRevoked = false,
      ExpiresAt = DateTime.UtcNow.AddDays(1),
      CreatedAt = DateTime.UtcNow,
      LastUsedAt = DateTime.UtcNow,
      IsCurrent = true
    };

    var alreadyRevokedToken = new RefreshToken
    {
      Token = "already-revoked-token",
      UserId = userId,
      AccessTokenJti = otherJti,
      IsRevoked = true,
      RevokedReason = "Previously revoked",
      ExpiresAt = DateTime.UtcNow.AddDays(1),
      CreatedAt = DateTime.UtcNow,
      LastUsedAt = DateTime.UtcNow,
      IsCurrent = false
    };

    _context.RefreshTokens.AddRange(currentToken, alreadyRevokedToken);
    await _context.SaveChangesAsync();

    // Act
    await _jwtTokenService.RevokeOtherUserRefreshTokensAsync(userId, currentJti);

    // Assert
    var tokens = _context.RefreshTokens.ToList();
    
    // Current token should remain active
    var currentTokenResult = tokens.First(t => t.AccessTokenJti == currentJti);
    Assert.False(currentTokenResult.IsRevoked);

    // Already revoked token should keep original revocation reason
    var revokedTokenResult = tokens.First(t => t.AccessTokenJti == otherJti);
    Assert.True(revokedTokenResult.IsRevoked);
    Assert.Equal("Previously revoked", revokedTokenResult.RevokedReason);
  }

  [Fact]
  public async Task RevokeOtherUserRefreshTokensAsync_ShouldHandleEmptyTokenList()
  {
    // Arrange
    var userId = "user123";
    var currentJti = "current-jti-123";

    // Act & Assert - Should not throw exception
    await _jwtTokenService.RevokeOtherUserRefreshTokensAsync(userId, currentJti);
    
    // No tokens should exist
    var tokens = _context.RefreshTokens.ToList();
    Assert.Empty(tokens);
  }

  [Fact]
  public async Task RevokeOtherUserRefreshTokensAsync_ShouldOnlyRevokeCurrentUserTokens()
  {
    // Arrange
    var userId = "user123";
    var otherUserId = "user456";
    var currentJti = "current-jti-123";
    var otherUserJti = "other-user-jti";

    var currentUserToken = new RefreshToken
    {
      Token = "current-user-token",
      UserId = userId,
      AccessTokenJti = currentJti,
      IsRevoked = false,
      ExpiresAt = DateTime.UtcNow.AddDays(1),
      CreatedAt = DateTime.UtcNow,
      LastUsedAt = DateTime.UtcNow,
      IsCurrent = true
    };

    var otherUserToken = new RefreshToken
    {
      Token = "other-user-token",
      UserId = otherUserId,
      AccessTokenJti = otherUserJti,
      IsRevoked = false,
      ExpiresAt = DateTime.UtcNow.AddDays(1),
      CreatedAt = DateTime.UtcNow,
      LastUsedAt = DateTime.UtcNow,
      IsCurrent = true
    };

    _context.RefreshTokens.AddRange(currentUserToken, otherUserToken);
    await _context.SaveChangesAsync();

    // Act
    await _jwtTokenService.RevokeOtherUserRefreshTokensAsync(userId, currentJti);

    // Assert
    var tokens = _context.RefreshTokens.ToList();
    
    // Current user's token should remain active
    var currentUserTokenResult = tokens.First(t => t.UserId == userId);
    Assert.False(currentUserTokenResult.IsRevoked);

    // Other user's token should remain unchanged
    var otherUserTokenResult = tokens.First(t => t.UserId == otherUserId);
    Assert.False(otherUserTokenResult.IsRevoked);
  }

  public void Dispose()
  {
    _context?.Dispose();
  }
}