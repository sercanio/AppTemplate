using AppTemplate.Domain.AppUsers.ValueObjects;
using AppTemplate.Domain.Notifications;
using AppTemplate.Domain.Roles;
using AppTemplate.Domain.Users.DomainEvents;
using Microsoft.AspNetCore.Identity;
using Myrtus.Clarity.Core.Domain.Abstractions;

namespace AppTemplate.Domain.AppUsers;

public sealed class AppUser : Entity<Guid>, IAggregateRoot
{
  private readonly List<Role> _roles = [];
  private readonly List<AppUser> _updatedUsers = [];
  private readonly List<AppUser> _deletedUsers = [];
  public readonly Guid AdminId = Guid.Parse("b3398ff2-1b43-4af7-812d-eb4347eecbb8");

  public IReadOnlyCollection<Role> Roles => _roles.AsReadOnly();

  public ICollection<Notification> Notifications { get; private set; } = new List<Notification>();
  public IReadOnlyCollection<AppUser> UpdatedUsers => _updatedUsers.AsReadOnly();
  public IReadOnlyCollection<AppUser> DeletedUsers => _deletedUsers.AsReadOnly();

  public NotificationPreference NotificationPreference { get; private set; }
  public string IdentityId { get; private set; }
  public IdentityUser IdentityUser { get; private set; }
  public bool UserChangedItsUsername { get; private set; } = false;

  public string? ProfilePictureUrl { get; private set; }
  public string? Biography { get; private set; }

  public Guid? CreatedById { get; private set; } = null;
  public AppUser CreatedBy { get; private set; } = null!;

  public Guid? UpdatedById { get; private set; } = null;
  public AppUser UpdatedBy { get; private set; } = null!;

  public Guid? DeletedById { get; private set; } = null;
  public AppUser DeletedBy { get; private set; } = null!;

  private AppUser(Guid id, NotificationPreference notificationPreference) : base(id)
  {
    NotificationPreference = notificationPreference;
  }

  private AppUser()
  {
  }

  public static AppUser Create()
  {
    NotificationPreference notificationPreference = new(true, true, true);
    var appUser = new AppUser(Guid.NewGuid(), notificationPreference);
    appUser.RaiseDomainEvent(new AppUserCreatedDomainEvent(appUser.Id));

    // this cause duplicate roles in seeding 
    // we are not adding default role here
    //appUser._roles.Add(Role.DefaultRole);
    return appUser;
  }

  public static AppUser CreateWithoutRolesForSeeding()
  {
    NotificationPreference notificationPreference = new(true, true, true);
    return new AppUser(Guid.Parse("55c7f429-0916-4d84-8b76-d45185d89aa7"), notificationPreference);
  }

  public void AddRole(Role role)
  {
    if (!_roles.Contains(role))
    {
      _roles.Add(role);
      RaiseDomainEvent(new AppUserRoleAddedDomainEvent(Id, role.Id));
    }

    MarkUpdated();
  }

  public void RemoveRole(Role role)
  {
    if (_roles.Remove(role))
    {
      RaiseDomainEvent(new AppUserRoleRemovedDomainEvent(Id, role.Id));
    }

    MarkUpdated();
  }

  public void SetIdentityId(string identityId)
  {
    IdentityId = identityId;
  }

  public void MarkUsernameAsChanged()
  {
    UserChangedItsUsername = true;
  }

  public void SetProfilePictureUrl(string? url)
  {
    ProfilePictureUrl = url;
    MarkUpdated();
  }
}
