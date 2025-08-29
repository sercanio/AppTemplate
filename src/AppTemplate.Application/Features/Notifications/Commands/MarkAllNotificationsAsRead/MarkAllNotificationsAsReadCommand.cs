using Myrtus.Clarity.Core.Application.Abstractions.Messaging;
using AppTemplate.Application.Features.Notifications.Commands.MarkAllNotificationsAsRead;

namespace AppTemplate.Application.Features.Notifications.Commands.MarkAllNotificationsAsRead;

public sealed record MarkAllNotificationsAsReadCommand(
        CancellationToken CancellationToken) : ICommand<MarkAllNotificationsAsReadCommandResponse>;
