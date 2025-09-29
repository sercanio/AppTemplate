using AppTemplate.Application.Authentication;
using AppTemplate.Application.Authentication.Models;
using AppTemplate.Application.Services.AppUsers;
using AppTemplate.Application.Services.Roles;
using AppTemplate.Domain.AppUsers;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace AppTemplate.Infrastructure.Authentication;

public sealed class JwtTokenService : IJwtTokenService
{
  private readonly UserManager<IdentityUser> _userManager;
  private readonly IConfiguration _configuration;
  private readonly ApplicationDbContext _context;
  private readonly ILogger<JwtTokenService> _logger;
  private readonly TokenValidationParameters _tokenValidationParameters;
  private readonly IAppUsersService _appUsersService;
  private readonly IRolesService _rolesService;

  public JwtTokenService(
      UserManager<IdentityUser> userManager,
      IConfiguration configuration,
      ApplicationDbContext context,
      ILogger<JwtTokenService> logger,
      IAppUsersService appUsersService,
      IRolesService rolesService
  )
  {
    _userManager = userManager;
    _configuration = configuration;
    _context = context;
    _logger = logger;
    _appUsersService = appUsersService;
    _rolesService = rolesService;

    _tokenValidationParameters = new TokenValidationParameters
    {
      ValidateIssuerSigningKey = true,
      IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(_configuration["Jwt:Secret"]!)),
      ValidateIssuer = true,
      ValidIssuer = _configuration["Jwt:Issuer"],
      ValidateAudience = true,
      ValidAudience = _configuration["Jwt:Audience"],
      ValidateLifetime = true,
      ClockSkew = TimeSpan.Zero,
      RequireExpirationTime = true
    };
  }

  // Method with device info and JTI extraction
  public async Task<JwtTokenResult> GenerateTokensAsync(IdentityUser user, AppUser appUser, DeviceInfo? deviceInfo = null)
  {
    var accessToken = await GenerateAccessTokenAsync(user, appUser);
    var refreshToken = GenerateRefreshToken();

    // Mark all previous tokens for this user as not current
    await MarkAllUserTokensAsNotCurrentAsync(user.Id);

    // Get refresh token expiry from configuration
    var refreshTokenExpiryDays = int.Parse(_configuration["Jwt:RefreshTokenExpiryInDays"] ?? "7");

    // Extract JTI from access token
    var handler = new JwtSecurityTokenHandler();
    var jsonToken = handler.ReadJwtToken(accessToken.Token);
    var jti = jsonToken.Claims.FirstOrDefault(x => x.Type == JwtRegisteredClaimNames.Jti)?.Value;

    // Store refresh token in database with device information
    var refreshTokenEntity = new RefreshToken
    {
      Token = refreshToken,
      UserId = user.Id,
      ExpiresAt = DateTime.UtcNow.AddDays(refreshTokenExpiryDays),
      CreatedAt = DateTime.UtcNow,
      LastUsedAt = DateTime.UtcNow,
      IsCurrent = true,
      AccessTokenJti = jti, // Store the JTI to match later
      DeviceName = deviceInfo?.DeviceName,
      UserAgent = deviceInfo?.UserAgent,
      IpAddress = deviceInfo?.IpAddress,
      Platform = deviceInfo?.Platform,
      Browser = deviceInfo?.Browser
    };

    _context.RefreshTokens.Add(refreshTokenEntity);
    await _context.SaveChangesAsync();

    _logger.LogInformation("Generated new tokens for user {UserId} from device {DeviceName} with {ExpiryDays} days expiry",
        user.Id, deviceInfo?.DeviceName ?? "Unknown", refreshTokenExpiryDays);

    return new JwtTokenResult(
        accessToken.Token,
        refreshToken,
        accessToken.ExpiresAt);
  }

  // Legacy method without device info - maintain backward compatibility
  public async Task<JwtTokenResult> RefreshTokensAsync(string refreshToken)
  {
    return await RefreshTokensAsync(refreshToken, null);
  }

  // New method with device info
  public async Task<JwtTokenResult> RefreshTokensAsync(string refreshToken, DeviceInfo? deviceInfo = null)
  {
    var storedToken = await _context.RefreshTokens
        .FirstOrDefaultAsync(rt => rt.Token == refreshToken && !rt.IsRevoked);

    if (storedToken == null || storedToken.ExpiresAt < DateTime.UtcNow)
    {
      _logger.LogWarning("Invalid or expired refresh token attempted: {Token}", refreshToken);
      throw new SecurityTokenValidationException("Invalid or expired refresh token");
    }

    var user = await _userManager.FindByIdAsync(storedToken.UserId);
    if (user == null)
    {
      _logger.LogWarning("User not found for refresh token: {UserId}", storedToken.UserId);
      throw new SecurityTokenValidationException("User not found");
    }

    var appUser = await _context.AppUsers.FirstOrDefaultAsync(au => au.IdentityId == user.Id);
    if (appUser == null)
    {
      _logger.LogWarning("App user not found for identity: {IdentityId}", user.Id);
      throw new SecurityTokenValidationException("App user not found");
    }

    // Update last used time and device info if provided
    storedToken.LastUsedAt = DateTime.UtcNow;
    if (deviceInfo?.IpAddress != null)
      storedToken.IpAddress = deviceInfo.IpAddress;

    // Revoke the old refresh token
    storedToken.IsRevoked = true;
    storedToken.RevokedReason = "Replaced by new token";

    var newTokens = await GenerateTokensAsync(user, appUser, deviceInfo);
    storedToken.ReplacedByToken = newTokens.RefreshToken;

    await _context.SaveChangesAsync();

    _logger.LogInformation("Refreshed tokens for user {UserId} from device {DeviceName}",
        user.Id, deviceInfo?.DeviceName ?? "Unknown");

    return newTokens;
  }

  public ClaimsPrincipal? ValidateToken(string token)
  {
    try
    {
      var tokenHandler = new JwtSecurityTokenHandler();
      var principal = tokenHandler.ValidateToken(token, _tokenValidationParameters, out var validatedToken);
      return principal;
    }
    catch (Exception ex)
    {
      _logger.LogWarning(ex, "Token validation failed");
      return null;
    }
  }

  public async Task RevokeRefreshTokenAsync(string refreshToken)
  {
    var storedToken = await _context.RefreshTokens
        .FirstOrDefaultAsync(rt => rt.Token == refreshToken);

    if (storedToken != null)
    {
      storedToken.IsRevoked = true;
      storedToken.RevokedReason = "Manually revoked";
      storedToken.IsCurrent = false;
      await _context.SaveChangesAsync();

      _logger.LogInformation("Revoked refresh token for user {UserId}", storedToken.UserId);
    }
  }

  public async Task RevokeAllUserRefreshTokensAsync(string userId)
  {
    var userTokens = _context.RefreshTokens.Where(rt => rt.UserId == userId && !rt.IsRevoked);

    await foreach (var token in userTokens.AsAsyncEnumerable())
    {
      token.IsRevoked = true;
      token.RevokedReason = "All tokens revoked";
      token.IsCurrent = false;
    }

    await _context.SaveChangesAsync();

    _logger.LogInformation("Revoked all refresh tokens for user {UserId}", userId);
  }

  public async Task<IEnumerable<DeviceSessionDto>> GetUserDeviceSessionsAsync(string userId, string? currentAccessTokenJti = null)
  {
    var refreshTokens = await _context.RefreshTokens
        .Where(rt => rt.UserId == userId && !rt.IsRevoked && rt.ExpiresAt > DateTime.UtcNow)
        .OrderByDescending(rt => rt.LastUsedAt)
        .ToListAsync();

    var deviceSessions = refreshTokens.Select(rt => new DeviceSessionDto(
        rt.Token,
        rt.DeviceName ?? "Unknown Device",
        rt.Platform ?? "Unknown",
        rt.Browser ?? "Unknown",
        rt.IpAddress ?? "Unknown",
        rt.LastUsedAt,
        rt.CreatedAt,
        // Set current based on matching JTI
        !string.IsNullOrEmpty(currentAccessTokenJti) && rt.AccessTokenJti == currentAccessTokenJti));

    _logger.LogDebug("Retrieved {Count} device sessions for user {UserId}", deviceSessions.Count(), userId);

    return deviceSessions;
  }

  public async Task<bool> RevokeDeviceSessionAsync(string refreshToken, string userId)
  {
    var token = await _context.RefreshTokens
        .FirstOrDefaultAsync(rt => rt.Token == refreshToken && rt.UserId == userId && !rt.IsRevoked);

    if (token == null)
    {
      _logger.LogWarning("Attempted to revoke non-existent or already revoked token for user {UserId}", userId);
      return false;
    }

    token.IsRevoked = true;
    token.RevokedReason = "Revoked by user";
    token.IsCurrent = false;

    await _context.SaveChangesAsync();

    _logger.LogInformation("Revoked device session {DeviceName} for user {UserId}",
        token.DeviceName ?? "Unknown Device", userId);

    return true;
  }

  private async Task<(string Token, DateTime ExpiresAt)> GenerateAccessTokenAsync(IdentityUser user, AppUser appUser)
  {
    var claims = new List<Claim>
          {
              new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
              new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
              new(JwtRegisteredClaimNames.Email, user.Email!),
              new(JwtRegisteredClaimNames.UniqueName, user.UserName!),
              new(ClaimTypes.NameIdentifier, user.Id),
              new("app_user_id", appUser.Id.ToString())
          };

    // Get domain roles for the user
    var appUserWithRoles = await _context.AppUsers
        .Include(u => u.Roles)
        .FirstOrDefaultAsync(u => u.IdentityId == user.Id);

    if (appUserWithRoles != null)
    {
      foreach (var role in appUserWithRoles.Roles)
      {
        // Add role claim (use domain role name)
        claims.Add(new Claim("roles", role.Name.Value));
      }
    }

    var expiresAt = DateTime.UtcNow.AddMinutes(int.Parse(_configuration["Jwt:ExpiryInMinutes"] ?? "15"));

    var tokenDescriptor = new SecurityTokenDescriptor
    {
      Subject = new ClaimsIdentity(claims),
      Expires = expiresAt,
      Issuer = _configuration["Jwt:Issuer"],
      Audience = _configuration["Jwt:Audience"],
      SigningCredentials = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.ASCII.GetBytes(_configuration["Jwt:Secret"]!)),
            SecurityAlgorithms.HmacSha256Signature)
    };

    var tokenHandler = new JwtSecurityTokenHandler();
    var token = tokenHandler.CreateToken(tokenDescriptor);
    return (tokenHandler.WriteToken(token), expiresAt);
  }

  private static string GenerateRefreshToken()
  {
    var randomNumber = new byte[64];
    using var rng = RandomNumberGenerator.Create();
    rng.GetBytes(randomNumber);
    return Convert.ToBase64String(randomNumber);
  }

  private async Task MarkAllUserTokensAsNotCurrentAsync(string userId)
  {
    var userTokens = _context.RefreshTokens.Where(rt => rt.UserId == userId && !rt.IsRevoked);

    await foreach (var token in userTokens.AsAsyncEnumerable())
    {
      token.IsCurrent = false;
    }
  }
}
