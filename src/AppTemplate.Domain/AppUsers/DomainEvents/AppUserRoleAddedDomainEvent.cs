namespace AppTemplate.Domain.Users.DomainEvents;

public sealed record AppUserRoleAddedDomainEvent(Guid UserId, Guid RoleId) : IDomainEvent;