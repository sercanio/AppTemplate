using AppTemplate.Application.Features.Statistics.Authentication.Queries.GetAuthenticationStatistics;
using AppTemplate.Application.Features.Statistics.Roles.Queries.GetRoleStatistics;
using AppTemplate.Application.Features.Statistics.Users.Queries.GetUserRegistrationTrends;
using AppTemplate.Application.Features.Statistics.Users.Queries.GetUsersCount;
using AppTemplate.Core.Infrastructure.Authorization;
using AppTemplate.Core.WebApi;
using AppTemplate.Web.Attributes;
using Ardalis.Result;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

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
    [HasPermission(Permissions.StatisticsRead)]
    public async Task<IActionResult> GetUsersCount(CancellationToken cancellationToken = default)
    {
        var query = new GetUsersCountQuery();
        var result = await _sender.Send(query, cancellationToken);

        return result.IsSuccess
            ? Ok(result.Value)
            : _errorHandlingService.HandleErrorResponse(result);
    }

    [HttpGet("users/trends")]
    [HasPermission(Permissions.StatisticsRead)]
    public async Task<IActionResult> GetUserRegistrationTrends(CancellationToken cancellationToken = default)
    {
        var query = new GetUserRegistrationTrendsQuery();
        var result = await _sender.Send(query, cancellationToken);

        return result.IsSuccess
            ? Ok(result.Value)
            : _errorHandlingService.HandleErrorResponse(result);
    }

    [HttpGet("authentication")]
    [HasPermission(Permissions.StatisticsRead)]
    public async Task<IActionResult> GetAuthenticationStatistics(CancellationToken cancellationToken = default)
    {
        var query = new GetAuthenticationStatisticsQuery();
        var result = await _sender.Send(query, cancellationToken);

        return result.IsSuccess
            ? Ok(result.Value)
            : _errorHandlingService.HandleErrorResponse(result);
    }

    [HttpGet("roles")]
    [HasPermission(Permissions.StatisticsRead)]
    public async Task<IActionResult> GetRoleStatistics(CancellationToken cancellationToken = default)
    {
        var query = new GetRoleStatisticsQuery();
        var result = await _sender.Send(query, cancellationToken);

        return result.IsSuccess
            ? Ok(result.Value)
            : _errorHandlingService.HandleErrorResponse(result);
    }

    [HttpGet("notifications")]
    [HasPermission(Permissions.StatisticsRead)]
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

    [HttpGet("system")]
    [HasPermission(Permissions.StatisticsRead)]
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

    public record GetNotificationStatisticsQuery();
    public record NotificationStatisticsResponse(
        int TotalNotifications,
        int UnreadNotifications,
        int NotificationsToday,
        Dictionary<string, int> NotificationsByType,
        Dictionary<string, int> NotificationsByChannel);

    public record GetSystemStatisticsQuery();
    public record SystemStatisticsResponse(
        int TotalApiRequests,
        int RateLimitedRequests,
        double AverageResponseTime,
        Dictionary<string, int> RequestsByEndpoint,
        int TotalErrors);
}

