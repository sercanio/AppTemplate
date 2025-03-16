using Myrtus.Clarity.Core.Domain.Abstractions;

namespace EcoFind.Domain.Posts.DomainEvents;

public sealed record PostDraftedDomainEvent(Post Post) : IDomainEvent;
