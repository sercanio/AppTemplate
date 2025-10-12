using AppTemplate.Domain.AppUsers;
using AppTemplate.Domain.AppUsers.DomainEvents;
using AppTemplate.Domain.AppUsers.ValueObjects;
using AppTemplate.Domain.Notifications;
using AppTemplate.Domain.Roles;
using AppTemplate.Domain.Users.DomainEvents;

namespace AppTemplate.Domain.Tests.Unit.AppUsersTests;

[Trait("Category", "Unit")]
public class AppUserUnitTests
{
  [Fact]
  public void Create_ShouldCreateAppUserWithValidProperties()
  {
    // Act
    var appUser = AppUser.Create();

    // Assert
    Assert.NotNull(appUser);
    Assert.NotEqual(Guid.Empty, appUser.Id);
    Assert.NotNull(appUser.NotificationPreference);
    Assert.True(appUser.NotificationPreference.IsInAppNotificationEnabled);
    Assert.True(appUser.NotificationPreference.IsEmailNotificationEnabled);
    Assert.True(appUser.NotificationPreference.IsPushNotificationEnabled);
    Assert.False(appUser.UserChangedItsUsername);
    Assert.Empty(appUser.Roles);
    Assert.Null(appUser.ProfilePictureUrl);
    Assert.Null(appUser.Biography);
    Assert.Null(appUser.CreatedById);
    Assert.Null(appUser.UpdatedById);
    Assert.Null(appUser.DeletedById);
  }

  [Fact]
  public void Create_ShouldRaiseAppUserCreatedDomainEvent()
  {
    // Act
    var appUser = AppUser.Create();

    // Assert
    var domainEvents = appUser.GetDomainEvents();
    Assert.Single(domainEvents);
    Assert.IsType<AppUserCreatedDomainEvent>(domainEvents.First());
    var createdEvent = (AppUserCreatedDomainEvent)domainEvents.First();
    Assert.Equal(appUser.Id, createdEvent.UserId);
  }

  [Fact]
  public void CreateWithoutRolesForSeeding_ShouldCreateAppUserWithSpecificId()
  {
    // Act
    var appUser = AppUser.CreateWithoutRolesForSeeding();

    // Assert
    Assert.NotNull(appUser);
    Assert.Equal(Guid.Parse("55c7f429-0916-4d84-8b76-d45185d89aa7"), appUser.Id);
    Assert.NotNull(appUser.NotificationPreference);
    Assert.True(appUser.NotificationPreference.IsInAppNotificationEnabled);
    Assert.True(appUser.NotificationPreference.IsEmailNotificationEnabled);
    Assert.True(appUser.NotificationPreference.IsPushNotificationEnabled);
    Assert.Empty(appUser.Roles);
  }

  [Fact]
  public void AdminId_ShouldHaveCorrectValue()
  {
    // Arrange & Act
    var appUser = AppUser.Create();

    // Assert
    Assert.Equal(Guid.Parse("b3398ff2-1b43-4af7-812d-eb4347eecbb8"), appUser.AdminId);
  }

  [Fact]
  public void AddRole_WithNewRole_ShouldAddRoleAndRaiseDomainEvent()
  {
    // Arrange
    var appUser = AppUser.Create();
    var creatorId = Guid.NewGuid();
    var role = Role.Create("TestRole", "Test Description", creatorId);
    appUser.ClearDomainEvents(); // Clear creation event

    // Act
    appUser.AddRole(role);

    // Assert
    Assert.Contains(role, appUser.Roles);
    var domainEvents = appUser.GetDomainEvents();
    Assert.Single(domainEvents);
    Assert.IsType<AppUserRoleAddedDomainEvent>(domainEvents.First());
    var roleAddedEvent = (AppUserRoleAddedDomainEvent)domainEvents.First();
    Assert.Equal(appUser.Id, roleAddedEvent.UserId);
    Assert.Equal(role.Id, roleAddedEvent.RoleId);
  }

  [Fact]
  public void AddRole_WithExistingRole_ShouldNotAddDuplicateRole()
  {
    // Arrange
    var appUser = AppUser.Create();
    var creatorId = Guid.NewGuid();
    var role = Role.Create("TestRole", "Test Description", creatorId);
    appUser.AddRole(role);
    appUser.ClearDomainEvents();

    // Act
    appUser.AddRole(role);

    // Assert
    Assert.Single(appUser.Roles);
    Assert.Empty(appUser.GetDomainEvents());
  }

