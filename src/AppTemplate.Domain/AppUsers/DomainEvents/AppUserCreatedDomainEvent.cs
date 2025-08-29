using AppTemplate.Core.Domain.Abstractions;

namespace AppTemplate.Domain.AppUsers.DomainEvents;

public sealed record AppUserCreatedDomainEvent(Guid UserId) : IDomainEvent;
