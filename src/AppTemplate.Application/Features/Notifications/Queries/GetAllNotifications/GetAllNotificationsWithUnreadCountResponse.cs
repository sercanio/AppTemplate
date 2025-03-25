using Myrtus.Clarity.Core.Application.Abstractions.Pagination;

namespace AppTemplate.Application.Features.Notifications.Queries.GetAllNotifications;

public sealed record GetAllNotificationsWithUnreadCountResponse(
    IPaginatedList<GetAllNotificationsQueryResponse> PaginatedNotifications,
    int UnreadCount);
