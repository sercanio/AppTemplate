using Myrtus.Clarity.Core.Application.Abstractions.Messaging;

namespace EcoFind.Application.Features.Roles.Commands.Delete;

public sealed record DeleteRoleCommand(Guid RoleId) : ICommand<DeleteRoleCommandResponse>;