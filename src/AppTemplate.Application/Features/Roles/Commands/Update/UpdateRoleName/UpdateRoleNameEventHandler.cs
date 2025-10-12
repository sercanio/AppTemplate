using AppTemplate.Domain.Roles.DomainEvents;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AppTemplate.Application.Features.Roles.Commands.Update.UpdateRoleName;

public class UpdateRoleNameEventHandler(
    ILogger<UpdateRoleNameEventHandler> logger) : INotificationHandler<RoleNameUpdatedDomainEvent>
{
  public async Task Handle(RoleNameUpdatedDomainEvent notification, CancellationToken cancellationToken)
  {
    logger.LogInformation("Handling RoleNameUpdatedDomainEvent for RoleId: {RoleId}",
        notification.RoleId);
  }
}
