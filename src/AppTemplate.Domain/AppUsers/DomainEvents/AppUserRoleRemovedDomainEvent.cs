using AppTemplate.Core.Domain.Abstractions;

namespace AppTemplate.Domain.Users.DomainEvents;

public sealed record AppUserRoleRemovedDomainEvent(Guid UserId, Guid RoleId) : IDomainEvent;