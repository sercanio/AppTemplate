using Myrtus.Clarity.Core.Application.Abstractions.Messaging;
using Myrtus.Clarity.Core.Application.Abstractions.Pagination;

namespace AppTemplate.Application.Features.Notifications.Queries.GetAllNotifications;

public sealed record GetAllNotificationsQuery(
    int PageIndex,
    int PageSize,
    CancellationToken CancellationToken
) : IQuery<GetAllNotificationsWithUnreadCountResponse>;

