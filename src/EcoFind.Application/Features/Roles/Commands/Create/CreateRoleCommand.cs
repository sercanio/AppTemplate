using Myrtus.Clarity.Core.Application.Abstractions.Messaging;

namespace EcoFind.Application.Features.Roles.Commands.Create;

public sealed record CreateRoleCommand(string Name) : ICommand<CreateRoleCommandResponse>;
