using AppTemplate.Domain.Roles.DomainEvents;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AppTemplate.Application.Features.Roles.Commands.Update.UpdatePermissions;

internal class RemoveRolePermissionEventHandler(
    ILogger<RemoveRolePermissionEventHandler> logger) : INotificationHandler<RolePermissionRemovedDomainEvent>
{
    public async Task Handle(RolePermissionRemovedDomainEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation("Handling RolePermissionRemovedDomainEvent for RoleId: {RoleId}, PermissionId: {PermissionId}",
            notification.RoleId, notification.PermissionId);
    }
}
