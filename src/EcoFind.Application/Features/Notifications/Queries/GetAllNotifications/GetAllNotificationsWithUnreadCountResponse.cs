using Myrtus.Clarity.Core.Application.Abstractions.Pagination;

namespace EcoFind.Application.Features.Notifications.Queries.GetAllNotifications;

public sealed record GetAllNotificationsWithUnreadCountResponse(
    IPaginatedList<GetAllNotificationsQueryResponse> PaginatedNotifications,
    int UnreadCount);
