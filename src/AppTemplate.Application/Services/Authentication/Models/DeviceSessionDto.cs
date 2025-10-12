namespace AppTemplate.Application.Services.Authentication.Models;

public record DeviceSessionDto(
    string Token,
    string DeviceName,
    string Platform,
    string Browser,
    string IpAddress,
    DateTime LastUsedAt,
    DateTime CreatedAt,
    bool IsCurrent);
