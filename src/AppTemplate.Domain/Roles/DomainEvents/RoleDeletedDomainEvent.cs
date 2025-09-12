namespace AppTemplate.Domain.Roles.DomainEvents;

public sealed record RoleDeletedDomainEvent(Guid RoleId) : IDomainEvent;