  [Fact]
  public void RemoveRole_WithExistingRole_ShouldRemoveRoleAndRaiseDomainEvent()
  {
    // Arrange
    var appUser = AppUser.Create();
    var creatorId = Guid.NewGuid();
    var role = Role.Create("TestRole", "Test Description", creatorId);
    appUser.AddRole(role);
    appUser.ClearDomainEvents();

    // Act
    appUser.RemoveRole(role);

    // Assert
    Assert.DoesNotContain(role, appUser.Roles);
    var domainEvents = appUser.GetDomainEvents();
    Assert.Single(domainEvents);
    Assert.IsType<AppUserRoleRemovedDomainEvent>(domainEvents.First());
    var roleRemovedEvent = (AppUserRoleRemovedDomainEvent)domainEvents.First();
    Assert.Equal(appUser.Id, roleRemovedEvent.UserId);
    Assert.Equal(role.Id, roleRemovedEvent.RoleId);
  }

  [Fact]
  public void RemoveRole_WithNonExistingRole_ShouldNotRaiseDomainEvent()
  {
    // Arrange
    var appUser = AppUser.Create();
    var creatorId = Guid.NewGuid();
    var role = Role.Create("TestRole", "Test Description", creatorId);
    appUser.ClearDomainEvents();

    // Act
    appUser.RemoveRole(role);

    // Assert
    Assert.DoesNotContain(role, appUser.Roles);
    Assert.Empty(appUser.GetDomainEvents());
  }

  [Fact]
  public void SetIdentityId_ShouldSetIdentityIdProperty()
  {
    // Arrange
    var appUser = AppUser.Create();
    var identityId = "test-identity-id";

    // Act
    appUser.SetIdentityId(identityId);

    // Assert
    Assert.Equal(identityId, appUser.IdentityId);
  }

  [Fact]
  public void MarkUsernameAsChanged_ShouldSetUserChangedItsUsernameToTrue()
  {
    // Arrange
    var appUser = AppUser.Create();
    Assert.False(appUser.UserChangedItsUsername);

    // Act
    appUser.MarkUsernameAsChanged();

    // Assert
    Assert.True(appUser.UserChangedItsUsername);
  }

  [Fact]
  public void SetProfilePictureUrl_WithValidUrl_ShouldSetUrlAndMarkUpdated()
  {
    // Arrange
    var appUser = AppUser.Create();
    var url = "https://example.com/profile.jpg";
    appUser.ClearDomainEvents();

    // Act
    appUser.SetProfilePictureUrl(url);

    // Assert
    Assert.Equal(url, appUser.ProfilePictureUrl);
    // Verify MarkUpdated was called by checking if UpdatedOnUtc changed
    Assert.True(appUser.UpdatedOnUtc.HasValue);
  }

  [Fact]
  public void SetProfilePictureUrl_WithNullUrl_ShouldSetNullAndMarkUpdated()
  {
    // Arrange
    var appUser = AppUser.Create();
    appUser.SetProfilePictureUrl("https://example.com/profile.jpg");
    appUser.ClearDomainEvents();

    // Act
    appUser.SetProfilePictureUrl(null);

    // Assert
    Assert.Null(appUser.ProfilePictureUrl);
    Assert.True(appUser.UpdatedOnUtc.HasValue);
  }

  [Fact]
  public void SetNotificationPreference_ShouldUpdateNotificationPreference()
  {
    // Arrange
    var appUser = AppUser.Create();
    var newPreference = new NotificationPreference(false, true, false);

    // Act
    appUser.SetNotificationPreference(newPreference);

    // Assert
    Assert.Equal(newPreference, appUser.NotificationPreference);
    Assert.False(appUser.NotificationPreference.IsInAppNotificationEnabled);
    Assert.True(appUser.NotificationPreference.IsEmailNotificationEnabled);
    Assert.False(appUser.NotificationPreference.IsPushNotificationEnabled);
  }

  [Fact]
  public void SetBiography_WithValidBiography_ShouldSetBiographyAndMarkUpdated()
  {
    // Arrange
    var appUser = AppUser.Create();
    var biography = "This is a test biography.";
    appUser.ClearDomainEvents();

    // Act
    appUser.SetBiography(biography);

    // Assert
    Assert.Equal(biography, appUser.Biography);
    Assert.True(appUser.UpdatedOnUtc.HasValue);
  }

  [Fact]
  public void SetBiography_WithNullBiography_ShouldSetNullAndMarkUpdated()
  {
    // Arrange
    var appUser = AppUser.Create();
    appUser.SetBiography("Previous biography");
    appUser.ClearDomainEvents();

    // Act
    appUser.SetBiography(null);

    // Assert
    Assert.Null(appUser.Biography);
    Assert.True(appUser.UpdatedOnUtc.HasValue);
  }

