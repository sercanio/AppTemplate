using Ardalis.Result;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Myrtus.Clarity.Core.Application.Abstractions.Pagination;
using Myrtus.Clarity.Core.Infrastructure.Authorization;
using Myrtus.Clarity.Core.WebAPI;
using Myrtus.Clarity.Core.WebAPI.Controllers;
using EcoFind.Application.Features.Notifications.Commands.MarkNotificationsAsRead;
using EcoFind.Application.Features.Notifications.Queries.GetAllNotifications;

namespace EcoFind.Web.Controllers;

[ApiController]
[Route("api/v1.0/notifications")]
[EnableRateLimiting("Fixed")]
public class NotificationsController(
    ISender sender,
    IErrorHandlingService errorHandlingService) : BaseController(sender, errorHandlingService)
{
    [HttpGet]
    // [HasPermission(Permissions.NotificationsRead)]
    public async Task<IActionResult> GetAllNotifications(
        [FromQuery] int pageIndex = 0,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        GetAllNotificationsQuery query = new(pageIndex, pageSize, cancellationToken);
        Result<GetAllNotificationsWithUnreadCountResponse> result = await _sender.Send(query, cancellationToken);

        return !result.IsSuccess ? _errorHandlingService.HandleErrorResponse(result) : Ok(result.Value);
    }

    [HttpPatch("read")]
    // [HasPermission(Permissions.NotificationsUpdate)]
    public async Task<IActionResult> MarkNotificationsAsRead(
        CancellationToken cancellationToken = default)
    {
        MarkNotificationsAsReadCommand command = new(cancellationToken);

        Result<MarkNotificationsAsReadCommandResponse> result = await _sender.Send(command, cancellationToken);

        return !result.IsSuccess ? _errorHandlingService.HandleErrorResponse(result) : Ok();
    }
}
