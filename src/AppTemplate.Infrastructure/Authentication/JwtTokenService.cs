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

    public async Task<JwtTokenResult> GenerateTokensAsync(IdentityUser user, AppUser appUser)
    {
        var accessToken = await GenerateAccessTokenAsync(user, appUser);
        var refreshToken = GenerateRefreshToken();
        
        // Store refresh token in database
        var refreshTokenEntity = new RefreshToken
        {
            Token = refreshToken,
            UserId = user.Id,
            ExpiresAt = DateTime.UtcNow.AddDays(7)
        };
        
        _context.RefreshTokens.Add(refreshTokenEntity);
        await _context.SaveChangesAsync();
        
        return new JwtTokenResult(
            accessToken.Token,
            refreshToken,
            accessToken.ExpiresAt);
    }

    public async Task<JwtTokenResult> RefreshTokensAsync(string refreshToken)
    {
        var storedToken = await _context.RefreshTokens
            .FirstOrDefaultAsync(rt => rt.Token == refreshToken && !rt.IsRevoked);

        if (storedToken == null || storedToken.ExpiresAt < DateTime.UtcNow)
        {
            throw new SecurityTokenValidationException("Invalid or expired refresh token");
        }

        var user = await _userManager.FindByIdAsync(storedToken.UserId);
        if (user == null)
        {
            throw new SecurityTokenValidationException("User not found");
        }

        var appUser = await _context.AppUsers.FirstOrDefaultAsync(au => au.IdentityId == user.Id);
        if (appUser == null)
        {
            throw new SecurityTokenValidationException("App user not found");
        }

        // Revoke the old refresh token
        storedToken.IsRevoked = true;
        storedToken.RevokedReason = "Replaced by new token";

        var newTokens = await GenerateTokensAsync(user, appUser);
        storedToken.ReplacedByToken = newTokens.RefreshToken;
        
        await _context.SaveChangesAsync();
        
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
            await _context.SaveChangesAsync();
        }
    }

    public async Task RevokeAllUserRefreshTokensAsync(string userId)
    {
        var userTokens = _context.RefreshTokens.Where(rt => rt.UserId == userId && !rt.IsRevoked);
        
        await foreach (var token in userTokens.AsAsyncEnumerable())
        {
            token.IsRevoked = true;
            token.RevokedReason = "All tokens revoked";
        }
        
        await _context.SaveChangesAsync();
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
}
