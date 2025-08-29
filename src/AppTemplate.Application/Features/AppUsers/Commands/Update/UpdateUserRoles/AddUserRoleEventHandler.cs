using AppTemplate.Application.Repositories;
using AppTemplate.Application.Services.Roles;
using AppTemplate.Domain.Users.DomainEvents;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AppTemplate.Application.Features.AppUsers.Commands.Update.UpdateUserRoles;

public class AddUserRoleEventHandler(
    IAppUsersRepository userRepository,
    IRolesService roleService,
    ILogger<AddUserRoleEventHandler> logger) : INotificationHandler<AppUserRoleAddedDomainEvent>
{
  private readonly IAppUsersRepository _userRepository = userRepository;
  private readonly IRolesService _roleService = roleService;

  public async Task Handle(AppUserRoleAddedDomainEvent notification, CancellationToken cancellationToken)
  {
    // just log the event for now

    logger.LogInformation("Handling AppUserRoleAddedDomainEvent for UserId: {UserId}, RoleId: {RoleId}",
        notification.UserId, notification.RoleId);
  }
}