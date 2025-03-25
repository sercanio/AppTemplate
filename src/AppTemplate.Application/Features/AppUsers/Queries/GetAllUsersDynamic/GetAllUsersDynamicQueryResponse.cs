using System.Collections.ObjectModel;
using AppTemplate.Application.Features.AppUsers.Queries.GetLoggedInUser;

namespace AppTemplate.Application.Features.AppUsers.Queries.GetAllUsersDynamic;

public sealed record GetAllUsersDynamicQueryResponse
{
    public Guid Id { get; init; }
    public string Email { get; init; }
    public string UserName { get; init; }
    public ICollection<LoggedInUserRolesDto> Roles { get; init; } = [];

    public GetAllUsersDynamicQueryResponse(
        Guid id,
        string email,
        string userName,
        Collection<LoggedInUserRolesDto> roles)
    {
        Id = id;
        Email = email;
        UserName = userName;
        Roles = roles;
    }
}