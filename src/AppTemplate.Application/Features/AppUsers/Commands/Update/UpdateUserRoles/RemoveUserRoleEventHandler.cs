using AppTemplate.Application.Repositories;
using AppTemplate.Application.Services.Roles;
using AppTemplate.Domain.Users.DomainEvents;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AppTemplate.Application.Features.AppUsers.Commands.Update.UpdateUserRoles;

public class RemoveUserRoleEventHandler(
    IAppUsersRepository userRepository,
    IRolesService roleRepository,
    ILogger<RemoveUserRoleEventHandler> logger) : INotificationHandler<AppUserRoleRemovedDomainEvent>
{
  private readonly IAppUsersRepository _userRepository = userRepository;
  private readonly IRolesService _roleService = roleRepository;

  public async Task Handle(AppUserRoleRemovedDomainEvent notification, CancellationToken cancellationToken)
  {
    logger.LogInformation("Handling AppUserRoleRemovedDomainEvent for UserId: {UserId}, RoleId: {RoleId}",
        notification.UserId, notification.RoleId);
  }
}
