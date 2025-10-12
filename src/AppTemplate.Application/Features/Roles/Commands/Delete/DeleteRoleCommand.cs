using AppTemplate.Application.Services.Messages;

namespace AppTemplate.Application.Features.Roles.Commands.Delete;

public sealed record DeleteRoleCommand(Guid RoleId) : ICommand<DeleteRoleCommandResponse>;