using AppTemplate.Application.Enums;
using Myrtus.Clarity.Core.Application.Abstractions.Messaging;

namespace AppTemplate.Application.Features.AppUsers.Commands.Update.UpdateUserRoles;

public sealed record UpdateUserRolesCommand(
    Guid UserId,
    Operation Operation,
    Guid RoleId) : ICommand<UpdateUserRolesCommandResponse>;

