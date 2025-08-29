using AppTemplate.Core.Application.Abstractions.Messaging;

namespace AppTemplate.Application.Features.Notifications.Commands.MarkNotificationsAsRead;

public sealed record MarkNotificationAsReadCommand(Guid NotificationId) : ICommand<MarkNotificationAsReadCommandResponse>;
