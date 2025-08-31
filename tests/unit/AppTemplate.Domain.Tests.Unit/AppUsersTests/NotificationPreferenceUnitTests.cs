using AppTemplate.Domain.AppUsers.ValueObjects;
using Xunit;

namespace AppTemplate.Domain.Tests.Unit.AppUsersTests;

public class NotificationPreferenceUnitTests
{
  [Fact]
  public void Constructor_ShouldSetPropertiesCorrectly()
  {
    // Arrange
    var inApp = true;
    var email = false;
    var push = true;

    // Act
    var pref = new NotificationPreference(inApp, email, push);

    // Assert
    Assert.Equal(inApp, pref.IsInAppNotificationEnabled);
    Assert.Equal(email, pref.IsEmailNotificationEnabled);
    Assert.Equal(push, pref.IsPushNotificationEnabled);
  }

  [Fact]
  public void Update_ShouldChangeProperties()
  {
    // Arrange
    var pref = new NotificationPreference(true, true, true);

    // Act
    pref.Update(false, false, true);

    // Assert
    Assert.False(pref.IsInAppNotificationEnabled);
    Assert.False(pref.IsEmailNotificationEnabled);
    Assert.True(pref.IsPushNotificationEnabled);
  }

  [Fact]
  public void ToString_ShouldReturnCorrectFormat()
  {
    var pref = new NotificationPreference(true, false, true);

    var result = pref.ToString();

    Assert.Equal("In-App: True, Email: False, Push: True", result);
  }

  [Fact]
  public void Equals_ShouldReturnTrueForSameValues()
  {
    var pref1 = new NotificationPreference(true, false, true);
    var pref2 = new NotificationPreference(true, false, true);

    Assert.Equal(pref1, pref2);
  }
}
