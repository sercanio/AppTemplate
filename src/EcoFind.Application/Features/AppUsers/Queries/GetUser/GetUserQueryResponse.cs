using EcoFind.Application.Features.Roles.Queries.GetRoleById;
using System.Collections.ObjectModel;

namespace EcoFind.Application.Features.AppUsers.Queries.GetUser;

public sealed record GetUserQueryResponse
{
    public Guid Id { get; set; }
    public string Email { get; set; }
    public string UserName { get; set; }
    public ICollection<GetRoleByIdQueryResponse> Roles { get; set; } = [];

    public GetUserQueryResponse(Guid id,
        string email,
        string userName,
        Collection<GetRoleByIdQueryResponse> roles)
    {
        Id = id;
        Email = email;
        UserName = userName;
        Roles = roles;
    }

    public GetUserQueryResponse(Guid id, string email, string userName)
    {
        Id = id;
        Email = email;
        UserName = userName;
    }

    public GetUserQueryResponse() { }
}
