using AppTemplate.Application.Services.Messages;

namespace AppTemplate.Application.Features.Notifications.Queries.GetAllNotifications;

public sealed record GetAllNotificationsQuery(
    int PageIndex,
    int PageSize,
    CancellationToken CancellationToken
) : IQuery<GetAllNotificationsWithUnreadCountResponse>;