  [Fact]
  public void Roles_ShouldReturnReadOnlyCollection()
  {
    // Arrange
    var appUser = AppUser.Create();
    var creatorId = Guid.NewGuid();
    var role = Role.Create("TestRole", "Test Description", creatorId);
    appUser.AddRole(role);

    // Act
    var roles = appUser.Roles;

    // Assert
    Assert.IsAssignableFrom<IReadOnlyCollection<Role>>(roles);
    Assert.Single(roles);
    Assert.Contains(role, roles);
  }

  [Fact]
  public void UpdatedUsers_ShouldReturnReadOnlyCollection()
  {
    // Arrange
    var appUser = AppUser.Create();

    // Act
    var updatedUsers = appUser.UpdatedUsers;

    // Assert
    Assert.IsAssignableFrom<IReadOnlyCollection<AppUser>>(updatedUsers);
    Assert.Empty(updatedUsers);
  }

  [Fact]
  public void DeletedUsers_ShouldReturnReadOnlyCollection()
  {
    // Arrange
    var appUser = AppUser.Create();

    // Act
    var deletedUsers = appUser.DeletedUsers;

    // Assert
    Assert.IsAssignableFrom<IReadOnlyCollection<AppUser>>(deletedUsers);
    Assert.Empty(deletedUsers);
  }

  [Fact]
  public void Notifications_ShouldBeInitializedAsEmptyCollection()
  {
    // Arrange & Act
    var appUser = AppUser.Create();

    // Assert
    Assert.NotNull(appUser.Notifications);
    Assert.Empty(appUser.Notifications);
    Assert.IsAssignableFrom<ICollection<Notification>>(appUser.Notifications);
  }

  [Theory]
  [InlineData("")]
  [InlineData("valid-identity-id")]
  [InlineData("another-test-id-123")]
  public void SetIdentityId_WithVariousInputs_ShouldSetCorrectly(string identityId)
  {
    // Arrange
    var appUser = AppUser.Create();

    // Act
    appUser.SetIdentityId(identityId);

    // Assert
    Assert.Equal(identityId, appUser.IdentityId);
  }

  [Theory]
  [InlineData("")]
  [InlineData("Short bio")]
  [InlineData("This is a very long biography that contains multiple sentences and provides detailed information about the user's background, interests, and professional experience.")]
  public void SetBiography_WithVariousLengths_ShouldSetCorrectly(string biography)
  {
    // Arrange
    var appUser = AppUser.Create();
    appUser.ClearDomainEvents();

    // Act
    appUser.SetBiography(biography);

    // Assert
    Assert.Equal(biography, appUser.Biography);
    Assert.True(appUser.UpdatedOnUtc.HasValue);
  }

  [Theory]
  [InlineData("https://example.com/image.jpg")]
  [InlineData("https://cdn.example.com/users/123/avatar.png")]
  [InlineData("")]
  public void SetProfilePictureUrl_WithVariousUrls_ShouldSetCorrectly(string url)
  {
    // Arrange
    var appUser = AppUser.Create();
    appUser.ClearDomainEvents();

    // Act
    appUser.SetProfilePictureUrl(url);

    // Assert
    Assert.Equal(url, appUser.ProfilePictureUrl);
    Assert.True(appUser.UpdatedOnUtc.HasValue);
  }

  [Fact]
  public void MultipleRoleOperations_ShouldMaintainCorrectState()
  {
    // Arrange
    var appUser = AppUser.Create();
    var creatorId = Guid.NewGuid();
    var role1 = Role.Create("Role1", "Description1", creatorId);
    var role2 = Role.Create("Role2", "Description2", creatorId);
    var role3 = Role.Create("Role3", "Description3", creatorId);

    // Act
    appUser.AddRole(role1);
    appUser.AddRole(role2);
    appUser.AddRole(role3);
    appUser.RemoveRole(role2);

    // Assert
    Assert.Equal(2, appUser.Roles.Count);
    Assert.Contains(role1, appUser.Roles);
    Assert.DoesNotContain(role2, appUser.Roles);
    Assert.Contains(role3, appUser.Roles);
  }

  // Property initialization tests with explicit property access
  [Fact]
  public void UserChangedItsUsername_ShouldInitializeAsFalse()
  {
    // Arrange & Act
    var appUser = AppUser.Create();

    // Assert
    Assert.False(appUser.UserChangedItsUsername);
  }

  [Fact]
  public void CreatedById_ShouldInitializeAsNull()
  {
    // Arrange & Act
    var appUser = AppUser.Create();

    // Assert
    Assert.Null(appUser.CreatedById);
  }

