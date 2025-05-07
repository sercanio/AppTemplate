using System.Security.Claims;
using AppTemplate.Application.Services.AppUsers;
using AppTemplate.Application.Services.Notifications;
using Ardalis.Result;
using Microsoft.AspNetCore.Http;
using Myrtus.Clarity.Core.Application.Abstractions.Messaging;

namespace AppTemplate.Application.Features.Notifications.Commands.MarkNotificationsAsRead;

public sealed class MarkNotificationsAsReadCommandHandler : ICommandHandler<MarkNotificationsAsReadCommand, MarkNotificationsAsReadCommandResponse>
{
    private readonly INotificationService _notificationService;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IAppUsersService _appUsersService;

    public MarkNotificationsAsReadCommandHandler(
        INotificationService notificationService,
        IHttpContextAccessor httpContextAccessor,
        IAppUsersService appUsersService)
    {
        _notificationService = notificationService;
        _httpContextAccessor = httpContextAccessor;
        _appUsersService = appUsersService;
    }

    public async Task<Result<MarkNotificationsAsReadCommandResponse>> Handle(
        MarkNotificationsAsReadCommand request,
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

        MarkNotificationsAsReadCommandResponse response = new();

        return Result.Success(response);
    }
}
