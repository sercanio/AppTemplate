using AppTemplate.Domain.Roles.DomainEvents;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AppTemplate.Application.Features.Roles.Commands.Update.UpdatePermissions;

public class AddRolePermissionEventHandler(
    ILogger<AddRolePermissionEventHandler> logger) : INotificationHandler<RolePermissionAddedDomainEvent>
{

  public async Task Handle(RolePermissionAddedDomainEvent notification, CancellationToken cancellationToken)
  {
    logger.LogInformation("Handling RolePermissionAddedDomainEvent for RoleId: {RoleId}, PermissionId: {PermissionId}",
        notification.RoleId, notification.PermissionId);
  }
}
