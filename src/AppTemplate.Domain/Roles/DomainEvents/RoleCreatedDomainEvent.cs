using Myrtus.Clarity.Core.Domain.Abstractions;

namespace AppTemplate.Domain.Roles.DomainEvents;

public sealed record RoleCreatedDomainEvent(Guid RoleId) : IDomainEvent;