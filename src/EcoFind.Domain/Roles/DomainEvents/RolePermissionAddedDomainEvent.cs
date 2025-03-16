using Myrtus.Clarity.Core.Domain.Abstractions;

namespace EcoFind.Domain.Roles.DomainEvents;

public sealed record RolePermissionAddedDomainEvent(Guid RoleId, Guid PermissionId) : IDomainEvent;