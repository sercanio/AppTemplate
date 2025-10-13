using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using AppTemplate.Application.Services.AppUsers;
using AppTemplate.Application.Services.Authentication;
using AppTemplate.Application.Services.Authentication.Models;
using AppTemplate.Application.Services.Roles;
using AppTemplate.Domain.AppUsers;
using AppTemplate.Domain.Roles;
using AppTemplate.Infrastructure.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Moq;

namespace AppTemplate.Infrastructure.Tests.Unit.AuthenticationTests;

[Trait("Category", "Unit")]
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

    var mockDateTimeProvider = new Mock<Application.Services.Clock.IDateTimeProvider>();
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
        store.Object, null!, null!, null!, null!, null!, null!, null!, null!);
    mgr.Object.UserValidators.Add(new UserValidator<IdentityUser>());
    mgr.Object.PasswordValidators.Add(new PasswordValidator<IdentityUser>());
    return mgr;
  }

  #region JwtTokenResult Tests

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

  #endregion

  #region RefreshToken Entity Tests

  [Fact]
  public void RefreshToken_CanSetAndGetDeviceInformation()
  {
    // Arrange & Act
    var refreshToken = new RefreshToken
    {
      Token = "test-token",
      UserId = "user-123",
      ExpiresAt = DateTime.UtcNow.AddDays(7),
      CreatedAt = DateTime.UtcNow,
      LastUsedAt = DateTime.UtcNow,
      IsRevoked = false,
      IsCurrent = true,
      DeviceName = "Windows - Chrome",
      UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36",
      IpAddress = "192.168.1.1",
      Platform = "Windows",
      Browser = "Chrome",
      AccessTokenJti = "jti-123"
    };

    // Assert
    Assert.Equal("test-token", refreshToken.Token);
    Assert.Equal("user-123", refreshToken.UserId);
    Assert.False(refreshToken.IsRevoked);
    Assert.True(refreshToken.IsCurrent);
    Assert.Equal("Windows - Chrome", refreshToken.DeviceName);
    Assert.Equal("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36", refreshToken.UserAgent);
    Assert.Equal("192.168.1.1", refreshToken.IpAddress);
    Assert.Equal("Windows", refreshToken.Platform);
    Assert.Equal("Chrome", refreshToken.Browser);
    Assert.Equal("jti-123", refreshToken.AccessTokenJti);
  }

  [Fact]
  public void RefreshToken_CanSetReplacedByToken()
  {
    // Arrange & Act
    var refreshToken = new RefreshToken
    {
      Token = "old-token",
      UserId = "user-123",
      ExpiresAt = DateTime.UtcNow.AddDays(7),
      CreatedAt = DateTime.UtcNow,
      LastUsedAt = DateTime.UtcNow,
      IsRevoked = true,
      RevokedReason = "Replaced by new token",
      ReplacedByToken = "new-token-456",
      IsCurrent = false,
      AccessTokenJti = "jti-old"
    };

    // Assert
    Assert.Equal("old-token", refreshToken.Token);
    Assert.True(refreshToken.IsRevoked);
    Assert.Equal("Replaced by new token", refreshToken.RevokedReason);
    Assert.Equal("new-token-456", refreshToken.ReplacedByToken);
    Assert.False(refreshToken.IsCurrent);
  }

  [Fact]
  public void RefreshToken_CanHaveNullDeviceInformation()
  {
    // Arrange & Act
    var refreshToken = new RefreshToken
    {
      Token = "test-token",
      UserId = "user-123",
      ExpiresAt = DateTime.UtcNow.AddDays(7),
      CreatedAt = DateTime.UtcNow,
      LastUsedAt = DateTime.UtcNow,
      IsRevoked = false,
      IsCurrent = true,
      DeviceName = null,
      UserAgent = null,
      IpAddress = null,
      Platform = null,
      Browser = null,
      AccessTokenJti = "jti-123"
    };

    // Assert
    Assert.Null(refreshToken.DeviceName);
    Assert.Null(refreshToken.UserAgent);
    Assert.Null(refreshToken.IpAddress);
    Assert.Null(refreshToken.Platform);
    Assert.Null(refreshToken.Browser);
    Assert.Null(refreshToken.RevokedReason);
    Assert.Null(refreshToken.ReplacedByToken);
  }

  #endregion

  #region GenerateTokensAsync Tests

  [Fact]
  public async Task GenerateTokensAsync_ShouldGenerateValidTokens()
  {
    // Arrange
    var user = new IdentityUser
    {
      Id = "user-123",
      Email = "test@example.com",
      UserName = "testuser"
    };

    var appUser = AppUser.Create();
    appUser.SetIdentityId(user.Id);
    appUser.AddRole(Role.DefaultRole);

    _context.AppUsers.Add(appUser);
    await _context.SaveChangesAsync();

    var deviceInfo = new DeviceInfo("Mozilla/5.0", "192.168.1.1", "Windows - Chrome", "Windows", "Chrome");

    // Act
    var result = await _jwtTokenService.GenerateTokensAsync(user, appUser, deviceInfo);

    // Assert
    Assert.NotNull(result);
    Assert.NotEmpty(result.AccessToken);
    Assert.NotEmpty(result.RefreshToken);
    Assert.True(result.ExpiresAt > DateTime.UtcNow);

    // Verify refresh token was saved
    var savedToken = await _context.RefreshTokens.FirstOrDefaultAsync(rt => rt.UserId == user.Id);
    Assert.NotNull(savedToken);
    Assert.Equal(result.RefreshToken, savedToken.Token);
    Assert.True(savedToken.IsCurrent);
    Assert.Equal("Windows - Chrome", savedToken.DeviceName);
  }

  [Fact]
  public async Task GenerateTokensAsync_ShouldMarkPreviousTokensAsNotCurrent()
  {
    // Arrange
    var user = new IdentityUser
    {
      Id = "user-456",
      Email = "test2@example.com",
      UserName = "testuser2"
    };

    var appUser = AppUser.Create();
    appUser.SetIdentityId(user.Id);
    appUser.AddRole(Role.DefaultRole);

    // Add existing token
    var existingToken = new RefreshToken
    {
      Token = "old-token",
      UserId = user.Id,
      ExpiresAt = DateTime.UtcNow.AddDays(7),
      CreatedAt = DateTime.UtcNow,
      LastUsedAt = DateTime.UtcNow,
      IsCurrent = true,
      IsRevoked = false,
      AccessTokenJti = "old-jti"
    };

    _context.RefreshTokens.Add(existingToken);
    _context.AppUsers.Add(appUser);
    await _context.SaveChangesAsync();

    // Act
    await _jwtTokenService.GenerateTokensAsync(user, appUser, null);

    // Assert
    var oldToken = await _context.RefreshTokens.FirstOrDefaultAsync(rt => rt.Token == "old-token");
    Assert.NotNull(oldToken);
    Assert.False(oldToken.IsCurrent);
  }

  [Fact]
  public async Task GenerateTokensAsync_ShouldIncludeRolesInAccessToken()
  {
    // Arrange
    var user = new IdentityUser
    {
      Id = "user-789",
      Email = "admin@example.com",
      UserName = "adminuser"
    };

    var appUser = AppUser.Create();
    appUser.SetIdentityId(user.Id);
    appUser.AddRole(Role.Admin);

    _context.AppUsers.Add(appUser);
    await _context.SaveChangesAsync();

    // Act
    var result = await _jwtTokenService.GenerateTokensAsync(user, appUser, null);

    // Assert
    var handler = new JwtSecurityTokenHandler();
    var jsonToken = handler.ReadJwtToken(result.AccessToken);
    var roleClaims = jsonToken.Claims.Where(c => c.Type == "roles").ToList();

    Assert.NotEmpty(roleClaims);
    Assert.Contains(roleClaims, c => c.Value == "Admin");
  }

  #endregion

  #region RefreshTokensAsync Tests

  [Fact]
  public async Task RefreshTokensAsync_WithValidToken_ShouldReturnNewTokens()
  {
    // Arrange
    var user = new IdentityUser
    {
      Id = "user-refresh-1",
      Email = "refresh@example.com",
      UserName = "refreshuser"
    };

    var appUser = AppUser.Create();
    appUser.SetIdentityId(user.Id);
    appUser.AddRole(Role.DefaultRole);

    var refreshToken = new RefreshToken
    {
      Token = "valid-refresh-token",
      UserId = user.Id,
      ExpiresAt = DateTime.UtcNow.AddDays(7),
      CreatedAt = DateTime.UtcNow,
      LastUsedAt = DateTime.UtcNow,
      IsCurrent = true,
      IsRevoked = false,
      AccessTokenJti = "old-jti",
      IpAddress = "192.168.1.1"
    };

    _context.RefreshTokens.Add(refreshToken);
    _context.AppUsers.Add(appUser);
    await _context.SaveChangesAsync();

    _mockUserManager.Setup(um => um.FindByIdAsync(user.Id))
        .ReturnsAsync(user);

    var deviceInfo = new DeviceInfo("Mozilla/5.0", "192.168.1.2", "Mobile - Safari", "iOS", "Safari");

    // Act
    var result = await _jwtTokenService.RefreshTokensAsync("valid-refresh-token", deviceInfo);

    // Assert
    Assert.NotNull(result);
    Assert.NotEmpty(result.AccessToken);
    Assert.NotEmpty(result.RefreshToken);

    // Verify old token was revoked
    var oldToken = await _context.RefreshTokens.FirstOrDefaultAsync(rt => rt.Token == "valid-refresh-token");
    Assert.NotNull(oldToken);
    Assert.True(oldToken.IsRevoked);
    Assert.Equal("Replaced by new token", oldToken.RevokedReason);
    Assert.Equal(result.RefreshToken, oldToken.ReplacedByToken);
    Assert.Equal("192.168.1.2", oldToken.IpAddress);
  }

  [Fact]
  public async Task RefreshTokensAsync_WithExpiredToken_ShouldThrowException()
  {
    // Arrange
    var expiredToken = new RefreshToken
    {
      Token = "expired-token",
      UserId = "user-exp",
      ExpiresAt = DateTime.UtcNow.AddDays(-1), // Expired
      CreatedAt = DateTime.UtcNow.AddDays(-8),
      LastUsedAt = DateTime.UtcNow.AddDays(-1),
      IsCurrent = true,
      IsRevoked = false,
      AccessTokenJti = "exp-jti"
    };

    _context.RefreshTokens.Add(expiredToken);
    await _context.SaveChangesAsync();

    // Act & Assert
    await Assert.ThrowsAsync<SecurityTokenValidationException>(
        () => _jwtTokenService.RefreshTokensAsync("expired-token", null));
  }

  [Fact]
  public async Task RefreshTokensAsync_WithRevokedToken_ShouldThrowException()
  {
    // Arrange
    var revokedToken = new RefreshToken
    {
      Token = "revoked-token",
      UserId = "user-rev",
      ExpiresAt = DateTime.UtcNow.AddDays(7),
      CreatedAt = DateTime.UtcNow,
      LastUsedAt = DateTime.UtcNow,
      IsCurrent = false,
      IsRevoked = true,
      RevokedReason = "User logged out",
      AccessTokenJti = "rev-jti"
    };

    _context.RefreshTokens.Add(revokedToken);
    await _context.SaveChangesAsync();

    // Act & Assert
    await Assert.ThrowsAsync<SecurityTokenValidationException>(
        () => _jwtTokenService.RefreshTokensAsync("revoked-token", null));
  }

  [Fact]
  public async Task RefreshTokensAsync_WithInvalidToken_ShouldThrowException()
  {
    // Act & Assert
    await Assert.ThrowsAsync<SecurityTokenValidationException>(
        () => _jwtTokenService.RefreshTokensAsync("non-existent-token", null));
  }

  [Fact]
  public async Task RefreshTokensAsync_WithNonExistentUser_ShouldThrowException()
  {
    // Arrange
    var refreshToken = new RefreshToken
    {
      Token = "orphan-token",
      UserId = "non-existent-user",
      ExpiresAt = DateTime.UtcNow.AddDays(7),
      CreatedAt = DateTime.UtcNow,
      LastUsedAt = DateTime.UtcNow,
      IsCurrent = true,
      IsRevoked = false,
      AccessTokenJti = "orphan-jti"
    };

    _context.RefreshTokens.Add(refreshToken);
    await _context.SaveChangesAsync();

    _mockUserManager.Setup(um => um.FindByIdAsync("non-existent-user"))
        .ReturnsAsync((IdentityUser)null!);

    // Act & Assert
    await Assert.ThrowsAsync<SecurityTokenValidationException>(
        () => _jwtTokenService.RefreshTokensAsync("orphan-token", null));
  }

  [Fact]
  public async Task RefreshTokensAsync_LegacyMethod_ShouldCallNewMethod()
  {
    // Arrange
    var user = new IdentityUser
    {
      Id = "user-legacy",
      Email = "legacy@example.com",
      UserName = "legacyuser"
    };

    var appUser = AppUser.Create();
    appUser.SetIdentityId(user.Id);

    // Create a new role instance and add to context
    var userRole = Role.Create("Registered", "Registered User", appUser.Id);
    _context.Roles.Add(userRole);
    await _context.SaveChangesAsync();

    // Add role to user
    appUser.AddRole(userRole);

    _context.AppUsers.Add(appUser);
    await _context.SaveChangesAsync();

    // Clear change tracker and re-fetch user with roles
    _context.ChangeTracker.Clear();
    var dbUser = await _context.AppUsers
        .Include(u => u.Roles)
        .FirstAsync(u => u.Id == appUser.Id);

    var refreshToken = new RefreshToken
    {
      Token = "legacy-token",
      UserId = user.Id,
      ExpiresAt = DateTime.UtcNow.AddDays(7),
      CreatedAt = DateTime.UtcNow,
      LastUsedAt = DateTime.UtcNow,
      IsCurrent = true,
      IsRevoked = false,
      AccessTokenJti = "legacy-jti"
    };

    _context.RefreshTokens.Add(refreshToken);
    await _context.SaveChangesAsync();

    _mockUserManager.Setup(um => um.FindByIdAsync(user.Id))
        .ReturnsAsync(user);

    // Act
    var result = await _jwtTokenService.RefreshTokensAsync("legacy-token");

    // Assert
    Assert.NotNull(result);
    Assert.NotEmpty(result.AccessToken);
  }

  #endregion

  #region ValidateToken Tests

  [Fact]
  public async Task ValidateToken_WithValidToken_ShouldReturnClaimsPrincipal()
  {
    // Arrange
    var user = new IdentityUser
    {
      Id = "user-validate",
      Email = "validate@example.com",
      UserName = "validateuser"
    };

    var appUser = AppUser.Create();
    appUser.SetIdentityId(user.Id);
    appUser.AddRole(Role.DefaultRole);

    _context.AppUsers.Add(appUser);
    await _context.SaveChangesAsync();

    var tokens = await _jwtTokenService.GenerateTokensAsync(user, appUser, null);

    // Act
    var principal = _jwtTokenService.ValidateToken(tokens.AccessToken);

    // Assert
    Assert.NotNull(principal);
    Assert.NotNull(principal.Identity);
    Assert.True(principal.Identity.IsAuthenticated);
    Assert.Contains(principal.Claims, c => c.Type == ClaimTypes.NameIdentifier && c.Value == user.Id);
  }

  [Fact]
  public void ValidateToken_WithInvalidToken_ShouldReturnNull()
  {
    // Arrange
    var invalidToken = "invalid.token.here";

    // Act
    var principal = _jwtTokenService.ValidateToken(invalidToken);

    // Assert
    Assert.Null(principal);
  }

  [Fact]
  public void ValidateToken_WithExpiredToken_ShouldReturnNull()
  {
    // Arrange
    _mockConfiguration.Setup(x => x["Jwt:ExpiryInMinutes"]).Returns("-1"); // Force expired token
    var expiredService = new JwtTokenService(
        _mockUserManager.Object,
        _mockConfiguration.Object,
        _context,
        _mockLogger.Object,
        _mockAppUsersService.Object,
        _mockRolesService.Object
    );

    // Create token (will be instantly expired)
    var user = new IdentityUser { Id = "exp-user", Email = "exp@test.com", UserName = "expuser" };
    var appUser = AppUser.Create();
    appUser.SetIdentityId(user.Id);

    // Reset config for validation
    SetupConfiguration();

    var claims = new List<Claim>
    {
      new(JwtRegisteredClaimNames.Sub, user.Id),
      new(JwtRegisteredClaimNames.Email, user.Email)
    };

    var key = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(_mockConfiguration.Object["Jwt:Secret"]!));
    var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

    var token = new JwtSecurityToken(
        _mockConfiguration.Object["Jwt:Issuer"],
        _mockConfiguration.Object["Jwt:Audience"],
        claims,
        expires: DateTime.UtcNow.AddMinutes(-10), // Expired
        signingCredentials: creds
    );

    var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

    // Act
    var principal = _jwtTokenService.ValidateToken(tokenString);

    // Assert
    Assert.Null(principal);
  }

  #endregion

  #region RevokeRefreshTokenAsync Tests

  [Fact]
  public async Task RevokeRefreshTokenAsync_WithValidToken_ShouldRevokeToken()
  {
    // Arrange
    var token = new RefreshToken
    {
      Token = "token-to-revoke",
      UserId = "user-revoke",
      ExpiresAt = DateTime.UtcNow.AddDays(7),
      CreatedAt = DateTime.UtcNow,
      LastUsedAt = DateTime.UtcNow,
      IsCurrent = true,
      IsRevoked = false,
      AccessTokenJti = "revoke-jti"
    };

    _context.RefreshTokens.Add(token);
    await _context.SaveChangesAsync();

    // Act
    await _jwtTokenService.RevokeRefreshTokenAsync("token-to-revoke");

    // Assert
    var revokedToken = await _context.RefreshTokens.FirstOrDefaultAsync(rt => rt.Token == "token-to-revoke");
    Assert.NotNull(revokedToken);
    Assert.True(revokedToken.IsRevoked);
    Assert.Equal("Manually revoked", revokedToken.RevokedReason);
    Assert.False(revokedToken.IsCurrent);
  }

  [Fact]
  public async Task RevokeRefreshTokenAsync_WithNonExistentToken_ShouldNotThrow()
  {
    // Act & Assert - Should not throw
    await _jwtTokenService.RevokeRefreshTokenAsync("non-existent-token");
  }

  #endregion

  #region RevokeAllUserRefreshTokensAsync Tests

  [Fact]
  public async Task RevokeAllUserRefreshTokensAsync_ShouldRevokeAllUserTokens()
  {
    // Arrange
    var userId = "user-revoke-all";
    var tokens = new List<RefreshToken>
    {
      new()
      {
        Token = "token-1",
        UserId = userId,
        ExpiresAt = DateTime.UtcNow.AddDays(7),
        CreatedAt = DateTime.UtcNow,
        LastUsedAt = DateTime.UtcNow,
        IsCurrent = true,
        IsRevoked = false,
        AccessTokenJti = "jti-1"
      },
      new()
      {
        Token = "token-2",
        UserId = userId,
        ExpiresAt = DateTime.UtcNow.AddDays(7),
        CreatedAt = DateTime.UtcNow,
        LastUsedAt = DateTime.UtcNow,
        IsCurrent = true,
        IsRevoked = false,
        AccessTokenJti = "jti-2"
      },
      new()
      {
        Token = "token-3",
        UserId = "other-user",
        ExpiresAt = DateTime.UtcNow.AddDays(7),
        CreatedAt = DateTime.UtcNow,
        LastUsedAt = DateTime.UtcNow,
        IsCurrent = true,
        IsRevoked = false,
        AccessTokenJti = "jti-3"
      }
    };

    _context.RefreshTokens.AddRange(tokens);
    await _context.SaveChangesAsync();

    // Act
    await _jwtTokenService.RevokeAllUserRefreshTokensAsync(userId);

    // Assert
    var userTokens = _context.RefreshTokens.Where(rt => rt.UserId == userId).ToList();
    Assert.All(userTokens, token =>
    {
      Assert.True(token.IsRevoked);
      Assert.Equal("All tokens revoked", token.RevokedReason);
      Assert.False(token.IsCurrent);
    });

    var otherUserToken = _context.RefreshTokens.First(rt => rt.UserId == "other-user");
    Assert.False(otherUserToken.IsRevoked);
  }

  [Fact]
  public async Task RevokeAllUserRefreshTokensAsync_WithNoTokens_ShouldNotThrow()
  {
    // Act & Assert - Should not throw
    await _jwtTokenService.RevokeAllUserRefreshTokensAsync("user-no-tokens");
  }

  [Fact]
  public async Task RevokeAllUserRefreshTokensAsync_ShouldNotRevokeAlreadyRevokedTokens()
  {
    // Arrange
    var userId = "user-mixed-tokens";
    var tokens = new List<RefreshToken>
    {
      new()
      {
        Token = "active-token",
        UserId = userId,
        ExpiresAt = DateTime.UtcNow.AddDays(7),
        CreatedAt = DateTime.UtcNow,
        LastUsedAt = DateTime.UtcNow,
        IsCurrent = true,
        IsRevoked = false,
        AccessTokenJti = "active-jti"
      },
      new()
      {
        Token = "already-revoked",
        UserId = userId,
        ExpiresAt = DateTime.UtcNow.AddDays(7),
        CreatedAt = DateTime.UtcNow,
        LastUsedAt = DateTime.UtcNow,
        IsCurrent = false,
        IsRevoked = true,
        RevokedReason = "Previously revoked",
        AccessTokenJti = "revoked-jti"
      }
    };

    _context.RefreshTokens.AddRange(tokens);
    await _context.SaveChangesAsync();

    // Act
    await _jwtTokenService.RevokeAllUserRefreshTokensAsync(userId);

    // Assert
    var activeToken = _context.RefreshTokens.First(rt => rt.Token == "active-token");
    Assert.True(activeToken.IsRevoked);
    Assert.Equal("All tokens revoked", activeToken.RevokedReason);

    var alreadyRevokedToken = _context.RefreshTokens.First(rt => rt.Token == "already-revoked");
    Assert.True(alreadyRevokedToken.IsRevoked);
    Assert.Equal("Previously revoked", alreadyRevokedToken.RevokedReason); // Original reason preserved
  }

  #endregion

  #region GetUserDeviceSessionsAsync Tests

  [Fact]
  public async Task GetUserDeviceSessionsAsync_ShouldReturnActiveSessionsOnly()
  {
    // Arrange
    var userId = "user-sessions";
    var currentJti = "current-jti";
    var tokens = new List<RefreshToken>
    {
      new()
      {
        Token = "token-1",
        UserId = userId,
        ExpiresAt = DateTime.UtcNow.AddDays(7),
        CreatedAt = DateTime.UtcNow,
        LastUsedAt = DateTime.UtcNow,
        IsCurrent = true,
        IsRevoked = false,
        AccessTokenJti = currentJti,
        DeviceName = "Windows - Chrome",
        Platform = "Windows",
        Browser = "Chrome",
        IpAddress = "192.168.1.1"
      },
      new()
      {
        Token = "token-2",
        UserId = userId,
        ExpiresAt = DateTime.UtcNow.AddDays(7),
        CreatedAt = DateTime.UtcNow.AddDays(-1),
        LastUsedAt = DateTime.UtcNow.AddHours(-2),
        IsCurrent = true,
        IsRevoked = false,
        AccessTokenJti = "other-jti",
        DeviceName = "iPhone - Safari",
        Platform = "iOS",
        Browser = "Safari",
        IpAddress = "192.168.1.2"
      },
      new()
      {
        Token = "token-3",
        UserId = userId,
        ExpiresAt = DateTime.UtcNow.AddDays(-1), // Expired
        CreatedAt = DateTime.UtcNow.AddDays(-8),
        LastUsedAt = DateTime.UtcNow.AddDays(-1),
        IsCurrent = false,
        IsRevoked = false,
        AccessTokenJti = "expired-jti",
        DeviceName = "Android - Chrome"
      },
      new()
      {
        Token = "token-4",
        UserId = userId,
        ExpiresAt = DateTime.UtcNow.AddDays(7),
        CreatedAt = DateTime.UtcNow.AddDays(-2),
        LastUsedAt = DateTime.UtcNow.AddDays(-2),
        IsCurrent = false,
        IsRevoked = true, // Revoked
        AccessTokenJti = "revoked-jti",
        DeviceName = "Linux - Firefox"
      }
    };

    _context.RefreshTokens.AddRange(tokens);
    await _context.SaveChangesAsync();

    // Act
    var sessions = await _jwtTokenService.GetUserDeviceSessionsAsync(userId, currentJti);

    // Assert
    var sessionList = sessions.ToList();
    Assert.Equal(2, sessionList.Count);

    var currentSession = sessionList.First(s => s.IsCurrent);
    Assert.Equal("Windows - Chrome", currentSession.DeviceName);
    Assert.True(currentSession.IsCurrent);

    var otherSession = sessionList.First(s => !s.IsCurrent);
    Assert.Equal("iPhone - Safari", otherSession.DeviceName);
    Assert.False(otherSession.IsCurrent);
  }

  [Fact]
  public async Task GetUserDeviceSessionsAsync_WithoutCurrentJti_ShouldNotMarkAnyCurrent()
  {
    // Arrange
    var userId = "user-no-current";
    var token = new RefreshToken
    {
      Token = "token-1",
      UserId = userId,
      ExpiresAt = DateTime.UtcNow.AddDays(7),
      CreatedAt = DateTime.UtcNow,
      LastUsedAt = DateTime.UtcNow,
      IsCurrent = true,
      IsRevoked = false,
      AccessTokenJti = "some-jti",
      DeviceName = "Windows - Chrome"
    };

    _context.RefreshTokens.Add(token);
    await _context.SaveChangesAsync();

    // Act
    var sessions = await _jwtTokenService.GetUserDeviceSessionsAsync(userId, null);

    // Assert
    var sessionList = sessions.ToList();
    Assert.Single(sessionList);
    Assert.False(sessionList[0].IsCurrent);
  }

  [Fact]
  public async Task GetUserDeviceSessionsAsync_WithNoSessions_ShouldReturnEmpty()
  {
    // Act
    var sessions = await _jwtTokenService.GetUserDeviceSessionsAsync("user-no-sessions", null);

    // Assert
    Assert.Empty(sessions);
  }

  #endregion

  #region RevokeDeviceSessionAsync Tests

  [Fact]
  public async Task RevokeDeviceSessionAsync_WithValidToken_ShouldRevokeAndReturnTrue()
  {
    // Arrange
    var userId = "user-device-revoke";
    var token = new RefreshToken
    {
      Token = "device-token",
      UserId = userId,
      ExpiresAt = DateTime.UtcNow.AddDays(7),
      CreatedAt = DateTime.UtcNow,
      LastUsedAt = DateTime.UtcNow,
      IsCurrent = true,
      IsRevoked = false,
      AccessTokenJti = "device-jti",
      DeviceName = "Mobile - Chrome"
    };

    _context.RefreshTokens.Add(token);
    await _context.SaveChangesAsync();

    // Act
    var result = await _jwtTokenService.RevokeDeviceSessionAsync("device-token", userId);

    // Assert
    Assert.True(result);

    var revokedToken = await _context.RefreshTokens.FirstOrDefaultAsync(rt => rt.Token == "device-token");
    Assert.NotNull(revokedToken);
    Assert.True(revokedToken.IsRevoked);
    Assert.Equal("Revoked by user", revokedToken.RevokedReason);
    Assert.False(revokedToken.IsCurrent);
  }

  [Fact]
  public async Task RevokeDeviceSessionAsync_WithInvalidToken_ShouldReturnFalse()
  {
    // Act
    var result = await _jwtTokenService.RevokeDeviceSessionAsync("non-existent-token", "user-id");

    // Assert
    Assert.False(result);
  }

  [Fact]
  public async Task RevokeDeviceSessionAsync_WithWrongUserId_ShouldReturnFalse()
  {
    // Arrange
    var token = new RefreshToken
    {
      Token = "token-wrong-user",
      UserId = "correct-user",
      ExpiresAt = DateTime.UtcNow.AddDays(7),
      CreatedAt = DateTime.UtcNow,
      LastUsedAt = DateTime.UtcNow,
      IsCurrent = true,
      IsRevoked = false,
      AccessTokenJti = "wrong-user-jti"
    };

    _context.RefreshTokens.Add(token);
    await _context.SaveChangesAsync();

    // Act
    var result = await _jwtTokenService.RevokeDeviceSessionAsync("token-wrong-user", "wrong-user");

    // Assert
    Assert.False(result);

    var unchangedToken = await _context.RefreshTokens.FirstOrDefaultAsync(rt => rt.Token == "token-wrong-user");
    Assert.False(unchangedToken!.IsRevoked);
  }

  [Fact]
  public async Task RevokeDeviceSessionAsync_WithAlreadyRevokedToken_ShouldReturnFalse()
  {
    // Arrange
    var userId = "user-already-revoked";
    var token = new RefreshToken
    {
      Token = "already-revoked-device",
      UserId = userId,
      ExpiresAt = DateTime.UtcNow.AddDays(7),
      CreatedAt = DateTime.UtcNow,
      LastUsedAt = DateTime.UtcNow,
      IsCurrent = false,
      IsRevoked = true,
      RevokedReason = "Already revoked",
      AccessTokenJti = "already-revoked-jti"
    };

    _context.RefreshTokens.Add(token);
    await _context.SaveChangesAsync();

    // Act
    var result = await _jwtTokenService.RevokeDeviceSessionAsync("already-revoked-device", userId);

    // Assert
    Assert.False(result);
  }

  #endregion

  #region RevokeOtherUserRefreshTokensAsync Tests

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

  #endregion

  #region DeviceInfo Tests

  [Fact]
  public void DeviceInfo_CanBeConstructed_WithAllParameters()
  {
    // Arrange & Act
    var deviceInfo = new DeviceInfo(
        UserAgent: "Mozilla/5.0",
        IpAddress: "192.168.1.1",
        DeviceName: "Windows - Chrome",
        Platform: "Windows",
        Browser: "Chrome"
    );

    // Assert
    Assert.Equal("Mozilla/5.0", deviceInfo.UserAgent);
    Assert.Equal("192.168.1.1", deviceInfo.IpAddress);
    Assert.Equal("Windows - Chrome", deviceInfo.DeviceName);
    Assert.Equal("Windows", deviceInfo.Platform);
    Assert.Equal("Chrome", deviceInfo.Browser);
  }

  [Fact]
  public void DeviceInfo_WithNullValues_ShouldHandleGracefully()
  {
    // Arrange & Act
    var deviceInfo = new DeviceInfo(
        UserAgent: null,
        IpAddress: null,
        DeviceName: null,
        Platform: null,
        Browser: null
    );

    // Assert
    Assert.Null(deviceInfo.UserAgent);
    Assert.Null(deviceInfo.IpAddress);
    Assert.Null(deviceInfo.DeviceName);
    Assert.Null(deviceInfo.Platform);
    Assert.Null(deviceInfo.Browser);
  }

  #endregion

  #region Edge Cases and Integration Tests

  [Fact]
  public async Task GenerateTokensAsync_WithUserWithMultipleRoles_ShouldIncludeAllRoles()
  {
    // Arrange
    var user = new IdentityUser
    {
      Id = "user-multi-role",
      Email = "multirole@example.com",
      UserName = "multiroleuser"
    };

    var appUser = AppUser.Create();
    appUser.SetIdentityId(user.Id);

    // Create new role instances instead of using static references
    var adminRole = Role.Create("Admin", "Admin Role", appUser.Id);
    var userRole = Role.Create("Registered", "Registered User", appUser.Id);

    // Add roles to context first
    _context.Roles.Add(adminRole);
    _context.Roles.Add(userRole);
    await _context.SaveChangesAsync();

    // Now add roles to user
    appUser.AddRole(adminRole);
    appUser.AddRole(userRole);

    _context.AppUsers.Add(appUser);
    await _context.SaveChangesAsync();

    // Clear change tracker to avoid tracking issues
    _context.ChangeTracker.Clear();

    // Re-fetch the user with includes to ensure proper tracking
    var dbUser = await _context.AppUsers
        .Include(u => u.Roles)
        .FirstAsync(u => u.Id == appUser.Id);

    // Act
    var result = await _jwtTokenService.GenerateTokensAsync(user, dbUser, null);

    // Assert
    Assert.NotNull(result);
    var handler = new JwtSecurityTokenHandler();
    var jsonToken = handler.ReadJwtToken(result.AccessToken);
    var roleClaims = jsonToken.Claims.Where(c => c.Type == "roles").Select(c => c.Value).ToList();

    Assert.Contains("Admin", roleClaims);
    Assert.Contains("Registered", roleClaims);
  }

  [Fact]
  public async Task TokenLifecycle_ShouldWorkEndToEnd()
  {
    // Arrange
    var user = new IdentityUser
    {
      Id = "lifecycle-user",
      Email = "lifecycle@example.com",
      UserName = "lifecycleuser"
    };

    var appUser = AppUser.Create();
    appUser.SetIdentityId(user.Id);

    // Create new role instance instead of using static reference
    var userRole = Role.Create("Registered", "Registered User", appUser.Id);

    // Add role to context first
    _context.Roles.Add(userRole);
    await _context.SaveChangesAsync();

    // Now add role to user
    appUser.AddRole(userRole);

    _context.AppUsers.Add(appUser);
    await _context.SaveChangesAsync();

    // Clear change tracker to avoid tracking issues
    _context.ChangeTracker.Clear();

    _mockUserManager.Setup(um => um.FindByIdAsync(user.Id))
        .ReturnsAsync(user);

    // Act 1: Generate tokens
    // Re-fetch the user with includes to ensure proper tracking
    var dbUser = await _context.AppUsers
        .Include(u => u.Roles)
        .FirstAsync(u => u.Id == appUser.Id);

    var initialTokens = await _jwtTokenService.GenerateTokensAsync(user, dbUser, null);
    Assert.NotNull(initialTokens);

    // Clear change tracker again
    _context.ChangeTracker.Clear();

    // Act 2: Validate access token
    var principal = _jwtTokenService.ValidateToken(initialTokens.AccessToken);
    Assert.NotNull(principal);

    // Act 3: Refresh tokens
    var refreshedTokens = await _jwtTokenService.RefreshTokensAsync(initialTokens.RefreshToken, null);
    Assert.NotNull(refreshedTokens);
    Assert.NotEqual(initialTokens.AccessToken, refreshedTokens.AccessToken);

    // Clear change tracker again
    _context.ChangeTracker.Clear();

    // Act 4: Revoke refresh token
    await _jwtTokenService.RevokeRefreshTokenAsync(refreshedTokens.RefreshToken);

    // Assert: Try to use revoked token
    await Assert.ThrowsAsync<SecurityTokenValidationException>(
        () => _jwtTokenService.RefreshTokensAsync(refreshedTokens.RefreshToken, null));
  }

  #endregion

  public void Dispose()
  {
    _context?.Dispose();
  }
}