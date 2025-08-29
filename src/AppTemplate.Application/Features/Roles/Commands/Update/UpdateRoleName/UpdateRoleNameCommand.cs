using Myrtus.Clarity.Core.Application.Abstractions.Messaging;

namespace AppTemplate.Application.Features.Roles.Commands.Update.UpdateRoleName;

public sealed record UpdateRoleNameCommand(
    Guid RoleId,
    string Name,
    string DisplayName) : ICommand<UpdateRoleNameCommandResponse>;