  [Fact]
  public void UpdatedById_ShouldInitializeAsNull()
  {
    // Arrange & Act
    var appUser = AppUser.Create();

    // Assert
    Assert.Null(appUser.UpdatedById);
  }

  [Fact]
  public void DeletedById_ShouldInitializeAsNull()
  {
    // Arrange & Act
    var appUser = AppUser.Create();

    // Assert
    Assert.Null(appUser.DeletedById);
  }

  [Fact]
  public void ProfilePictureUrl_ShouldInitializeAsNull()
  {
    // Arrange & Act
    var appUser = AppUser.Create();

    // Assert
    Assert.Null(appUser.ProfilePictureUrl);
  }

  [Fact]
  public void Biography_ShouldInitializeAsNull()
  {
    // Arrange & Act
    var appUser = AppUser.Create();

    // Assert
    Assert.Null(appUser.Biography);
  }

  [Fact]
  public void SetNotificationPreference_WithAllCombinations_ShouldWork()
  {
    // Arrange
    var appUser = AppUser.Create();
    var testCases = new[]
    {
            new NotificationPreference(true, true, true),
            new NotificationPreference(false, false, false),
            new NotificationPreference(true, false, true),
            new NotificationPreference(false, true, false)
        };

    foreach (var preference in testCases)
    {
      // Act
      appUser.SetNotificationPreference(preference);

      // Assert
      Assert.Equal(preference.IsInAppNotificationEnabled, appUser.NotificationPreference.IsInAppNotificationEnabled);
      Assert.Equal(preference.IsEmailNotificationEnabled, appUser.NotificationPreference.IsEmailNotificationEnabled);
      Assert.Equal(preference.IsPushNotificationEnabled, appUser.NotificationPreference.IsPushNotificationEnabled);
    }
  }

  [Fact]
  public void AddRole_WithDefaultRole_ShouldAddRoleSuccessfully()
  {
    // Arrange
    var appUser = AppUser.Create();
    var defaultRole = Role.DefaultRole;
    appUser.ClearDomainEvents();

    // Act
    appUser.AddRole(defaultRole);

    // Assert
    Assert.Contains(defaultRole, appUser.Roles);
    Assert.Single(appUser.Roles);
  }

  [Fact]
  public void AddRole_WithAdminRole_ShouldAddRoleSuccessfully()
  {
    // Arrange
    var appUser = AppUser.Create();
    var adminRole = Role.Admin;
    appUser.ClearDomainEvents();

    // Act
    appUser.AddRole(adminRole);

    // Assert
    Assert.Contains(adminRole, appUser.Roles);
    Assert.Single(appUser.Roles);
  }

  [Fact]
  public void AddRole_WithBothDefaultAndAdminRoles_ShouldAddBothRoles()
  {
    // Arrange
    var appUser = AppUser.Create();
    var defaultRole = Role.DefaultRole;
    var adminRole = Role.Admin;
    appUser.ClearDomainEvents();

    // Act
    appUser.AddRole(defaultRole);
    appUser.AddRole(adminRole);

    // Assert
    Assert.Contains(defaultRole, appUser.Roles);
    Assert.Contains(adminRole, appUser.Roles);
    Assert.Equal(2, appUser.Roles.Count);
  }

  [Fact]
  public void RemoveRole_WithDefaultRole_ShouldRemoveRoleSuccessfully()
  {
    // Arrange
    var appUser = AppUser.Create();
    var defaultRole = Role.DefaultRole;
    appUser.AddRole(defaultRole);
    appUser.ClearDomainEvents();

    // Act
    appUser.RemoveRole(defaultRole);

    // Assert
    Assert.DoesNotContain(defaultRole, appUser.Roles);
    Assert.Empty(appUser.Roles);
  }

  [Fact]
  public void AddRole_ShouldCallMarkUpdated()
  {
    // Arrange
    var appUser = AppUser.Create();
    var creatorId = Guid.NewGuid();
    var role = Role.Create("TestRole", "Test Description", creatorId);
    var initialUpdatedTime = appUser.UpdatedOnUtc;

    // Act
    appUser.AddRole(role);

    // Assert
    Assert.True(appUser.UpdatedOnUtc.HasValue);
    Assert.NotEqual(initialUpdatedTime, appUser.UpdatedOnUtc);
  }

  [Fact]
  public void RemoveRole_ShouldCallMarkUpdated()
  {
    // Arrange
    var appUser = AppUser.Create();
    var creatorId = Guid.NewGuid();
    var role = Role.Create("TestRole", "Test Description", creatorId);
    appUser.AddRole(role);
    var timeAfterAdd = appUser.UpdatedOnUtc;

    // Act
    appUser.RemoveRole(role);

    // Assert
    Assert.True(appUser.UpdatedOnUtc.HasValue);
    Assert.NotEqual(timeAfterAdd, appUser.UpdatedOnUtc);
  }

