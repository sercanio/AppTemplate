using AppTemplate.Application.Services.Messages;

namespace AppTemplate.Application.Features.Roles.Commands.Update.UpdateRoleName;

public sealed record UpdateRoleNameCommand(
    Guid RoleId,
    string Name,
    string DisplayName) : ICommand<UpdateRoleNameCommandResponse>;