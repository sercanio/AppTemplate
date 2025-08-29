using AppTemplate.Core.Application.Abstractions.Messaging;

namespace AppTemplate.Application.Features.Notifications.Commands.MarkAllNotificationsAsRead;

public sealed record MarkAllNotificationsAsReadCommand(
        CancellationToken CancellationToken) : ICommand<MarkAllNotificationsAsReadCommandResponse>;