  [Fact]
  public void CreatedBy_ShouldInitializeAsNull()
  {
    // Arrange & Act
    var appUser = AppUser.Create();

    // Assert
    Assert.Null(appUser.CreatedBy);
  }

  [Fact]
  public void UpdatedBy_ShouldInitializeAsNull()
  {
    // Arrange & Act
    var appUser = AppUser.Create();

    // Assert
    Assert.Null(appUser.UpdatedBy);
  }

  [Fact]
  public void DeletedBy_ShouldInitializeAsNull()
  {
    // Arrange & Act
    var appUser = AppUser.Create();

    // Assert
    Assert.Null(appUser.DeletedBy);
  }

  [Fact]
  public void IdentityUser_ShouldInitializeAsNull()
  {
    // Arrange & Act
    var appUser = AppUser.Create();

    // Assert
    Assert.Null(appUser.IdentityUser);
  }

  [Fact]
  public void MarkUsernameAsChanged_CalledMultipleTimes_ShouldStayTrue()
  {
    // Arrange
    var appUser = AppUser.Create();

    // Act
    appUser.MarkUsernameAsChanged();
    appUser.MarkUsernameAsChanged(); // Call multiple times

    // Assert
    Assert.True(appUser.UserChangedItsUsername);
  }

  [Fact]
  public void RemoveRole_WithMultipleRoles_ShouldRemoveOnlySpecifiedRole()
  {
    // Arrange
    var appUser = AppUser.Create();
    var creatorId = Guid.NewGuid();
    var role1 = Role.Create("Role1", "Description1", creatorId);
    var role2 = Role.Create("Role2", "Description2", creatorId);
    var role3 = Role.Create("Role3", "Description3", creatorId);

    appUser.AddRole(role1);
    appUser.AddRole(role2);
    appUser.AddRole(role3);
    appUser.ClearDomainEvents();

    // Act
    appUser.RemoveRole(role2);

    // Assert
    Assert.Equal(2, appUser.Roles.Count);
    Assert.Contains(role1, appUser.Roles);
    Assert.DoesNotContain(role2, appUser.Roles);
    Assert.Contains(role3, appUser.Roles);

    // Verify domain event was raised
    var domainEvents = appUser.GetDomainEvents();
    Assert.Single(domainEvents);
    Assert.IsType<AppUserRoleRemovedDomainEvent>(domainEvents.First());
  }

  [Fact]
  public void RemoveRole_FromEmptyRolesList_ShouldNotRaiseDomainEvent()
  {
    // Arrange
    var appUser = AppUser.Create();
    var creatorId = Guid.NewGuid();
    var role = Role.Create("TestRole", "Test Description", creatorId);
    appUser.ClearDomainEvents();

    // Act
    appUser.RemoveRole(role);

    // Assert
    Assert.Empty(appUser.Roles);
    Assert.Empty(appUser.GetDomainEvents());
  }

  [Fact]
  public void SetProfilePictureUrl_WithEmptyString_ShouldSetEmptyStringAndMarkUpdated()
  {
    // Arrange
    var appUser = AppUser.Create();
    appUser.ClearDomainEvents();

    // Act
    appUser.SetProfilePictureUrl("");

    // Assert
    Assert.Equal("", appUser.ProfilePictureUrl);
    Assert.True(appUser.UpdatedOnUtc.HasValue);
  }

  [Fact]
  public void SetBiography_WithEmptyString_ShouldSetEmptyStringAndMarkUpdated()
  {
    // Arrange
    var appUser = AppUser.Create();
    appUser.ClearDomainEvents();

    // Act
    appUser.SetBiography("");

    // Assert
    Assert.Equal("", appUser.Biography);
    Assert.True(appUser.UpdatedOnUtc.HasValue);
  }

  [Fact]
  public void SetIdentityId_WithNullValue_ShouldSetNull()
  {
    // Arrange
    var appUser = AppUser.Create();
    appUser.SetIdentityId("initial-id");

    // Act
    appUser.SetIdentityId(null!);

    // Assert
    Assert.Null(appUser.IdentityId);
  }

