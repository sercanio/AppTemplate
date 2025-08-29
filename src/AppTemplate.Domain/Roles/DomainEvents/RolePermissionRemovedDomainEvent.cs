using AppTemplate.Core.Domain.Abstractions;

namespace AppTemplate.Domain.Roles.DomainEvents;

public sealed record RolePermissionRemovedDomainEvent(Guid RoleId, Guid PermissionId) : IDomainEvent;