using System.Collections.ObjectModel;

namespace EcoFind.Application.Features.AppUsers.Queries.GetLoggedInUser;

public sealed record GetAllUsersQueryResponse
{
    public Guid Id { get; set; }
    public string Email { get; set; }
    public string? UserName { get; set; }
    public ICollection<LoggedInUserRolesDto> Roles { get; set; } = new List<LoggedInUserRolesDto>();

    public GetAllUsersQueryResponse(
        Guid id,
        string email,
        string? userName,
        Collection<LoggedInUserRolesDto>? roles = null)
    {
        Id = id;
        Email = email;
        UserName = userName;
        Roles = roles ?? new Collection<LoggedInUserRolesDto>();
    }

    public GetAllUsersQueryResponse() { }
}
