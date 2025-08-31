using AppTemplate.Domain.AppUsers;
using AppTemplate.Domain.AppUsers.DomainEvents;
using AppTemplate.Domain.Roles;
using AppTemplate.Domain.Users.DomainEvents;

namespace AppTemplate.Domain.Tests.Unit.AppUsersTests;

public class AppUsersUnitTests
{
  [Fact]
  public void Create_ShouldInitializeAppUserWithDefaultNotificationPreference()
  {
    // Act
    var user = AppUser.Create();

    // Assert
    Assert.NotNull(user);
    Assert.NotEqual(Guid.Empty, user.Id);
    Assert.NotNull(user.NotificationPreference);
    Assert.True(user.NotificationPreference.IsInAppNotificationEnabled);
    Assert.True(user.NotificationPreference.IsEmailNotificationEnabled);
    Assert.True(user.NotificationPreference.IsPushNotificationEnabled);
  }

  [Fact]
  public void AddRole_ShouldAddRoleToUser()
  {
    var user = AppUser.Create();
    var role = Role.Create("TestRole", "TestDisplayName", Guid.NewGuid());

    user.AddRole(role);

    Assert.Contains(role, user.Roles);
  }

  [Fact]
  public void AddRole_ShouldNotAddDuplicateRole()
  {
    var user = AppUser.Create();
    var role = Role.Create("TestRole", "TestDisplayName", Guid.NewGuid());

    user.AddRole(role);
    user.AddRole(role);

    Assert.Single(user.Roles);
  }

  [Fact]
  public void RemoveRole_ShouldRemoveRoleFromUser()
  {
    var user = AppUser.Create();
    var role = Role.Create("TestRole", "TestDisplayName", Guid.NewGuid());

    user.AddRole(role);
    user.RemoveRole(role);

    Assert.DoesNotContain(role, user.Roles);
  }

  [Fact]
  public void SetIdentityId_ShouldUpdateIdentityId()
  {
    var user = AppUser.Create();
    var identityId = "identity-123";

    user.SetIdentityId(identityId);

    Assert.Equal(identityId, user.IdentityId);
  }

  [Fact]
  public void MarkUsernameAsChanged_ShouldSetFlag()
  {
    var user = AppUser.Create();

    user.MarkUsernameAsChanged();

    Assert.True(user.UserChangedItsUsername);
  }

  [Fact]
  public void SetProfilePictureUrl_ShouldUpdateUrl()
  {
    var user = AppUser.Create();
    var url = "https://example.com/pic.jpg";

    user.SetProfilePictureUrl(url);

    Assert.Equal(url, user.ProfilePictureUrl);
  }

  [Fact]
  public void Create_ShouldRaiseAppUserCreatedDomainEvent()
  {
    var user = AppUser.Create();

    // Assuming you have a way to access domain events, e.g. user.DomainEvents
    var domainEvent = user.GetDomainEvents().OfType<AppUserCreatedDomainEvent>().FirstOrDefault();

    Assert.NotNull(domainEvent);
    Assert.Equal(user.Id, domainEvent.UserId);
  }

  [Fact]
  public void AddRole_ShouldRaiseAppUserRoleAddedDomainEvent()
  {
    var user = AppUser.Create();
    var role = Role.Create("TestRole", "TestDisplayName", Guid.NewGuid());

    user.AddRole(role);

    var domainEvent = user.GetDomainEvents().OfType<AppUserRoleAddedDomainEvent>().FirstOrDefault();

    Assert.NotNull(domainEvent);
    Assert.Equal(user.Id, domainEvent.UserId);
    Assert.Equal(role.Id, domainEvent.RoleId);
  }

  [Fact]
  public void RemoveRole_ShouldRaiseAppUserRoleRemovedDomainEvent()
  {
    var user = AppUser.Create();
    var role = Role.Create("TestRole", "TestDisplayName", Guid.NewGuid());

    user.AddRole(role);
    user.RemoveRole(role);

    var domainEvent = user.GetDomainEvents().OfType<AppUserRoleRemovedDomainEvent>().FirstOrDefault();

    Assert.NotNull(domainEvent);
    Assert.Equal(user.Id, domainEvent.UserId);
    Assert.Equal(role.Id, domainEvent.RoleId);
  }

  [Fact]
  public void RemoveRole_ShouldNotRaiseEventIfRoleNotPresent()
  {
    var user = AppUser.Create();
    var role = Role.Create("TestRole", "TestDisplayName", Guid.NewGuid());

    user.RemoveRole(role);

    var domainEvent = user.GetDomainEvents().OfType<AppUserRoleRemovedDomainEvent>().FirstOrDefault();
    Assert.Null(domainEvent);
  }

  [Fact]
  public void CreateWithoutRolesForSeeding_ShouldInitializeWithFixedGuid()
  {
    var user = AppUser.CreateWithoutRolesForSeeding();

    Assert.Equal(Guid.Parse("55c7f429-0916-4d84-8b76-d45185d89aa7"), user.Id);
    Assert.NotNull(user.NotificationPreference);
  }
}
