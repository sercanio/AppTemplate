using AppTemplate.Core.Application.Abstractions.Messaging;

namespace AppTemplate.Application.Features.Notifications.Queries.GetAllNotifications;

public sealed record GetAllNotificationsQuery(
    int PageIndex,
    int PageSize,
    CancellationToken CancellationToken
) : IQuery<GetAllNotificationsWithUnreadCountResponse>;

