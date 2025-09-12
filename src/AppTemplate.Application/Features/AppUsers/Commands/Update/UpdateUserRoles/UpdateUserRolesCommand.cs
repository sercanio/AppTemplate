using AppTemplate.Application.Enums;
using AppTemplate.Application.Services.Messages;

namespace AppTemplate.Application.Features.AppUsers.Commands.Update.UpdateUserRoles;

public sealed record UpdateUserRolesCommand(
    Guid UserId,
    Operation Operation,
    Guid RoleId) : ICommand<UpdateUserRolesCommandResponse>;

