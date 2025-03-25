using MediatR;
using Myrtus.Clarity.Core.Application.Abstractions.Auditing;
using Myrtus.Clarity.Core.Domain.Abstractions;
using AppTemplate.Application.Repositories;
using AppTemplate.Application.Services.Roles;
using AppTemplate.Domain.Users.DomainEvents;

namespace AppTemplate.Application.Features.AppUsers.Commands.Update.UpdateUserRoles;

public class RemoveUserRoleEventHandler(
    IAppUsersRepository userRepository,
    IRolesService roleRepository,
    IAuditLogService auditLogService) : INotificationHandler<AppUserRoleRemovedDomainEvent>
{
    private readonly IAppUsersRepository _userRepository = userRepository;
    private readonly IRolesService _roleService = roleRepository;
    private readonly IAuditLogService _auditLogService = auditLogService;

    public async Task Handle(AppUserRoleRemovedDomainEvent notification, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetAsync(
            predicate: u => u.Id == notification.UserId,
            includeSoftDeleted: false,
            cancellationToken: cancellationToken,
            include: [
                u => u.Roles,
                u => u.IdentityUser
                ]);

        var role = await _roleService.GetAsync(
            predicate: role => role.Id == notification.RoleId,
            includeSoftDeleted: true,
            cancellationToken: cancellationToken);

        AuditLog log = new()
        {
            User = user!.UpdatedBy!,
            Action = AppUserDomainEvents.RemovedRole,
            Entity = user.GetType().Name,
            EntityId = user.Id.ToString(),
            Details = $"{user.GetType().Name} '{user.IdentityUser.Email}' has been revoked the role '{role!.Name.Value}'."
        };
        await _auditLogService.LogAsync(log);
    }
}
