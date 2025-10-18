using AppTemplate.Application.Features.Statistics.Authentication.Queries.GetAuthenticationStatistics;
using AppTemplate.Application.Features.Statistics.Roles.Queries.GetRoleStatistics;
using AppTemplate.Application.Features.Statistics.Users.Queries.GetUserRegistrationTrends;
using AppTemplate.Application.Features.Statistics.Users.Queries.GetUsersCount;
using AppTemplate.Application.Services.Authorization;
using AppTemplate.Application.Services.ErrorHandling;
using AppTemplate.Presentation.Attributes;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace AppTemplate.Presentation.Controllers.Api.v1;

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
}

