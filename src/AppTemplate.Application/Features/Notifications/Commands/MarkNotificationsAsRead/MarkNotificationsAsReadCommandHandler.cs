using System.Security.Claims;
using AppTemplate.Application.Services.AppUsers;
using AppTemplate.Application.Services.Notifications;
using Ardalis.Result;
using Microsoft.AspNetCore.Http;
using Myrtus.Clarity.Core.Application.Abstractions.Messaging;

namespace AppTemplate.Application.Features.Notifications.Commands.MarkNotificationsAsRead;

public sealed class MarkNotificationAsReadCommandHandler : ICommandHandler<MarkNotificationAsReadCommand, MarkNotificationAsReadCommandResponse>
{
  private readonly INotificationService _notificationService;
  private readonly IHttpContextAccessor _httpContextAccessor;
  private readonly IAppUsersService _appUsersService;

  public MarkNotificationAsReadCommandHandler(
      INotificationService notificationService,
      IHttpContextAccessor httpContextAccessor,
      IAppUsersService appUsersService)
  {
    _notificationService = notificationService;
    _httpContextAccessor = httpContextAccessor;
    _appUsersService = appUsersService;
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

    var user = await _appUsersService.GetAsync(
        predicate: u => string.Equals(identityIdString, u.IdentityId),
        cancellationToken: cancellationToken);

    if (user is null)
    {
      return Result.NotFound("User not found.");
    }

    var success = await _notificationService.MarkNotificationAsReadAsync(request.NotificationId, cancellationToken);

    var response = new MarkNotificationAsReadCommandResponse(success);
    return Result.Success(response);
  }
}
