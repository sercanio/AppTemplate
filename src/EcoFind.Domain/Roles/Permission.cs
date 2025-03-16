using Myrtus.Clarity.Core.Domain.Abstractions;

namespace EcoFind.Domain.Roles;

public sealed class Permission : Entity
{
    public static readonly Permission UsersRead = new(Guid.Parse("33261a4a-c423-4876-8f15-e40068aea5ca"), "users", "users:read");
    public static readonly Permission UsersCreate = new(Guid.Parse("9f79a54c-0b54-4de5-94b9-8582a5f32e78"), "users", "users:create");
    public static readonly Permission UsersUpdate = new(Guid.Parse("25bb194c-ea15-4339-9f45-5a895c51b626"), "users", "users:update");
    public static readonly Permission UsersDelete = new(Guid.Parse("559dd4ec-4d2e-479d-a0a9-5229ecc04fb4"), "users", "users:delete");

    public static readonly Permission RolesRead = new(Guid.Parse("d066e4ee-6af2-4857-bd40-b9b058fa2201"), "roles", "roles:read");
    public static readonly Permission RolesCreate = new(Guid.Parse("940c88ad-24fe-4d86-a982-fa5ea224edba"), "roles", "roles:create");
    public static readonly Permission RolesUpdate = new(Guid.Parse("346d3cc6-ac81-42b1-8539-cd53f42b6566"), "roles", "roles:update");
    public static readonly Permission RolesDelete = new(Guid.Parse("386e40e9-da38-4d2f-8d02-ac4cbaddf760"), "roles", "roles:delete");

    public static readonly Permission PermissionsRead = new(Guid.Parse("0eeb5f27-10fd-430a-9257-a8457107141a"), "permissions", "permissions:read");

    public static readonly Permission AuditLogsRead = new(Guid.Parse("3050d953-5dcf-4eb0-a18d-a3ce62a0dd3c"), "auditlogs", "auditlogs:read");

    public static readonly Permission NotificationsRead = new(Guid.Parse("a03a127b-9a03-46a0-b709-b6919f2598be"), "notifications", "notifications:read");
    public static readonly Permission NotificationsUpdate = new(Guid.Parse("a5585e9e-ec65-431b-9bb9-9bbc1663ebb8"), "notifications", "notifications:update");
    
    public Permission(Guid id, string feature, string name) : base(id)
    {
        Feature = feature;
        Name = name;
        Roles = new List<Role>([]);
    }

    internal Permission()
    {
        Feature = string.Empty;
        Name = string.Empty;
        Roles = new List<Role>([]);
    }

    public string Feature { get; set; }
    public string Name { get; init; }
    public IList<Role> Roles {  get; set; }

}
