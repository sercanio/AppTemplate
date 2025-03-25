using Myrtus.Clarity.Core.Domain.Abstractions;

namespace AppTemplate.Domain.Users.DomainEvents;

public sealed record AppUserCreatedDomainEvent(Guid UserId) : IDomainEvent;
