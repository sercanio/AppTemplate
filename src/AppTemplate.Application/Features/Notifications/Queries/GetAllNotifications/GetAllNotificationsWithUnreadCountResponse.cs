using AppTemplate.Application.Data.Pagination;

namespace AppTemplate.Application.Features.Notifications.Queries.GetAllNotifications;

public sealed record GetAllNotificationsWithUnreadCountResponse(
    PaginatedList<GetAllNotificationsQueryResponse> PaginatedNotifications,
    int UnreadCount);