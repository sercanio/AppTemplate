using MediatR;
using Ardalis.Result;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Myrtus.Clarity.Core.Application.Abstractions.Authentication;
using Myrtus.Clarity.Core.Application.Abstractions.Messaging;
using Myrtus.Clarity.Core.Application.Abstractions.Notification;

namespace AppTemplate.Application.Features.Notifications.Commands.MarkNotificationsAsRead;

public sealed class MarkNotificationsAsReadCommandHandler : ICommandHandler<MarkNotificationsAsReadCommand, MarkNotificationsAsReadCommandResponse>
{
    private readonly INotificationService _notificationService;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public MarkNotificationsAsReadCommandHandler(
        INotificationService notificationService,
        IHttpContextAccessor httpContextAccessor)
    {
        _notificationService = notificationService;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<Result<MarkNotificationsAsReadCommandResponse>> Handle(
        MarkNotificationsAsReadCommand request,
        CancellationToken cancellationToken)
    {
        var userId = _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
        await _notificationService.MarkNotificationsAsReadAsync(userId, cancellationToken);

        MarkNotificationsAsReadCommandResponse response = new();

        return Result.Success(response);
    }
}
