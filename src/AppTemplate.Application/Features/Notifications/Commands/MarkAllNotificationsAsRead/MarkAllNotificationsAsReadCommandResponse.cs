namespace AppTemplate.Application.Features.Notifications.Commands.MarkAllNotificationsAsRead;

public sealed class MarkAllNotificationsAsReadCommandResponse
{
    public bool Success { get; }
    public MarkAllNotificationsAsReadCommandResponse(bool success) => Success = success;
}

