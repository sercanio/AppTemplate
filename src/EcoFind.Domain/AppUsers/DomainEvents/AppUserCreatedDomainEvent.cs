using Myrtus.Clarity.Core.Domain.Abstractions;

namespace EcoFind.Domain.Users.DomainEvents;

public sealed record AppUserCreatedDomainEvent(Guid UserId) : IDomainEvent;
