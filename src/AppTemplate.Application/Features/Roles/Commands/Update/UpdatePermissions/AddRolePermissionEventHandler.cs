using AppTemplate.Application.Repositories;
using AppTemplate.Application.Services.AuditLogs;
using AppTemplate.Domain.AuditLogs;
using AppTemplate.Domain.Roles.DomainEvents;
using MediatR;

namespace AppTemplate.Application.Features.Roles.Commands.Update.UpdatePermissions;

internal class AddRolePermissionEventHandler(
    IRolesRepository roleRepository,
    IPermissionsRepository permissionRepository,
    IAuditLogService auditLogService) : INotificationHandler<RolePermissionAddedDomainEvent>
{
    private readonly IRolesRepository _roleRepository = roleRepository;
    private readonly IPermissionsRepository _permissionRepository = permissionRepository;
    private readonly IAuditLogService _auditLogService = auditLogService;

    public async Task Handle(RolePermissionAddedDomainEvent notification, CancellationToken cancellationToken)
    {
        var role = await _roleRepository.GetAsync(
            predicate: role => role.Id == notification.RoleId,
            cancellationToken: cancellationToken);

        var permission = await _permissionRepository.GetAsync(
            predicate: permission => permission.Id == notification.PermissionId,
            cancellationToken: cancellationToken);

        AuditLog log = new()
        {
            User = role!.UpdatedBy!,
            Action = RoleDomainEvents.AddedPermission,
            Entity = role.GetType().Name,
            EntityId = role.Id.ToString(),
            Details = $"{role.GetType().Name} '{role.Name}' has been granted permission '{permission!.Name}'."
        };
        await _auditLogService.LogAsync(log);
    }
}
