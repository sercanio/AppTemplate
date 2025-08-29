using AppTemplate.Application.Features.Notifications.Commands.MarkAllNotificationsAsRead;
using AppTemplate.Application.Features.Notifications.Commands.MarkNotificationsAsRead;
using AppTemplate.Application.Features.Notifications.Queries.GetAllNotifications;
using AppTemplate.Core.Infrastructure.Authorization;
using AppTemplate.Core.WebApi;
using AppTemplate.Web.Attributes;
using AppTemplate.Web.Controllers.Api;
using Ardalis.Result;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace AppTemplate.Web.Controllers;

[ApiController]
[ApiVersion(ApiVersions.V1)]
[Route("api/v{version:apiVersion}/[controller]")]
[EnableRateLimiting("Fixed")]
public class NotificationsController(
    ISender sender,
    IErrorHandlingService errorHandlingService) : BaseController(sender, errorHandlingService)
{
  [HttpGet]
  public async Task<IActionResult> GetAllNotifications(
      [FromQuery] int pageIndex = 0,
      [FromQuery] int pageSize = 10,
      CancellationToken cancellationToken = default)
  {
    GetAllNotificationsQuery query = new(pageIndex, pageSize, cancellationToken);
    var result = await _sender.Send(query, cancellationToken);

    if (!result.IsSuccess)
    {
      return _errorHandlingService.HandleErrorResponse(result);
    }

    return Ok(result.Value);
  }

  [HttpPatch("read")]
  [HasPermission(Permissions.NotificationsRead)]
  public async Task<IActionResult> MarkNotificationsAsRead(
      CancellationToken cancellationToken = default)
  {
    MarkAllNotificationsAsReadCommand command = new(cancellationToken);

    Result<MarkAllNotificationsAsReadCommandResponse> result = await _sender.Send(command, cancellationToken);

    return !result.IsSuccess ? _errorHandlingService.HandleErrorResponse(result) : Ok();
  }

  [HttpPatch("{notificationId:guid}/read")]
  [HasPermission(Permissions.NotificationsUpdate)]
  public async Task<IActionResult> MarkNotificationAsRead(
      [FromRoute] Guid notificationId,
      CancellationToken cancellationToken = default)
  {
    var command = new MarkNotificationAsReadCommand(notificationId);
    var result = await _sender.Send(command, cancellationToken);

    return !result.IsSuccess
        ? _errorHandlingService.HandleErrorResponse(result)
        : Ok(result.Value);
  }
}
