using AppTemplate.Domain.AppUsers;
using AppTemplate.Domain.AppUsers.DomainEvents;
using AppTemplate.Domain.AppUsers.ValueObjects;
using AppTemplate.Domain.Notifications;
using AppTemplate.Domain.Roles;
using AppTemplate.Domain.Users.DomainEvents;

namespace AppTemplate.Domain.Tests.Unit.AppUsersTests;

[Trait("Category", "Unit")]
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

  [Fact]
  public void MarkUsernameAsChanged_ShouldSetFlagToTrue()
  {
    // Arrange
    var user = AppUser.Create();
    Assert.False(user.UserChangedItsUsername); // Initially false

    // Act
    user.MarkUsernameAsChanged();

    // Assert
    Assert.True(user.UserChangedItsUsername);
  }

  [Fact]
  public void SetProfilePictureUrl_WithValidUrl_ShouldUpdateUrl()
  {
    // Arrange
    var user = AppUser.Create();
    var url = "https://example.com/profile.jpg";

    // Act
    user.SetProfilePictureUrl(url);

    // Assert
    Assert.Equal(url, user.ProfilePictureUrl);
  }

  [Fact]
  public void SetProfilePictureUrl_WithNullUrl_ShouldSetNull()
  {
    // Arrange
    var user = AppUser.Create();

    // Act
    user.SetProfilePictureUrl(null);

    // Assert
    Assert.Null(user.ProfilePictureUrl);
  }

  [Fact]
  public void SetNotificationPreference_ShouldUpdatePreference()
  {
    // Arrange
    var user = AppUser.Create();
    var newPreference = new NotificationPreference(false, true, false);

    // Act
    user.SetNotificationPreference(newPreference);

    // Assert
    Assert.Equal(newPreference, user.NotificationPreference);
    Assert.False(user.NotificationPreference.IsInAppNotificationEnabled);
    Assert.True(user.NotificationPreference.IsEmailNotificationEnabled);
    Assert.False(user.NotificationPreference.IsPushNotificationEnabled);
  }

  [Fact]
  public void UpdatedUsers_ShouldReturnReadOnlyCollection()
  {
    // Arrange & Act
    var user = AppUser.Create();

    // Assert
    Assert.NotNull(user.UpdatedUsers);
    Assert.Empty(user.UpdatedUsers);
    Assert.IsAssignableFrom<IReadOnlyCollection<AppUser>>(user.UpdatedUsers);
  }

  [Fact]
  public void DeletedUsers_ShouldReturnReadOnlyCollection()
  {
    // Arrange & Act
    var user = AppUser.Create();

    // Assert
    Assert.NotNull(user.DeletedUsers);
    Assert.Empty(user.DeletedUsers);
    Assert.IsAssignableFrom<IReadOnlyCollection<AppUser>>(user.DeletedUsers);
  }

  [Fact]
  public void IdentityUser_ShouldBeInitiallyNull()
  {
    // Arrange & Act
    var user = AppUser.Create();

    // Assert
    Assert.Null(user.IdentityUser);
  }

  [Fact]
  public void ProfilePictureUrl_ShouldBeInitiallyNull()
  {
    // Arrange & Act
    var user = AppUser.Create();

    // Assert
    Assert.Null(user.ProfilePictureUrl);
  }

  [Fact]
  public void Biography_ShouldBeInitiallyNull()
  {
    // Arrange & Act
    var user = AppUser.Create();

    // Assert
    Assert.Null(user.Biography);
  }

  [Fact]
  public void AddRole_WithNewRole_ShouldAddRoleAndRaiseEvent()
  {
    // Arrange
    var user = AppUser.Create();
    var role1 = Role.Create("TestRole1", "Test Role 1", Guid.NewGuid());
    var role2 = Role.Create("TestRole2", "Test Role 2", Guid.NewGuid());

    // Act
    user.AddRole(role1);
    user.AddRole(role2);

    // Assert
    Assert.Equal(2, user.Roles.Count);
    Assert.Contains(role1, user.Roles);
    Assert.Contains(role2, user.Roles);

    // Verify domain events
    var addedEvents = user.GetDomainEvents().OfType<AppUserRoleAddedDomainEvent>().ToList();
    Assert.Equal(2, addedEvents.Count);
    Assert.Contains(addedEvents, e => e.RoleId == role1.Id);
    Assert.Contains(addedEvents, e => e.RoleId == role2.Id);
  }

  [Fact]
  public void RemoveRole_WithExistingRole_ShouldRemoveRoleAndRaiseEvent()
  {
    // Arrange
    var user = AppUser.Create();
    var role1 = Role.Create("TestRole1", "Test Role 1", Guid.NewGuid());
    var role2 = Role.Create("TestRole2", "Test Role 2", Guid.NewGuid());

    user.AddRole(role1);
    user.AddRole(role2);

    // Act
    user.RemoveRole(role1);

    // Assert
    Assert.Single(user.Roles);
    Assert.DoesNotContain(role1, user.Roles);
    Assert.Contains(role2, user.Roles);

    // Verify domain event
    var removedEvent = user.GetDomainEvents().OfType<AppUserRoleRemovedDomainEvent>().FirstOrDefault();
    Assert.NotNull(removedEvent);
    Assert.Equal(role1.Id, removedEvent.RoleId);
  }

  [Fact]
  public void UserChangedItsUsername_ShouldBeInitiallyFalse()
  {
    // Arrange & Act
    var user = AppUser.Create();

    // Assert
    Assert.False(user.UserChangedItsUsername);
  }

  [Fact]
  public void SetIdentityId_WithValidId_ShouldUpdateIdentityId()
  {
    // Arrange
    var user = AppUser.Create();
    var identityId = "user-identity-123";

    // Act
    user.SetIdentityId(identityId);

    // Assert
    Assert.Equal(identityId, user.IdentityId);
  }

  [Fact]
  public void SetIdentityId_WithEmptyString_ShouldSetEmptyString()
  {
    // Arrange
    var user = AppUser.Create();

    // Act
    user.SetIdentityId(string.Empty);

    // Assert
    Assert.Equal(string.Empty, user.IdentityId);
  }

  [Fact]
  public void Notifications_ShouldBeInitializedAsEmptyCollection()
  {
    // Arrange & Act
    var user = AppUser.Create();

    // Assert
    Assert.NotNull(user.Notifications);
    Assert.Empty(user.Notifications);
    Assert.IsAssignableFrom<ICollection<Notification>>(user.Notifications);
  }

  [Fact]
  public void AdminId_ShouldHaveCorrectValue()
  {
    // Arrange & Act
    var user = AppUser.Create();

    // Assert
    Assert.Equal(Guid.Parse("b3398ff2-1b43-4af7-812d-eb4347eecbb8"), user.AdminId);
  }

  [Fact]
  public void AuditFields_ShouldBeInitializedCorrectly()
  {
    // Arrange & Act
    var user = AppUser.Create();

    // Assert
    Assert.Null(user.CreatedById);
    Assert.Null(user.CreatedBy); // These are actually null, not NotNull
    Assert.Null(user.UpdatedById);
    Assert.Null(user.UpdatedBy); // These are actually null, not NotNull
    Assert.Null(user.DeletedById);
    Assert.Null(user.DeletedBy); // These are actually null, not NotNull
  }

  // Add these additional tests to increase coverage

  [Fact]
  public void AddRole_WithNewRole_ShouldAddToCollection()
  {
    // Arrange
    var user = AppUser.Create();
    var role = Role.Create("TestRole", "Test Role", Guid.NewGuid());
    var initialCount = user.Roles.Count;

    // Act
    user.AddRole(role);

    // Assert
    Assert.Equal(initialCount + 1, user.Roles.Count);
    Assert.Contains(role, user.Roles);
  }

  [Fact]
  public void AddRole_WithExistingRole_ShouldNotDuplicate()
  {
    // Arrange
    var user = AppUser.Create();
    var role = Role.Create("TestRole", "Test Role", Guid.NewGuid());

    // Add role first time
    user.AddRole(role);
    var countAfterFirstAdd = user.Roles.Count;

    // Act - Add same role again
    user.AddRole(role);

    // Assert
    Assert.Equal(countAfterFirstAdd, user.Roles.Count);
    Assert.Single(user.Roles.Where(r => r.Id == role.Id));
  }

  [Fact]
  public void AddRole_ShouldRaiseDomainEventOnlyOnce()
  {
    // Arrange
    var user = AppUser.Create();
    var role = Role.Create("TestRole", "Test Role", Guid.NewGuid());

    // Clear any existing events
    user.ClearDomainEvents();

    // Act
    user.AddRole(role);

    // Assert
    var domainEvents = user.GetDomainEvents().OfType<AppUserRoleAddedDomainEvent>().ToList();
    Assert.Single(domainEvents);
    Assert.Equal(user.Id, domainEvents.First().UserId);
    Assert.Equal(role.Id, domainEvents.First().RoleId);
  }

  [Fact]
  public void AddRole_WithDuplicateRole_ShouldNotRaiseDomainEvent()
  {
    // Arrange
    var user = AppUser.Create();
    var role = Role.Create("TestRole", "Test Role", Guid.NewGuid());

    // Add role first time
    user.AddRole(role);
    user.ClearDomainEvents(); // Clear events from first add

    // Act - Add same role again
    user.AddRole(role);

    // Assert
    var domainEvents = user.GetDomainEvents().OfType<AppUserRoleAddedDomainEvent>().ToList();
    Assert.Empty(domainEvents);
  }

  [Fact]
  public void RemoveRole_WithExistingRole_ShouldRemoveFromCollection()
  {
    // Arrange
    var user = AppUser.Create();
    var role = Role.Create("TestRole", "Test Role", Guid.NewGuid());
    user.AddRole(role);
    var initialCount = user.Roles.Count;

    // Act
    user.RemoveRole(role);

    // Assert
    Assert.Equal(initialCount - 1, user.Roles.Count);
    Assert.DoesNotContain(role, user.Roles);
  }

  [Fact]
  public void RemoveRole_WithNonExistentRole_ShouldNotChangeCollection()
  {
    // Arrange
    var user = AppUser.Create();
    var role1 = Role.Create("ExistingRole", "Existing Role", Guid.NewGuid());
    var role2 = Role.Create("NonExistentRole", "Non Existent Role", Guid.NewGuid());

    user.AddRole(role1);
    var initialCount = user.Roles.Count;

    // Act
    user.RemoveRole(role2);

    // Assert
    Assert.Equal(initialCount, user.Roles.Count);
    Assert.Contains(role1, user.Roles);
  }

  [Fact]
  public void RemoveRole_WithExistingRole_ShouldRaiseDomainEvent()
  {
    // Arrange
    var user = AppUser.Create();
    var role = Role.Create("TestRole", "Test Role", Guid.NewGuid());
    user.AddRole(role);
    user.ClearDomainEvents(); // Clear events from add

    // Act
    user.RemoveRole(role);

    // Assert
    var domainEvents = user.GetDomainEvents().OfType<AppUserRoleRemovedDomainEvent>().ToList();
    Assert.Single(domainEvents);
    Assert.Equal(user.Id, domainEvents.First().UserId);
    Assert.Equal(role.Id, domainEvents.First().RoleId);
  }

  [Fact]
  public void RemoveRole_WithNonExistentRole_ShouldNotRaiseDomainEvent()
  {
    // Arrange
    var user = AppUser.Create();
    var role = Role.Create("TestRole", "Test Role", Guid.NewGuid());
    user.ClearDomainEvents(); // Clear any initial events

    // Act
    user.RemoveRole(role);

    // Assert
    var domainEvents = user.GetDomainEvents().OfType<AppUserRoleRemovedDomainEvent>().ToList();
    Assert.Empty(domainEvents);
  }

  [Fact]
  public void MarkUsernameAsChanged_ShouldSetPropertyToTrue()
  {
    // Arrange
    var user = AppUser.Create();
    Assert.False(user.UserChangedItsUsername); // Initial state

    // Act
    user.MarkUsernameAsChanged();

    // Assert
    Assert.True(user.UserChangedItsUsername);
  }

  [Fact]
  public void MarkUsernameAsChanged_CalledMultipleTimes_ShouldRemainTrue()
  {
    // Arrange
    var user = AppUser.Create();

    // Act
    user.MarkUsernameAsChanged();
    user.MarkUsernameAsChanged(); // Call again

    // Assert
    Assert.True(user.UserChangedItsUsername);
  }

  [Fact]
  public void SetProfilePictureUrl_WithValidUrl_ShouldUpdateProperty()
  {
    // Arrange
    var user = AppUser.Create();
    var testUrl = "https://example.com/avatar.jpg";

    // Act
    user.SetProfilePictureUrl(testUrl);

    // Assert
    Assert.Equal(testUrl, user.ProfilePictureUrl);
  }

  [Fact]
  public void SetProfilePictureUrl_WithNullUrl_ShouldSetToNull()
  {
    // Arrange
    var user = AppUser.Create();
    user.SetProfilePictureUrl("https://example.com/old-avatar.jpg"); // Set initial value

    // Act
    user.SetProfilePictureUrl(null);

    // Assert
    Assert.Null(user.ProfilePictureUrl);
  }

  [Fact]
  public void SetProfilePictureUrl_WithEmptyString_ShouldSetToEmptyString()
  {
    // Arrange
    var user = AppUser.Create();

    // Act
    user.SetProfilePictureUrl(string.Empty);

    // Assert
    Assert.Equal(string.Empty, user.ProfilePictureUrl);
  }

  [Fact]
  public void SetNotificationPreference_WithNewPreference_ShouldReplaceExistingPreference()
  {
    // Arrange
    var user = AppUser.Create();
    var originalPreference = user.NotificationPreference;
    var newPreference = new NotificationPreference(false, true, false);

    // Act
    user.SetNotificationPreference(newPreference);

    // Assert
    Assert.Equal(newPreference, user.NotificationPreference);
    Assert.NotEqual(originalPreference, user.NotificationPreference);
    Assert.False(user.NotificationPreference.IsInAppNotificationEnabled);
    Assert.True(user.NotificationPreference.IsEmailNotificationEnabled);
    Assert.False(user.NotificationPreference.IsPushNotificationEnabled);
  }

  // Tests specifically designed to improve branch coverage

  [Theory]
  [InlineData(true, true, true)]
  [InlineData(false, false, false)]
  [InlineData(true, false, true)]
  [InlineData(false, true, false)]
  public void SetNotificationPreference_WithVariousSettings_ShouldUpdateCorrectly(bool inApp, bool email, bool push)
  {
    // Arrange
    var user = AppUser.Create();
    var preference = new NotificationPreference(inApp, email, push);

    // Act
    user.SetNotificationPreference(preference);

    // Assert
    Assert.Equal(inApp, user.NotificationPreference.IsInAppNotificationEnabled);
    Assert.Equal(email, user.NotificationPreference.IsEmailNotificationEnabled);
    Assert.Equal(push, user.NotificationPreference.IsPushNotificationEnabled);
  }

  [Fact]
  public void MultipleRoles_AddAndRemove_ShouldMaintainCorrectState()
  {
    // Arrange
    var user = AppUser.Create();
    var roles = new[]
    {
        Role.Create("Admin", "Administrator", Guid.NewGuid()),
        Role.Create("Moderator", "Moderator", Guid.NewGuid()),
        Role.Create("User", "Standard User", Guid.NewGuid())
    };

    // Act & Assert - Add all roles
    foreach (var role in roles)
    {
      user.AddRole(role);
      Assert.Contains(role, user.Roles);
    }
    Assert.Equal(3, user.Roles.Count);

    // Act & Assert - Remove middle role
    user.RemoveRole(roles[1]);
    Assert.Equal(2, user.Roles.Count);
    Assert.Contains(roles[0], user.Roles);
    Assert.DoesNotContain(roles[1], user.Roles);
    Assert.Contains(roles[2], user.Roles);

    // Act & Assert - Try to add duplicate
    user.AddRole(roles[0]);
    Assert.Equal(2, user.Roles.Count); // Should not increase

    // Act & Assert - Remove non-existent role
    var nonExistentRole = Role.Create("NonExistent", "Non Existent", Guid.NewGuid());
    user.RemoveRole(nonExistentRole);
    Assert.Equal(2, user.Roles.Count); // Should not change
  }

  // Additional constructor tests

  [Fact]
  public void AppUser_ParameterlessConstructor_ShouldInitializeCorrectly()
  {
    // This tests the private parameterless constructor
    // We can test this through CreateWithoutRolesForSeeding which uses it
    var user = AppUser.CreateWithoutRolesForSeeding();
    
    Assert.NotNull(user);
    Assert.Equal(Guid.Parse("55c7f429-0916-4d84-8b76-d45185d89aa7"), user.Id);
    Assert.NotNull(user.NotificationPreference);
  }

  [Fact]
  public void AppUser_CreateVsCreateWithoutRoles_ShouldHaveDifferentBehavior()
  {
    // Arrange & Act
    var normalUser = AppUser.Create();
    var seedingUser = AppUser.CreateWithoutRolesForSeeding();

    // Assert
    Assert.NotEqual(normalUser.Id, seedingUser.Id);
    Assert.Equal(Guid.Parse("55c7f429-0916-4d84-8b76-d45185d89aa7"), seedingUser.Id);
    
    // Normal user should have domain events, seeding user should not
    Assert.NotEmpty(normalUser.GetDomainEvents());
    Assert.Empty(seedingUser.GetDomainEvents());
  }
}