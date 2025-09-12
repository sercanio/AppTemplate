using AppTemplate.Application.Services.Messages;

namespace AppTemplate.Application.Features.Notifications.Commands.MarkNotificationsAsRead;

public sealed record MarkNotificationAsReadCommand(Guid NotificationId) : ICommand<MarkNotificationAsReadCommandResponse>;
