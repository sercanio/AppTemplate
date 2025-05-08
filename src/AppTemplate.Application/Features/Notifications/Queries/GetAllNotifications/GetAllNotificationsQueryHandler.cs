using System.Security.Claims;
using Ardalis.Result;
using MediatR;
using Microsoft.AspNetCore.Http;
using Myrtus.Clarity.Core.Application.Abstractions.Pagination;
using Myrtus.Clarity.Core.Infrastructure.Pagination;
using AppTemplate.Application.Repositories;
using AppTemplate.Application.Services.AppUsers;
using AppTemplate.Domain.AppUsers;

namespace AppTemplate.Application.Features.Notifications.Queries.GetAllNotifications;

public sealed class GetAllNotificationsQueryHandler
    : IRequestHandler<GetAllNotificationsQuery, Result<IPaginatedList<GetAllNotificationsQueryResponse>>>
{
    private readonly INotificationsRepository _notificationsRepository;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IAppUsersService _usersService;

    public GetAllNotificationsQueryHandler(INotificationsRepository notificationsRepository, IHttpContextAccessor httpContextAccessor, IAppUsersService usersService)
    {
        _notificationsRepository = notificationsRepository;
        _httpContextAccessor = httpContextAccessor;
        _usersService = usersService;
    }

    public async Task<Result<IPaginatedList<GetAllNotificationsQueryResponse>>> Handle(GetAllNotificationsQuery request, CancellationToken cancellationToken)
    {
        var identityId = _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
        var user = await _usersService.GetAsync(
            predicate: user => user.IdentityId == identityId,
            include: user => user.Notifications,
            cancellationToken: cancellationToken);

        if (user is null)
        {
            return Result.NotFound(AppUserErrors.NotFound.Name);
        }

        int unreadCount = user.Notifications.Count(notification => !notification.IsRead);

        List<GetAllNotificationsQueryResponse> paginatedNotifications = user.Notifications
           .OrderByDescending(notification => notification.Timestamp)
           .Skip(request.PageIndex * request.PageSize)
           .Take(request.PageSize)
           .Select(notification => new GetAllNotificationsQueryResponse(
               notification.Id,
               notification.UserId.ToString(),
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
        user.Notifications.Count,
        request.PageIndex,
        request.PageSize);

        return Result.Success<IPaginatedList<GetAllNotificationsQueryResponse>>(paginatedList);
    }
}
