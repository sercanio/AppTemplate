using Myrtus.Clarity.Core.Application.Abstractions.Messaging;

namespace AppTemplate.Application.Features.Roles.Commands.Create;

public sealed record CreateRoleCommand(string Name, string DisplayName) : ICommand<CreateRoleCommandResponse>;
