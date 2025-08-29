using AppTemplate.Application.Enums;
using Myrtus.Clarity.Core.Application.Abstractions.Messaging;

namespace AppTemplate.Application.Features.Roles.Commands.Update.UpdatePermissions;

public sealed record UpdateRolePermissionsCommand(
    Guid RoleId,
    Guid PermissionId,
    Operation Operation) : ICommand<UpdateRolePermissionsCommandResponse>;