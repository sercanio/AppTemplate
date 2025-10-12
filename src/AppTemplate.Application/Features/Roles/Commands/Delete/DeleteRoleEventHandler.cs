using AppTemplate.Domain.Roles.DomainEvents;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AppTemplate.Application.Features.Roles.Commands.Delete;

public class DeleteRoleEventHandler(
    ILogger<DeleteRoleEventHandler> logger) : INotificationHandler<RoleDeletedDomainEvent>
{

  public async Task Handle(RoleDeletedDomainEvent notification, CancellationToken cancellationToken)
  {
    logger.LogInformation("Handling RoleDeletedDomainEvent for RoleId: {RoleId}", notification.RoleId);
  }
}
