using AppTemplate.Application.Data.Pagination;
using AppTemplate.Application.Repositories;
using AppTemplate.Application.Services.AppUsers;
using AppTemplate.Domain.AppUsers;
using Ardalis.Result;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace AppTemplate.Application.Features.Notifications.Queries.GetAllNotifications;

public sealed class GetAllNotificationsQueryHandler
    : IRequestHandler<GetAllNotificationsQuery, Result<GetAllNotificationsWithUnreadCountResponse>>
{
  private readonly INotificationsRepository _notificationsRepository;
  private readonly IHttpContextAccessor _httpContextAccessor;
  private readonly IAppUsersService _usersService;

  public GetAllNotificationsQueryHandler(
      INotificationsRepository notificationsRepository,
      IHttpContextAccessor httpContextAccessor,
      IAppUsersService usersService)
  {
    _notificationsRepository = notificationsRepository;
    _httpContextAccessor = httpContextAccessor;
    _usersService = usersService;
  }

  public async Task<Result<GetAllNotificationsWithUnreadCountResponse>> Handle(
      GetAllNotificationsQuery request,
      CancellationToken cancellationToken)
  {
    var identityId = _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
    var user = await _usersService.GetAsync(
        predicate: user => user.IdentityId == identityId,
        include: query => query.Include(u => u.Notifications),
        cancellationToken: cancellationToken);

    if (user is null)
    {
      return Result.NotFound(AppUserErrors.NotFound.Name);
    }

    int unreadCount = user.Notifications.Count(notification => !notification.IsRead);

    List<GetAllNotificationsQueryResponse> paginatedNotifications = user.Notifications
       .OrderByDescending(notification => notification.CreatedOnUtc)
       .Skip(request.PageIndex * request.PageSize)
       .Take(request.PageSize)
       .Select(notification => new GetAllNotificationsQueryResponse(
           notification.Id,
           notification.RecipientId,
           notification.Title,
           notification.Message,
           notification.Type,
           notification.CreatedOnUtc,
           notification.IsRead
       ))
       .ToList();

    PaginatedList<GetAllNotificationsQueryResponse> paginatedList = new(
        paginatedNotifications,
        user.Notifications.Count,
        request.PageIndex,
        request.PageSize);

    var response = new GetAllNotificationsWithUnreadCountResponse(paginatedList, unreadCount);

    return Result.Success(response);
  }
}