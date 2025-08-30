using AppTemplate.Application.Repositories;
using AppTemplate.Core.Application.Abstractions.Messaging;
using Ardalis.Result;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace AppTemplate.Application.Features.Notifications.Commands.MarkAllNotificationsAsRead;

public sealed class MarkAllNotificationsAsReadCommandHandler : ICommandHandler<MarkAllNotificationsAsReadCommand, MarkAllNotificationsAsReadCommandResponse>
{
    private readonly INotificationsRepository _notificationsRepository;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IAppUsersRepository _appUsersRepository;

    public MarkAllNotificationsAsReadCommandHandler(
        INotificationsRepository notificationsRepository,
        IHttpContextAccessor httpContextAccessor,
        IAppUsersRepository appUsersRepository)
    {
        _notificationsRepository = notificationsRepository;
        _httpContextAccessor = httpContextAccessor;
        _appUsersRepository = appUsersRepository;
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

        var userResult = await _appUsersRepository.GetUserByIdentityIdWithIdentityAndRolesAsync(identityIdString, cancellationToken);
        if (!userResult.IsSuccess || userResult.Value is null)
        {
            return Result.NotFound("User not found.");
        }

        await _notificationsRepository.MarkAllAsReadAsync(userResult.Value.Id, cancellationToken);

        var response = new MarkAllNotificationsAsReadCommandResponse(true);
        return Result.Success(response);
    }
}
