using Myrtus.Clarity.Core.Domain.Abstractions;

namespace AppTemplate.Application.Features.Roles.Commands.Update.UpdatePermissions;

public sealed class UpdateRolePermissionsDto : Entity
{
    public Guid Id { get; init; }
    public string Feature { get; init; }
    public string Name { get; init; }

    public UpdateRolePermissionsDto(Guid id, string feature, string name)
    {
        Id = id;
        Feature = feature;
        Name = name;
    }

    private UpdateRolePermissionsDto()
    {
        Feature = string.Empty;
        Name = string.Empty;
    }
}
