namespace AppTemplate.Application.Features.Notifications.Commands.MarkNotificationsAsRead;

public sealed class MarkNotificationAsReadCommandResponse
{
  public bool Success { get; }
  public MarkNotificationAsReadCommandResponse(bool success) => Success = success;
}

