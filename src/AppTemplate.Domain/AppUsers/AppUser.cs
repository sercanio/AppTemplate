using Microsoft.AspNetCore.Identity;
using Myrtus.Clarity.Core.Domain.Abstractions;
using Newtonsoft.Json;
using AppTemplate.Domain.AppUsers.ValueObjects;
using AppTemplate.Domain.Roles;
using AppTemplate.Domain.Users.DomainEvents;
using AppTemplate.Domain.AuditLogs;

namespace AppTemplate.Domain.AppUsers;

public sealed class AppUser : Entity, IAggregateRoot
{
    private readonly List<Role> _roles = [];
    private readonly List<AuditLog> _auditLogs = [];

    public IReadOnlyCollection<Role> Roles => _roles.AsReadOnly();

    [JsonIgnore]
    public IReadOnlyCollection<AuditLog> AuditLogs => _auditLogs.AsReadOnly();

    public NotificationPreference NotificationPreference { get; private set; }

    public string IdentityId { get; private set; }
    public IdentityUser IdentityUser { get; private set; }

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
        appUser.AddRole(Role.DefaultRole);
        appUser.UpdatedBy = "System";
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
    }

    public void RemoveRole(Role role)
    {
        if (_roles.Remove(role))
        {
            RaiseDomainEvent(new AppUserRoleRemovedDomainEvent(Id, role.Id));
        }
    }

    public void SetIdentityId(string identityId)
    {
        IdentityId = identityId;
    }
}
