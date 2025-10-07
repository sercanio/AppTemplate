using AppTemplate.Domain.AppUsers.ValueObjects;
using Xunit;

namespace AppTemplate.Domain.Tests.Unit.AppUsersTests;

[Trait("Category", "Unit")]
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

  [Theory]
  [InlineData(true, true, true)]
  [InlineData(false, false, false)]
  [InlineData(true, false, true)]
  [InlineData(false, true, false)]
  public void Constructor_WithVariousInputs_ShouldSetPropertiesCorrectly(bool inApp, bool email, bool push)
  {
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

  [Theory]
  [InlineData(true, true, true, false, false, false)]
  [InlineData(false, false, false, true, true, true)]
  [InlineData(true, false, true, false, true, false)]
  public void Update_WithDifferentValues_ShouldChangeAllProperties(
      bool initialInApp, bool initialEmail, bool initialPush,
      bool newInApp, bool newEmail, bool newPush)
  {
      // Arrange
      var pref = new NotificationPreference(initialInApp, initialEmail, initialPush);

      // Act
      pref.Update(newInApp, newEmail, newPush);

      // Assert
      Assert.Equal(newInApp, pref.IsInAppNotificationEnabled);
      Assert.Equal(newEmail, pref.IsEmailNotificationEnabled);
      Assert.Equal(newPush, pref.IsPushNotificationEnabled);
  }

  [Fact]
  public void ToString_ShouldReturnCorrectFormat()
  {
    var pref = new NotificationPreference(true, false, true);

    var result = pref.ToString();

    Assert.Equal("In-App: True, Email: False, Push: True", result);
  }

  [Theory]
  [InlineData(true, true, true, "In-App: True, Email: True, Push: True")]
  [InlineData(false, false, false, "In-App: False, Email: False, Push: False")]
  [InlineData(true, false, true, "In-App: True, Email: False, Push: True")]
  [InlineData(false, true, false, "In-App: False, Email: True, Push: False")]
  public void ToString_WithVariousValues_ShouldReturnCorrectFormat(bool inApp, bool email, bool push, string expected)
  {
      // Arrange
      var pref = new NotificationPreference(inApp, email, push);

      // Act
      var result = pref.ToString();

      // Assert
      Assert.Equal(expected, result);
  }

  [Fact]
  public void Equals_ShouldReturnTrueForSameValues()
  {
    var pref1 = new NotificationPreference(true, false, true);
    var pref2 = new NotificationPreference(true, false, true);

    Assert.Equal(pref1, pref2);
  }

  [Fact]
  public void Equals_WithSameValues_ShouldReturnTrue()
  {
      // Arrange
      var pref1 = new NotificationPreference(true, false, true);
      var pref2 = new NotificationPreference(true, false, true);

      // Act & Assert
      Assert.Equal(pref1, pref2);
      Assert.True(pref1.Equals(pref2));
  }

  [Fact]
  public void Equals_WithDifferentValues_ShouldReturnFalse()
  {
    // Arrange
    var pref1 = new NotificationPreference(true, false, true);
    var pref2 = new NotificationPreference(false, false, true);

    // Act & Assert
    Assert.NotEqual(pref1, pref2);
    Assert.False(pref1.Equals(pref2));
  }

  [Fact]
  public void Equals_WithNull_ShouldReturnFalse()
  {
    // Arrange
    var pref = new NotificationPreference(true, false, true);

    // Act & Assert
    Assert.False(pref.Equals(null));
  }

  [Fact]
  public void GetHashCode_WithSameValues_ShouldReturnSameHashCode()
  {
    // Arrange
    var pref1 = new NotificationPreference(true, false, true);
    var pref2 = new NotificationPreference(true, false, true);

    // Act & Assert
    Assert.Equal(pref1.GetHashCode(), pref2.GetHashCode());
  }

  [Fact]
  public void GetHashCode_WithDifferentValues_ShouldReturnDifferentHashCode()
  {
    // Arrange
    var pref1 = new NotificationPreference(true, false, true);
    var pref2 = new NotificationPreference(false, false, true);

    // Act & Assert
    Assert.NotEqual(pref1.GetHashCode(), pref2.GetHashCode());
  }

  [Fact]
  public void Update_ShouldMaintainObjectIdentity()
  {
    // Arrange
    var pref = new NotificationPreference(true, true, true);
    var originalHashCode = pref.GetHashCode();

    // Act
    pref.Update(false, false, false);

    // Assert
    Assert.NotEqual(originalHashCode, pref.GetHashCode());
  }
}
