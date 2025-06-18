using AppTemplate.Application.Features.Statistics.Users.Queries.GetUserRegistrationTrends;
using AppTemplate.Application.Features.Statistics.Users.Queries.GetUsersCount;
using Ardalis.Result;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Myrtus.Clarity.Core.WebAPI;
using Myrtus.Clarity.Core.WebAPI.Controllers;

namespace AppTemplate.Web.Controllers.Api;

[ApiController]
[ApiVersion(ApiVersions.V1)]
[Route("api/v{version:apiVersion}/[controller]")]
[EnableRateLimiting("Fixed")]
public class StatisticsController : BaseController
{
    public StatisticsController(ISender sender, IErrorHandlingService errorHandlingService)
        : base(sender, errorHandlingService)
    {
    }

    [HttpGet("users/count")]
    public async Task<IActionResult> GetUsersCount(CancellationToken cancellationToken = default)
    {
        var query = new GetUsersCountQuery();
        var result = await _sender.Send(query, cancellationToken);

        return result.IsSuccess
            ? Ok(result.Value)
            : _errorHandlingService.HandleErrorResponse(result);
    }

    [HttpGet("users/trends")]
    public async Task<IActionResult> GetUserRegistrationTrends(CancellationToken cancellationToken = default)
    {
        var query = new GetUserRegistrationTrendsQuery();
        var result = await _sender.Send(query, cancellationToken);

        return result.IsSuccess
            ? Ok(result.Value)
            : _errorHandlingService.HandleErrorResponse(result);
    }

    [HttpGet("roles")]
    public async Task<IActionResult> GetRoleStatistics(CancellationToken cancellationToken = default)
    {
        var query = new GetRoleStatisticsQuery();
        var result = await _sender.Send(query, cancellationToken) as Result<RoleStatisticsResponse>;

        if (result == null)
        {
            return _errorHandlingService.HandleErrorResponse(Result<RoleStatisticsResponse>.Error("Failed to retrieve role statistics."));
        }

        return result.IsSuccess ? Ok(result.Value) : _errorHandlingService.HandleErrorResponse(result);
    }

    [HttpGet("notifications")]
    public async Task<IActionResult> GetNotificationStatistics(CancellationToken cancellationToken = default)
    {
        var query = new GetNotificationStatisticsQuery();
        var result = await _sender.Send(query, cancellationToken) as Result<NotificationStatisticsResponse>;

        if (result == null)
        {
            return _errorHandlingService.HandleErrorResponse(Result<NotificationStatisticsResponse>.Error("Failed to retrieve notification statistics."));
        }

        return result.IsSuccess ? Ok(result.Value) : _errorHandlingService.HandleErrorResponse(result);
    }

    [HttpGet("authentication")]
    public async Task<IActionResult> GetAuthenticationStatistics(CancellationToken cancellationToken = default)
    {
        var query = new GetAuthenticationStatisticsQuery();
        var result = await _sender.Send(query, cancellationToken) as Result<AuthenticationStatisticsResponse>;

        if (result == null)
        {
            return _errorHandlingService.HandleErrorResponse(Result<AuthenticationStatisticsResponse>.Error("Failed to retrieve authentication statistics."));
        }

        return result.IsSuccess ? Ok(result.Value) : _errorHandlingService.HandleErrorResponse(result);
    }

    [HttpGet("system")]
    public async Task<IActionResult> GetSystemStatistics(CancellationToken cancellationToken = default)
    {
        var query = new GetSystemStatisticsQuery();
        var result = await _sender.Send(query, cancellationToken) as Result<SystemStatisticsResponse>;

        if (result == null)
        {
            return _errorHandlingService.HandleErrorResponse(Result<SystemStatisticsResponse>.Error("Failed to retrieve system statistics."));
        }

        return result.IsSuccess ? Ok(result.Value) : _errorHandlingService.HandleErrorResponse(result);

    }

    // Request/Response DTOs
    public record GetUserStatisticsQuery();
    public record UserStatisticsResponse(
        int TotalUsers,
        int ActiveUsers,
        int NewUsersToday,
        int NewUsersThisWeek,
        int NewUsersThisMonth,
        Dictionary<string, int> UsersByRole);

    public record GetRoleStatisticsQuery();
    public record RoleStatisticsResponse(
        int TotalRoles,
        int TotalPermissions,
        Dictionary<string, int> PermissionsPerRole,
        Dictionary<string, int> UsersPerRole);

    public record GetNotificationStatisticsQuery();
    public record NotificationStatisticsResponse(
        int TotalNotifications,
        int UnreadNotifications,
        int NotificationsToday,
        Dictionary<string, int> NotificationsByType,
        Dictionary<string, int> NotificationsByChannel);

    public record GetAuthenticationStatisticsQuery();
    public record AuthenticationStatisticsResponse(
        int SuccessfulLogins,
        int FailedLogins,
        int TwoFactorLogins,
        int PasswordResets,
        int AccountLockouts);

    public record GetSystemStatisticsQuery();
    public record SystemStatisticsResponse(
        int TotalApiRequests,
        int RateLimitedRequests,
        double AverageResponseTime,
        Dictionary<string, int> RequestsByEndpoint,
        int TotalErrors);
}

