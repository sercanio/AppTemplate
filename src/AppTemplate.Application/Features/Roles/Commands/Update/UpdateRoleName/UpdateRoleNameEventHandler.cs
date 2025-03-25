using AppTemplate.Application.Repositories;
using AppTemplate.Domain.Roles.DomainEvents;
using MediatR;
using Myrtus.Clarity.Core.Application.Abstractions.Auditing;
using Myrtus.Clarity.Core.Domain.Abstractions;

namespace AppTemplate.Application.Features.Roles.Commands.Update.UpdateRoleName;

internal class UpdateRoleNameEventHandler(
    IRolesRepository roleRepository,
    IAuditLogService auditLogService) : INotificationHandler<RoleNameUpdatedDomainEvent>
{
    private readonly IRolesRepository _roleRepository = roleRepository;
    private readonly IAuditLogService _auditLogService = auditLogService;

    public async Task Handle(RoleNameUpdatedDomainEvent notification, CancellationToken cancellationToken)
    {
        var role = await _roleRepository.GetAsync(
            predicate: role => role.Id == notification.RoleId,
            cancellationToken: cancellationToken);

        string oldName = notification.OldRoleName;

        AuditLog log = new()
        {
            User = role!.UpdatedBy!,
            Action = RoleDomainEvents.UpdatedName,
            Entity = role.GetType().Name,
            EntityId = role.Id.ToString(),
            Details = $"{role.GetType().Name} '{oldName}' has been updated to '{role.Name}'."
        };
        await _auditLogService.LogAsync(log);
    }
}
