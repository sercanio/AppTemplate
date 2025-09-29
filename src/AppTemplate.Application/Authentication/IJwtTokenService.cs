using AppTemplate.Application.Authentication.Models;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using AppTemplate.Domain.AppUsers;

namespace AppTemplate.Application.Authentication;

public interface IJwtTokenService
{
  Task<JwtTokenResult> GenerateTokensAsync(IdentityUser user, AppUser appUser, DeviceInfo? deviceInfo = null);
  Task<JwtTokenResult> RefreshTokensAsync(string refreshToken, DeviceInfo? deviceInfo = null);
  ClaimsPrincipal? ValidateToken(string token);
  Task RevokeRefreshTokenAsync(string refreshToken);
  Task RevokeAllUserRefreshTokensAsync(string userId);
  Task<IEnumerable<DeviceSessionDto>> GetUserDeviceSessionsAsync(string userId, string? currentAccessTokenJti = null);
  Task<bool> RevokeDeviceSessionAsync(string refreshToken, string userId);
}

public sealed record JwtTokenResult(
    string AccessToken,
    string RefreshToken,
    DateTime ExpiresAt,
    string TokenType = "Bearer");