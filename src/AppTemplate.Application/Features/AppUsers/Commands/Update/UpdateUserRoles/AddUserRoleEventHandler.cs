using AppTemplate.Domain.Users.DomainEvents;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AppTemplate.Application.Features.AppUsers.Commands.Update.UpdateUserRoles;

public class AddUserRoleEventHandler(
    ILogger<AddUserRoleEventHandler> logger) : INotificationHandler<AppUserRoleAddedDomainEvent>
{

  public async Task Handle(AppUserRoleAddedDomainEvent notification, CancellationToken cancellationToken)
  {

    logger.LogInformation("Handling AppUserRoleAddedDomainEvent for UserId: {UserId}, RoleId: {RoleId}",
        notification.UserId, notification.RoleId);
  }
}