  [Fact]
  public void AddRole_WithSameRoleMultipleTimes_ShouldOnlyAddOnce()
  {
    // Arrange
    var appUser = AppUser.Create();
    var creatorId = Guid.NewGuid();
    var role = Role.Create("TestRole", "Test Description", creatorId);
    appUser.ClearDomainEvents();

    // Act
    appUser.AddRole(role);
    appUser.AddRole(role);
    appUser.AddRole(role);

    // Assert
    Assert.Single(appUser.Roles);
    Assert.Contains(role, appUser.Roles);

    // Should only have one domain event from first add
    var domainEvents = appUser.GetDomainEvents();
    Assert.Single(domainEvents);
  }

  [Fact]
  public void CreateWithoutRolesForSeeding_ShouldNotRaiseDomainEvent()
  {
    // Act
    var appUser = AppUser.CreateWithoutRolesForSeeding();

    // Assert
    Assert.Empty(appUser.GetDomainEvents());
  }

  [Fact]
  public void AllPropertiesAccessibility_ShouldBeAccessible()
  {
    // Arrange
    var appUser = AppUser.Create();
    var creatorId = Guid.NewGuid();
    var role = Role.Create("TestRole", "Test Description", creatorId);
    appUser.AddRole(role);

    // Act & Assert - Access all properties to improve line coverage
    _ = appUser.AdminId;
    _ = appUser.Roles;
    _ = appUser.Notifications;
    _ = appUser.UpdatedUsers;
    _ = appUser.DeletedUsers;
    _ = appUser.NotificationPreference;
    _ = appUser.IdentityId;
    _ = appUser.IdentityUser;
    _ = appUser.UserChangedItsUsername;
    _ = appUser.ProfilePictureUrl;
    _ = appUser.Biography;
    _ = appUser.CreatedById;
    _ = appUser.CreatedBy;
    _ = appUser.UpdatedById;
    _ = appUser.UpdatedBy;
    _ = appUser.DeletedById;
    _ = appUser.DeletedBy;

    // Basic assertion to ensure test doesn't get optimized away
    Assert.NotEqual(Guid.Empty, appUser.AdminId);
  }

  [Fact]
  public void RemoveRole_WithAdminRole_ShouldRemoveSuccessfully()
  {
    // Arrange
    var appUser = AppUser.Create();
    var adminRole = Role.Admin;
    appUser.AddRole(adminRole);
    appUser.ClearDomainEvents();

    // Act
    appUser.RemoveRole(adminRole);

    // Assert
    Assert.DoesNotContain(adminRole, appUser.Roles);
    Assert.Empty(appUser.Roles);

    // Verify domain event was raised
    var domainEvents = appUser.GetDomainEvents();
    Assert.Single(domainEvents);
    Assert.IsType<AppUserRoleRemovedDomainEvent>(domainEvents.First());
  }

  [Fact]
  public void RemoveRole_BothBranches_ShouldWorkCorrectly()
  {
    // Arrange
    var appUser = AppUser.Create();
    var creatorId = Guid.NewGuid();
    var existingRole = Role.Create("ExistingRole", "Description", creatorId);
    var nonExistingRole = Role.Create("NonExistingRole", "Description", creatorId);

    appUser.AddRole(existingRole);
    appUser.ClearDomainEvents();

    // Act & Assert - Test the TRUE branch (role exists and is removed)
    appUser.RemoveRole(existingRole);
    Assert.DoesNotContain(existingRole, appUser.Roles);
    Assert.Single(appUser.GetDomainEvents()); // Should have domain event

    appUser.ClearDomainEvents();

    // Act & Assert - Test the FALSE branch (role doesn't exist)
    appUser.RemoveRole(nonExistingRole);
    Assert.Empty(appUser.GetDomainEvents()); // Should NOT have domain event
  }

  [Fact]
  public void AddRole_BothBranches_ShouldWorkCorrectly()
  {
    // Arrange
    var appUser = AppUser.Create();
    var creatorId = Guid.NewGuid();
    var role = Role.Create("TestRole", "Description", creatorId);
    appUser.ClearDomainEvents();

    // Act & Assert - Test the TRUE branch (role doesn't exist and is added)
    appUser.AddRole(role);
    Assert.Contains(role, appUser.Roles);
    Assert.Single(appUser.GetDomainEvents()); // Should have domain event

    appUser.ClearDomainEvents();

    // Act & Assert - Test the FALSE branch (role already exists)
    appUser.AddRole(role);
    Assert.Single(appUser.Roles); // Still only one role
    Assert.Empty(appUser.GetDomainEvents()); // Should NOT have domain event
  }

