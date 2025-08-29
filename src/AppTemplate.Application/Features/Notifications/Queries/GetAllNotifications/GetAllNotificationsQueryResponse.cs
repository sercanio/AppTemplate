using AppTemplate.Domain.Notifications.Enums;

namespace AppTemplate.Application.Features.Notifications.Queries.GetAllNotifications;

public sealed record GetAllNotificationsQueryResponse(
        Guid Id,
        Guid RecipientId,
        string Title,
        string Message,
        NotificationTypeEnum Type,
        DateTime CreatedOnUtc,
        bool IsRead);
