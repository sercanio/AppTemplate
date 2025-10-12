namespace AppTemplate.Application.Services.Authentication.Models;

public record DeviceInfo(
    string? UserAgent,
    string? IpAddress,
    string? DeviceName = null,
    string? Platform = null,
    string? Browser = null);