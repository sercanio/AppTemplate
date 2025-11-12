using System.Security.Claims;
using AppTemplate.Application.Services.Authentication.Models;
using AppTemplate.Domain.AppUsers;
using Microsoft.AspNetCore.Identity;

namespace AppTemplate.Application.Services.Authentication;

public interface IJwtTokenService
{
  Task<JwtTokenResult> GenerateTokensAsync(IdentityUser user, AppUser appUser, DeviceInfo? deviceInfo = null);
  Task<JwtTokenResult> RefreshTokensAsync(string refreshToken, DeviceInfo? deviceInfo = null);
  ClaimsPrincipal? ValidateToken(string token);
  Task RevokeRefreshTokenAsync(string refreshToken);
  Task RevokeAllUserRefreshTokensAsync(string userId);
  Task RevokeOtherUserRefreshTokensAsync(string userId, string currentAccessTokenJti);
  Task<IEnumerable<DeviceSessionDto>> GetUserDeviceSessionsAsync(string userId, string? currentAccessTokenJti = null);
  Task<bool> RevokeDeviceSessionAsync(string refreshToken, string userId);
}

public sealed record JwtTokenResult(
    string AccessToken,
    string RefreshToken,
    DateTime ExpiresAt,
    string TokenType = "Bearer");