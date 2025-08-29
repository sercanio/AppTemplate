using AppTemplate.Core.Domain.Abstractions;

namespace AppTemplate.Domain.Roles.DomainEvents;

public sealed record RolePermissionAddedDomainEvent(Guid RoleId, Guid PermissionId) : IDomainEvent;