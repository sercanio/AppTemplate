using Myrtus.Clarity.Core.Domain.Abstractions;

namespace EcoFind.Domain.Roles.DomainEvents;

public sealed record RoleDeletedDomainEvent(Guid RoleId) : IDomainEvent;