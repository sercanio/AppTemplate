﻿using Myrtus.Clarity.Core.Domain.Abstractions;

namespace AppTemplate.Domain.Roles.DomainEvents;

public sealed record RoleDeletedDomainEvent(Guid RoleId) : IDomainEvent;