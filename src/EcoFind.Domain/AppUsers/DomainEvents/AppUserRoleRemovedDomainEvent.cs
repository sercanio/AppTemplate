using Myrtus.Clarity.Core.Domain.Abstractions;

namespace EcoFind.Domain.Users.DomainEvents;

public sealed record AppUserRoleRemovedDomainEvent(Guid UserId, Guid RoleId) : IDomainEvent;