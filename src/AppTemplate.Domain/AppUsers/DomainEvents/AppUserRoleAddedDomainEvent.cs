using Myrtus.Clarity.Core.Domain.Abstractions;

namespace AppTemplate.Domain.Users.DomainEvents;

public sealed record AppUserRoleAddedDomainEvent(Guid UserId, Guid RoleId) : IDomainEvent;