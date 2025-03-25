using Myrtus.Clarity.Core.Domain.Abstractions;
using AppTemplate.Domain.AppUsers;
using AppTemplate.Domain.Roles.DomainEvents;
using AppTemplate.Domain.Roles.ValueObjects;

namespace AppTemplate.Domain.Roles;

public sealed class Role : Entity, IAggregateRoot
{
    public static readonly Role DefaultRole = Create(
        Guid.Parse("5dc6ec47-5b7c-4c2b-86cd-3a671834e56e"),
        "Registered",
        isDefault: true);

    public static readonly Role Admin = Create(
        Guid.Parse("4b606d86-3537-475a-aa20-26aadd8f5cfd"),
        "Admin",
        isDefault: false);

    private readonly List<AppUser> _users = new();
    public IReadOnlyCollection<AppUser> Users => _users.AsReadOnly();

    private readonly List<Permission> _permissions = new();
    public IReadOnlyCollection<Permission> Permissions => _permissions.AsReadOnly();

    public RoleName Name { get; private set; }
    public RoleDefaultFlag IsDefault { get; private set; }

    public Role(Guid id, RoleName name, RoleDefaultFlag isDefault) : base(id)
    {
        Name = name;
        IsDefault = isDefault;
    }

    // Parameterless constructor for ORM and serialization purposes
    public Role() : base(Guid.NewGuid())
    {
        Name = new RoleName("DefaultRoleName"); // A valid default\n
        IsDefault = new RoleDefaultFlag(false);
    }

    // Used for data seeding for default roles
    private static Role Create(Guid id, string name, bool isDefault = false)
    {
        return new Role(id, new RoleName(name), new RoleDefaultFlag(isDefault));
    }

    public static Role Create(string name, bool isDefault = false)
    {
        var role = new Role(Guid.NewGuid(), new RoleName(name), new RoleDefaultFlag(isDefault));
        role.RaiseDomainEvent(new RoleCreatedDomainEvent(role.Id));
        return role;
    }

    public static Role Delete(Role role)
    {
        role.RaiseDomainEvent(new RoleDeletedDomainEvent(role.Id));
        role.MarkDeleted();
        return role;
    }

    public void ChangeName(string newName)
    {
        var oldName = Name.ToString();
        Name = new RoleName(newName);
        RaiseDomainEvent(new RoleNameUpdatedDomainEvent(Id, oldName));
    }

    // Domain methods for Permissions
    public void AddPermission(Permission permission)
    {
        if (!_permissions.Contains(permission))
        {
            _permissions.Add(permission);
            RaiseDomainEvent(new RolePermissionAddedDomainEvent(Id, permission.Id));
        }
    }

    public void RemovePermission(Permission permission)
    {
        if (_permissions.Remove(permission))
        {
            RaiseDomainEvent(new RolePermissionRemovedDomainEvent(Id, permission.Id));
        }
    }

    // Domain methods for Users
    public void AddUser(AppUser appUser)
    {
        if (!_users.Contains(appUser))
        {
            _users.Add(appUser);
            // Possibly raise a domain event
        }
    }

    public void RemoveUser(AppUser appUser)
    {
        _users.Remove(appUser);
        // Possibly raise a domain event
    }
}
