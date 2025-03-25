using MediatR;
using Ardalis.Result;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Myrtus.Clarity.Core.Application.Abstractions.Authentication;
using Myrtus.Clarity.Core.Application.Abstractions.Notification;
using Myrtus.Clarity.Core.Application.Abstractions.Pagination;
using Myrtus.Clarity.Core.Domain.Abstractions;
using Myrtus.Clarity.Core.Infrastructure.Pagination;
using AppTemplate.Domain.AppUsers;
using AppTemplate.Application.Services.AppUsers;

namespace AppTemplate.Application.Features.Notifications.Queries.GetAllNotifications;

public sealed class GetAllNotificationsQueryHandler : IRequestHandler<GetAllNotificationsQuery, Result<GetAllNotificationsWithUnreadCountResponse>>
{
    private readonly INotificationService _notificationService;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IAppUsersService _usersService;

    public GetAllNotificationsQueryHandler(INotificationService notificationService, IHttpContextAccessor httpContextAccessor, IAppUsersService usersService)
    {
        _notificationService = notificationService;
        _httpContextAccessor = httpContextAccessor;
        _usersService = usersService;
    }

    public async Task<Result<GetAllNotificationsWithUnreadCountResponse>> Handle(GetAllNotificationsQuery request, CancellationToken cancellationToken)
    {
        var identityId = _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
        var user = await _usersService.GetAsync(
            predicate: user => user.IdentityId == identityId,
            cancellationToken: cancellationToken);

        if (user is null)
        {
            return Result.NotFound(AppUserErrors.NotFound.Name);
        }

        List<Notification> notifications = await _notificationService.GetNotificationsByUserIdAsync(user.IdentityId.ToString());

        int unreadCount = notifications.Count(notification => !notification.IsRead);

        List<GetAllNotificationsQueryResponse> paginatedNotifications = notifications
            .OrderByDescending(notification => notification.Timestamp)
            .Skip(request.PageIndex * request.PageSize)
            .Take(request.PageSize)
            .Select(notification => new GetAllNotificationsQueryResponse(
                notification.Id,
                notification.UserId,
                notification.User,
                notification.Action,
                notification.Entity,
                notification.EntityId,
                notification.Timestamp,
                notification.Details,
                notification.IsRead
            ))
            .ToList();

        PaginatedList<GetAllNotificationsQueryResponse> paginatedList = new(
            paginatedNotifications,
            notifications.Count,
            request.PageIndex,
            request.PageSize
        );

        var response = new GetAllNotificationsWithUnreadCountResponse(paginatedList, unreadCount);

        return Result.Success(response);
    }
}
