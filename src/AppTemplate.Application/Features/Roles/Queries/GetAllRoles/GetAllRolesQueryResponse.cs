namespace AppTemplate.Application.Features.Roles.Queries.GetAllRoles;

public sealed record GetAllRolesQueryResponse(string Id, string Name, string DisplayName, bool IsDefault);