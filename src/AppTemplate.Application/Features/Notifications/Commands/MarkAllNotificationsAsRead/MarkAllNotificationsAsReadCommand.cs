using AppTemplate.Application.Services.Messages;

namespace AppTemplate.Application.Features.Notifications.Commands.MarkAllNotificationsAsRead;

public sealed record MarkAllNotificationsAsReadCommand(
        CancellationToken CancellationToken) : ICommand<MarkAllNotificationsAsReadCommandResponse>;
