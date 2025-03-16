using Myrtus.Clarity.Core.Application.Abstractions.Messaging;

namespace EcoFind.Application.Features.Roles.Commands.Update.UpdateRoleName;

public sealed record UpdateRoleNameCommand(
    Guid RoleId,
    string Name) : ICommand<UpdateRoleNameCommandResponse>;