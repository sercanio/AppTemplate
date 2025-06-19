using System.Collections.ObjectModel;
using AppTemplate.Application.Features.AppUsers.Queries.GetLoggedInUser;

namespace AppTemplate.Application.Features.AppUsers.Queries.GetAllUsersByRoleId;

public sealed record GetAllUsersByRoleIdQueryResponse
{
    public Guid Id { get; set; }
    public string UserName { get; set; } = string.Empty;
    public ICollection<LoggedInUserRolesDto> Roles { get; set; } = [];

    public GetAllUsersByRoleIdQueryResponse(
        Guid id,
        string userName,
        Collection<LoggedInUserRolesDto> roles)
    {
        Id = id;
        UserName = userName;
        Roles = roles;
    }

    public GetAllUsersByRoleIdQueryResponse(
        Guid id, string email, string userName)
    {
        Id = id;
        UserName = userName;
    }

    public GetAllUsersByRoleIdQueryResponse() { }
}