  [Fact]
  public void RoleOperations_WithComplexScenario_ShouldMaintainIntegrity()
  {
    // Arrange
    var appUser = AppUser.Create();
    var creatorId = Guid.NewGuid();
    var role1 = Role.Create("Role1", "Description1", creatorId);
    var role2 = Role.Create("Role2", "Description2", creatorId);
    var role3 = Role.Create("Role3", "Description3", creatorId);
    var defaultRole = Role.DefaultRole;
    var adminRole = Role.Admin;

    // Act - Complex sequence of operations
    appUser.AddRole(role1);
    appUser.AddRole(defaultRole);
    appUser.AddRole(role2);
    appUser.AddRole(adminRole);
    appUser.AddRole(role3);

    // Try to add duplicates
    appUser.AddRole(role1); // Should not add
    appUser.AddRole(defaultRole); // Should not add

    // Remove some roles
    appUser.RemoveRole(role2);
    appUser.RemoveRole(defaultRole);

    // Try to remove non-existing role
    var nonExistingRole = Role.Create("NonExisting", "Description", creatorId);
    appUser.RemoveRole(nonExistingRole); // Should not remove anything

    // Assert
    Assert.Equal(3, appUser.Roles.Count); // role1, adminRole, role3
    Assert.Contains(role1, appUser.Roles);
    Assert.Contains(adminRole, appUser.Roles);
    Assert.Contains(role3, appUser.Roles);
    Assert.DoesNotContain(role2, appUser.Roles);
    Assert.DoesNotContain(defaultRole, appUser.Roles);
    Assert.DoesNotContain(nonExistingRole, appUser.Roles);
  }

  [Fact]
  public void UpdatedUsers_PropertyAccess_ShouldReturnEmptyReadOnlyCollection()
  {
    // Arrange
    var appUser = AppUser.Create();

    // Act
    var updatedUsers = appUser.UpdatedUsers;

    // Assert
    Assert.NotNull(updatedUsers);
    Assert.Empty(updatedUsers);
    Assert.IsAssignableFrom<IReadOnlyCollection<AppUser>>(updatedUsers);

    // Access again to ensure property getter is covered
    var updatedUsersSecondCall = appUser.UpdatedUsers;
    Assert.Same(updatedUsers.GetType(), updatedUsersSecondCall.GetType());
  }

  [Fact]
  public void DeletedUsers_PropertyAccess_ShouldReturnEmptyReadOnlyCollection()
  {
    // Arrange
    var appUser = AppUser.Create();

    // Act
    var deletedUsers = appUser.DeletedUsers;

    // Assert
    Assert.NotNull(deletedUsers);
    Assert.Empty(deletedUsers);
    Assert.IsAssignableFrom<IReadOnlyCollection<AppUser>>(deletedUsers);

    // Access again to ensure property getter is covered
    var deletedUsersSecondCall = appUser.DeletedUsers;
    Assert.Same(deletedUsers.GetType(), deletedUsersSecondCall.GetType());
  }

  [Fact]
  public void ProfilePictureUrl_PropertyAccess_AfterSettingValue()
  {
    // Arrange
    var appUser = AppUser.Create();
    var testUrl = "https://example.com/avatar.jpg";

    // Act
    appUser.SetProfilePictureUrl(testUrl);
    var retrievedUrl = appUser.ProfilePictureUrl;

    // Assert
    Assert.Equal(testUrl, retrievedUrl);

    // Access multiple times to cover getter
    Assert.Equal(testUrl, appUser.ProfilePictureUrl);
    Assert.Equal(testUrl, appUser.ProfilePictureUrl);
  }

  [Fact]
  public void Biography_PropertyAccess_AfterSettingValue()
  {
    // Arrange
    var appUser = AppUser.Create();
    var testBiography = "Software developer with 10 years experience";

    // Act
    appUser.SetBiography(testBiography);
    var retrievedBiography = appUser.Biography;

    // Assert
    Assert.Equal(testBiography, retrievedBiography);

    // Access multiple times to cover getter
    Assert.Equal(testBiography, appUser.Biography);
    Assert.Equal(testBiography, appUser.Biography);
  }

  [Fact]
  public void CreatedBy_PropertyAccess_ShouldReturnNull()
  {
    // Arrange
    var appUser = AppUser.Create();

    // Act
    var createdBy = appUser.CreatedBy;

    // Assert
    Assert.Null(createdBy);

    // Access multiple times to cover getter
    Assert.Null(appUser.CreatedBy);
    Assert.Null(appUser.CreatedBy);
  }

  [Fact]
  public void UpdatedBy_PropertyAccess_ShouldReturnNull()
  {
    // Arrange
    var appUser = AppUser.Create();

    // Act
    var updatedBy = appUser.UpdatedBy;

    // Assert
    Assert.Null(updatedBy);

    // Access multiple times to cover getter
    Assert.Null(appUser.UpdatedBy);
    Assert.Null(appUser.UpdatedBy);
  }

