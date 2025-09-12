using AppTemplate.Application.Services.Messages;

namespace AppTemplate.Application.Features.Roles.Commands.Create;

public sealed record CreateRoleCommand(string Name, string DisplayName) : ICommand<CreateRoleCommandResponse>;
