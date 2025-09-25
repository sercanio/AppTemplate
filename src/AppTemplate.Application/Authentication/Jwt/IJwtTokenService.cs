using AppTemplate.Domain.AppUsers;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace AppTemplate.Application.Authentication.Jwt;

public interface IJwtTokenService
{
    Task<JwtTokenResult> GenerateTokensAsync(IdentityUser user, AppUser appUser);
    Task<JwtTokenResult> RefreshTokensAsync(string refreshToken);
    ClaimsPrincipal? ValidateToken(string token);
    Task RevokeRefreshTokenAsync(string refreshToken);
    Task RevokeAllUserRefreshTokensAsync(string userId);
}

public sealed record JwtTokenResult(
    string AccessToken,
    string RefreshToken,
    DateTime ExpiresAt,
    string TokenType = "Bearer");