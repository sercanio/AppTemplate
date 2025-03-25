using Myrtus.Clarity.Core.Domain.Abstractions;

namespace AppTemplate.Domain.Roles.DomainEvents;

public sealed record RoleNameUpdatedDomainEvent(Guid RoleId, string OldRoleName) : IDomainEvent;