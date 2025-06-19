using AppTemplate.Domain.Roles;

namespace AppTemplate.Infrastructure.Autorization;

internal sealed class UserRolesResponse
{
    public Guid UserId { get; init; }
    public ICollection<Role> Roles { get; init; } = [];
}
