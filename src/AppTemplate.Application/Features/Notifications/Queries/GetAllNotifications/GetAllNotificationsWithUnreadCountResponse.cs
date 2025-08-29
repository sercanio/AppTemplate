using AppTemplate.Core.Infrastructure.Pagination;

namespace AppTemplate.Application.Features.Notifications.Queries.GetAllNotifications;

public sealed record GetAllNotificationsWithUnreadCountResponse(
    PaginatedList<GetAllNotificationsQueryResponse> PaginatedNotifications,
    int UnreadCount);