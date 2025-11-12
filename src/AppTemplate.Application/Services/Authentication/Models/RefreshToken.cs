namespace AppTemplate.Application.Services.Authentication.Models;

public class RefreshToken
{
  public string Token { get; set; } = string.Empty;
  public string UserId { get; set; } = string.Empty;
  public DateTime ExpiresAt { get; set; }
  public DateTime CreatedAt { get; set; }
  public DateTime LastUsedAt { get; set; }
  public bool IsRevoked { get; set; }
  public string? RevokedReason { get; set; }
  public string? ReplacedByToken { get; set; }
  public bool IsCurrent { get; set; }

  // New device information fields
  public string? DeviceName { get; set; }
  public string? UserAgent { get; set; }
  public string? IpAddress { get; set; }
  public string? Platform { get; set; }
  public string? Browser { get; set; }
  public string? AccessTokenJti { get; set; }
}