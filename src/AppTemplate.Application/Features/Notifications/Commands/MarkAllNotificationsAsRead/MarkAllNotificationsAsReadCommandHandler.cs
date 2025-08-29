using AppTemplate.Application.Services.AppUsers;
using AppTemplate.Application.Services.Notifications;
using AppTemplate.Core.Application.Abstractions.Messaging;
using Ardalis.Result;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace AppTemplate.Application.Features.Notifications.Commands.MarkAllNotificationsAsRead;

public sealed class MarkAllNotificationsAsReadCommandHandler : ICommandHandler<MarkAllNotificationsAsReadCommand, MarkAllNotificationsAsReadCommandResponse>
{
    private readonly INotificationService _notificationService;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IAppUsersService _appUsersService;

    public MarkAllNotificationsAsReadCommandHandler(
        INotificationService notificationService,
        IHttpContextAccessor httpContextAccessor,
        IAppUsersService appUsersService)
    {
        _notificationService = notificationService;
        _httpContextAccessor = httpContextAccessor;
        _appUsersService = appUsersService;
    }

    public async Task<Result<MarkAllNotificationsAsReadCommandResponse>> Handle(
        MarkAllNotificationsAsReadCommand request,
        CancellationToken cancellationToken)
    {
        var identityIdString = _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(identityIdString))
        {
            return Result.NotFound("IdentityId is null or empty string.");
        }

        var user = await _appUsersService.GetAsync(
            predicate: u => string.Equals(identityIdString, u.IdentityId),
            cancellationToken: cancellationToken);

        if (user is null)
        {
            return Result.NotFound("User not found.");
        }

        await _notificationService.MarkNotificationsAsReadAsync(user.Id, cancellationToken);

        MarkAllNotificationsAsReadCommandResponse response = new(true);

        return Result.Success(response);
    }
}
