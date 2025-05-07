using AppTemplate.Application.Repositories;
using AppTemplate.Application.Services.AuditLogs;
using AppTemplate.Domain.AuditLogs;
using AppTemplate.Domain.Roles.DomainEvents;
using MediatR;

namespace AppTemplate.Application.Features.Roles.Commands.Delete;

internal class DeleteRoleEventHandler(IRolesRepository rolesRepository, IAuditLogService auditLogService) : INotificationHandler<RoleDeletedDomainEvent>
{
    private readonly IRolesRepository _rolesRepository = rolesRepository;
    private readonly IAuditLogService _auditLogService = auditLogService;

    public async Task Handle(RoleDeletedDomainEvent notification, CancellationToken cancellationToken)
    {
        var role = await _rolesRepository.GetAsync(
            predicate: role => role.Id == notification.RoleId,
            includeSoftDeleted: true,
            cancellationToken: cancellationToken);

        AuditLog log = new()
        {
            User = role!.UpdatedBy!,
            Action = RoleDomainEvents.Deleted,
            Entity = role.GetType().Name,
            EntityId = role.Id.ToString(),
            Details = $"{role.GetType().Name} '{role.Name}' has been deleted."
        };
        await _auditLogService.LogAsync(log);
    }
}
