namespace AppTemplate.Application.Features.Notifications.Queries.GetAllNotifications;

public sealed record GetAllNotificationsQueryResponse(
      Guid Id,
      string UserId,
      string User,
      string Action,
      string Entity,
      string EntityId,
      DateTime Timestamp,
      string Details,
      bool IsRead);