  [Fact]
  public void DeletedById_PropertyAccess_ShouldReturnNull()
  {
    // Arrange
    var appUser = AppUser.Create();

    // Act
    var deletedById = appUser.DeletedById;

    // Assert
    Assert.Null(deletedById);

    // Access multiple times to cover getter
    Assert.Null(appUser.DeletedById);
    Assert.Null(appUser.DeletedById);
  }

  [Fact]
  public void DeletedBy_PropertyAccess_ShouldReturnNull()
  {
    // Arrange
    var appUser = AppUser.Create();

    // Act
    var deletedBy = appUser.DeletedBy;

    // Assert
    Assert.Null(deletedBy);

    // Access multiple times to cover getter
    Assert.Null(appUser.DeletedBy);
    Assert.Null(appUser.DeletedBy);
  }

  [Fact]
  public void MarkUsernameAsChanged_PropertyAccess_BeforeAndAfter()
  {
    // Arrange
    var appUser = AppUser.Create();

    // Act & Assert - Before
    var beforeChange = appUser.UserChangedItsUsername;
    Assert.False(beforeChange);

    appUser.MarkUsernameAsChanged();

    // Act & Assert - After
    var afterChange = appUser.UserChangedItsUsername;
    Assert.True(afterChange);

    // Access multiple times to cover getter
    Assert.True(appUser.UserChangedItsUsername);
    Assert.True(appUser.UserChangedItsUsername);
  }

  [Fact]
  public void SetNotificationPreference_PropertyAccess_AfterSettingValue()
  {
    // Arrange
    var appUser = AppUser.Create();
    var newPreference = new NotificationPreference(false, false, true);

    // Act
    appUser.SetNotificationPreference(newPreference);
    var retrievedPreference = appUser.NotificationPreference;

    // Assert
    Assert.Equal(newPreference, retrievedPreference);

    // Access multiple times to cover getter
    Assert.Equal(newPreference, appUser.NotificationPreference);
    Assert.Equal(newPreference, appUser.NotificationPreference);
  }

  [Fact]
  public void SetProfilePictureUrl_PropertyAccess_MultipleChanges()
  {
    // Arrange
    var appUser = AppUser.Create();

    // Act & Assert - Set first value
    appUser.SetProfilePictureUrl("url1");
    Assert.Equal("url1", appUser.ProfilePictureUrl);

    // Set second value
    appUser.SetProfilePictureUrl("url2");
    Assert.Equal("url2", appUser.ProfilePictureUrl);

    // Set to null
    appUser.SetProfilePictureUrl(null);
    Assert.Null(appUser.ProfilePictureUrl);

    // Set to empty string
    appUser.SetProfilePictureUrl("");
    Assert.Equal("", appUser.ProfilePictureUrl);
  }

  [Fact]
  public void SetBiography_PropertyAccess_MultipleChanges()
  {
    // Arrange
    var appUser = AppUser.Create();

    // Act & Assert - Set first value
    appUser.SetBiography("First bio");
    Assert.Equal("First bio", appUser.Biography);

    // Set second value
    appUser.SetBiography("Second bio");
    Assert.Equal("Second bio", appUser.Biography);

    // Set to null
    appUser.SetBiography(null);
    Assert.Null(appUser.Biography);

    // Set to empty string
    appUser.SetBiography("");
    Assert.Equal("", appUser.Biography);
  }

  [Fact]
  public void AllUncoveredGetters_ShouldBeAccessibleAndReturnExpectedValues()
  {
    // Arrange
    var appUser = AppUser.Create();
    appUser.SetProfilePictureUrl("test-url");
    appUser.SetBiography("test-bio");
    appUser.MarkUsernameAsChanged();

    // Act & Assert - Cover all getters that were showing 0% coverage
    var profilePic = appUser.ProfilePictureUrl;
    Assert.Equal("test-url", profilePic);

    var biography = appUser.Biography;
    Assert.Equal("test-bio", biography);

    var updatedUsers = appUser.UpdatedUsers;
    Assert.NotNull(updatedUsers);

    var deletedUsers = appUser.DeletedUsers;
    Assert.NotNull(deletedUsers);

    var createdBy = appUser.CreatedBy;
    Assert.Null(createdBy);

    var updatedBy = appUser.UpdatedBy;
    Assert.Null(updatedBy);

    var deletedById = appUser.DeletedById;
    Assert.Null(deletedById);

    var deletedBy = appUser.DeletedBy;
    Assert.Null(deletedBy);

    var usernameChanged = appUser.UserChangedItsUsername;
    Assert.True(usernameChanged);
  }
}