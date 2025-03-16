using Myrtus.Clarity.Core.Application.Abstractions.Messaging;
using Myrtus.Clarity.Core.Domain.Abstractions;

namespace EcoFind.Application.Features.Notifications.Commands.MarkNotificationsAsRead;

public sealed record MarkNotificationsAsReadCommand(
        CancellationToken CancellationToken) : ICommand<MarkNotificationsAsReadCommandResponse>;
