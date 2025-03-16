namespace EcoFind.Domain.Roles.DomainEvents;

public sealed record RoleDomainEvents
{
    public static readonly string Created = "RoleCreated";
    public static readonly string Deleted = "RoleDeleted";
    public static readonly string UpdatedName = "RoleNameUpdated";
    public static readonly string AddedPermission = "RolePermissionAdded";
    public static readonly string RemovedPermission = "RolePermissionRemoved";
}