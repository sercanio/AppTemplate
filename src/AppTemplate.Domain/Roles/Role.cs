using System.ComponentModel.DataAnnotations.Schema;
using AppTemplate.Domain.AppUsers;
using AppTemplate.Domain.Roles.DomainEvents;
using AppTemplate.Domain.Roles.ValueObjects;

namespace AppTemplate.Domain.Roles;

public sealed class Role : Entity<Guid>, IAggregateRoot
{
  public static readonly Role DefaultRole = Create(
      Guid.Parse("5dc6ec47-5b7c-4c2b-86cd-3a671834e56e"),
      "Registered",
      "kayıtlı",
      isDefault: true);

  public static readonly Role Admin = Create(
      Guid.Parse("4b606d86-3537-475a-aa20-26aadd8f5cfd"),
      "Admin",
      "yönetici",
      isDefault: false);

  private readonly List<AppUser> _users = new();
  public IReadOnlyCollection<AppUser> Users => _users.AsReadOnly();

  private readonly List<Permission> _permissions = new();
  public IReadOnlyCollection<Permission> Permissions => _permissions.AsReadOnly();

  public RoleName Name { get; private set; }
  public RoleName DisplayName { get; private set; }
  public bool IsDefault { get; private set; }

  public Guid? CreatedById { get; private set; } = null;
  [NotMapped]
  public AppUser CreatedBy { get; private set; } = null!;

  public Guid? UpdatedById { get; private set; } = null;
  [NotMapped]
  public AppUser UpdatedBy { get; private set; } = null!;

  public Guid? DeletedById { get; private set; } = null;
  [NotMapped]
  public AppUser DeletedBy { get; private set; } = null!;

  public Role(Guid id, RoleName name, RoleName displayName, Guid? createdById, bool isDefault) : base(id)
  {
    Name = name;
    DisplayName = displayName;
    IsDefault = isDefault;
    CreatedById = createdById;
  }

  // Parameterless constructor for ORM and serialization purposes
  public Role() : base(Guid.NewGuid())
  {
    Name = new RoleName("DefaultRoleName");
    DisplayName = new RoleName("DefaultRoleDisplayName");
    IsDefault = false;
  }

  // Used for data seeding for default roles
  private static Role Create(Guid id, string name, string displayName, bool isDefault = false)
  {
    return new Role(id, new RoleName(name), new RoleName(displayName), null, isDefault);
  }

  public static Role Create(string name, string displayName, Guid createdById, bool isDefault = false)
  {
    var role = new Role(Guid.NewGuid(), new RoleName(name), new RoleName(displayName), createdById, isDefault);
    role.RaiseDomainEvent(new RoleCreatedDomainEvent(role.Id));
    return role;
  }

  public static Role Delete(Role role, Guid deletedById)
  {
    role.MarkDeleted();
    role.DeletedById = deletedById;
    role.RaiseDomainEvent(new RoleDeletedDomainEvent(role.Id));

    return role;
  }

  public void ChangeName(string newName, Guid updatedById)
  {
    var oldName = Name.ToString();
    Name = new RoleName(newName);
    UpdatedById = updatedById;
    MarkUpdated();
    RaiseDomainEvent(new RoleNameUpdatedDomainEvent(Id, oldName));
  }

  public void ChangeDisplayName(string newDisplayName, Guid updatedById)
  {
    var oldDisplayName = DisplayName.ToString();
    DisplayName = new RoleName(newDisplayName);
    UpdatedById = updatedById;
    MarkUpdated();
    RaiseDomainEvent(new RoleNameUpdatedDomainEvent(Id, oldDisplayName));
  }

  // Domain methods for Permissions
  public void AddPermission(Permission permission, Guid updatedById)
  {
    if (!_permissions.Contains(permission))
    {
      _permissions.Add(permission);
      MarkUpdated();
      UpdatedById = updatedById;
      RaiseDomainEvent(new RolePermissionAddedDomainEvent(Id, permission.Id));
    }
  }

  public void RemovePermission(Permission permission, Guid updatedById)
  {
    if (_permissions.Remove(permission))
    {
      UpdatedById = updatedById;
      MarkUpdated();
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
