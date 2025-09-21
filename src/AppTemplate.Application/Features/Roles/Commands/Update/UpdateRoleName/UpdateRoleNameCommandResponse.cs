namespace AppTemplate.Application.Features.Roles.Commands.Update.UpdateRoleName;

public sealed record UpdateRoleNameCommandResponse(Guid Id, string Name, string DisplayName);