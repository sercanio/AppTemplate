namespace AppTemplate.Application.Authentication.Models;

public class RefreshToken
{
    public string Token { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public bool IsRevoked { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string? RevokedReason { get; set; }
    public string? ReplacedByToken { get; set; }
}