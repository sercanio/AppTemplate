using AppTemplate.Domain.Roles.ValueObjects;

namespace AppTemplate.Application.Features.AppUsers.Queries.GetLoggedInUser;

public sealed record LoggedInUserRolesDto
{
    public Guid Id { get; set; }
    public string Name { get; set; }

    public LoggedInUserRolesDto(Guid id, string name)
    {
        Id = id;
        Name = name;
    }
};