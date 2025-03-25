using MediatR;
using Myrtus.Clarity.Core.Application.Abstractions.Auditing;
using Myrtus.Clarity.Core.Domain.Abstractions;
using AppTemplate.Application.Repositories;
using AppTemplate.Application.Services.Roles;
using AppTemplate.Domain.Users.DomainEvents;

namespace AppTemplate.Application.Features.AppUsers.Commands.Update.UpdateUserRoles;

public class AddUserRoleEventHandler(
    IAppUsersRepository userRepository,
    IRolesService roleService,
    IAuditLogService auditLogService) : INotificationHandler<AppUserRoleAddedDomainEvent>
{
    private readonly IAppUsersRepository _userRepository = userRepository;
    private readonly IRolesService _roleService = roleService;
    private readonly IAuditLogService _auditLogService = auditLogService;

    public async Task Handle(AppUserRoleAddedDomainEvent notification, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetUserByIdAsync(notification.UserId, cancellationToken);

        var role = await _roleService.GetAsync(
            predicate: role => role.Id == notification.RoleId,
            cancellationToken: cancellationToken);

        AuditLog log = new()
        {
            User = user!.UpdatedBy!,
            Action = AppUserDomainEvents.AddedRole,
            Entity = user.GetType().Name,
            EntityId = user.Id.ToString(),
            Details = $"{user.GetType().Name} '{user.IdentityUser.Email}' has been granted a new role '{role!.Name}'."
        };
        await _auditLogService.LogAsync(log);
    }
}