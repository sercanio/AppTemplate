using Myrtus.Clarity.Core.Domain.Abstractions;

namespace EcoFind.Domain.Roles.DomainEvents;

public sealed record RoleCreatedDomainEvent(Guid RoleId) : IDomainEvent;