using AppTemplate.Domain.Roles;

namespace AppTemplate.Application.Services.Authorization;

public sealed class UserRolesResponse
{
    public Guid UserId { get; init; }
    public ICollection<Role> Roles { get; init; } = [];
}
