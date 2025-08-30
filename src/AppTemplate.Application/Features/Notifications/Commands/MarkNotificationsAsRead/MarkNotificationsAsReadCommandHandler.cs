using AppTemplate.Application.Repositories;
using AppTemplate.Core.Application.Abstractions.Messaging;
using Ardalis.Result;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace AppTemplate.Application.Features.Notifications.Commands.MarkNotificationsAsRead;

public sealed class MarkNotificationAsReadCommandHandler : ICommandHandler<MarkNotificationAsReadCommand, MarkNotificationAsReadCommandResponse>
{
    private readonly INotificationsRepository _notificationsRepository;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IAppUsersRepository _appUsersRepository;

    public MarkNotificationAsReadCommandHandler(
        INotificationsRepository notificationsRepository,
        IHttpContextAccessor httpContextAccessor,
        IAppUsersRepository appUsersRepository)
    {
        _notificationsRepository = notificationsRepository;
        _httpContextAccessor = httpContextAccessor;
        _appUsersRepository = appUsersRepository;
    }

    public async Task<Result<MarkNotificationAsReadCommandResponse>> Handle(
        MarkNotificationAsReadCommand request,
        CancellationToken cancellationToken)
    {
        var identityIdString = _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(identityIdString))
        {
            return Result.NotFound("IdentityId is null or empty string.");
        }

        var userResult = await _appUsersRepository.GetUserByIdentityIdWithIdentityAndRolesAsync(identityIdString, cancellationToken);
        if (!userResult.IsSuccess || userResult.Value is null)
        {
            return Result.NotFound("User not found.");
        }

        var success = await _notificationsRepository.MarkAsReadAsync(request.NotificationId, cancellationToken);

        var response = new MarkNotificationAsReadCommandResponse(success);
        return Result.Success(response);
    }
}
