using AppTemplate.Application.Enums;
using AppTemplate.Application.Services.Messages;

namespace AppTemplate.Application.Features.Roles.Commands.Update.UpdatePermissions;

public sealed record UpdateRolePermissionsCommand(
    Guid RoleId,
    Guid PermissionId,
    Operation Operation) : ICommand<UpdateRolePermissionsCommandResponse>;