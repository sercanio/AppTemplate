namespace AppTemplate.Domain.Roles;

public sealed class RolePermission
{
  public Guid RoleId { get; set; }

  public Guid PermissionId { get; set; }

  public required Role Role { get; set; }
  public required Permission Permission { get; set; }
}
