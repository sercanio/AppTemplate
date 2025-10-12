using AppTemplate.Domain.AppUsers.ValueObjects;

namespace AppTemplate.Domain.Tests.Unit.AppUsersTests;

[Trait("Category", "Unit")]
public class NotificationPreferenceUnitTests
{
  [Fact]
  public void Constructor_ShouldInitializePropertiesCorrectly()
  {
    // Arrange & Act
    var preference = new NotificationPreference(true, false, true);

    // Assert
    Assert.True(preference.IsInAppNotificationEnabled);
    Assert.False(preference.IsEmailNotificationEnabled);
    Assert.True(preference.IsPushNotificationEnabled);
  }

  [Theory]
  [InlineData(true, true, true)]
  [InlineData(false, false, false)]
  [InlineData(true, false, false)]
  [InlineData(false, true, false)]
  [InlineData(false, false, true)]
  [InlineData(true, true, false)]
  [InlineData(true, false, true)]
  [InlineData(false, true, true)]
  public void Constructor_WithVariousCombinations_ShouldInitializeCorrectly(
      bool isInApp, bool isEmail, bool isPush)
  {
    // Act
    var preference = new NotificationPreference(isInApp, isEmail, isPush);

    // Assert
    Assert.Equal(isInApp, preference.IsInAppNotificationEnabled);
    Assert.Equal(isEmail, preference.IsEmailNotificationEnabled);
    Assert.Equal(isPush, preference.IsPushNotificationEnabled);
  }

  [Fact]
  public void Update_ShouldUpdateAllProperties()
  {
    // Arrange
    var preference = new NotificationPreference(true, true, true);

    // Act
    preference.Update(false, false, false);

    // Assert
    Assert.False(preference.IsInAppNotificationEnabled);
    Assert.False(preference.IsEmailNotificationEnabled);
    Assert.False(preference.IsPushNotificationEnabled);
  }

  [Fact]
  public void Update_ShouldUpdateFromFalseToTrue()
  {
    // Arrange
    var preference = new NotificationPreference(false, false, false);

    // Act
    preference.Update(true, true, true);

    // Assert
    Assert.True(preference.IsInAppNotificationEnabled);
    Assert.True(preference.IsEmailNotificationEnabled);
    Assert.True(preference.IsPushNotificationEnabled);
  }

  [Theory]
  [InlineData(true, false, true)]
  [InlineData(false, true, false)]
  [InlineData(true, true, false)]
  [InlineData(false, false, true)]
  public void Update_WithVariousCombinations_ShouldUpdateCorrectly(
      bool isInApp, bool isEmail, bool isPush)
  {
    // Arrange
    var preference = new NotificationPreference(true, true, true);

    // Act
    preference.Update(isInApp, isEmail, isPush);

    // Assert
    Assert.Equal(isInApp, preference.IsInAppNotificationEnabled);
    Assert.Equal(isEmail, preference.IsEmailNotificationEnabled);
    Assert.Equal(isPush, preference.IsPushNotificationEnabled);
  }

  [Fact]
  public void Update_CalledMultipleTimes_ShouldUpdateCorrectly()
  {
    // Arrange
    var preference = new NotificationPreference(true, true, true);

    // Act & Assert - First update
    preference.Update(false, false, false);
    Assert.False(preference.IsInAppNotificationEnabled);
    Assert.False(preference.IsEmailNotificationEnabled);
    Assert.False(preference.IsPushNotificationEnabled);

    // Act & Assert - Second update
    preference.Update(true, false, true);
    Assert.True(preference.IsInAppNotificationEnabled);
    Assert.False(preference.IsEmailNotificationEnabled);
    Assert.True(preference.IsPushNotificationEnabled);

    // Act & Assert - Third update
    preference.Update(false, true, false);
    Assert.False(preference.IsInAppNotificationEnabled);
    Assert.True(preference.IsEmailNotificationEnabled);
    Assert.False(preference.IsPushNotificationEnabled);
  }

  [Fact]
  public void ToString_WithAllTrue_ShouldReturnCorrectFormat()
  {
    // Arrange
    var preference = new NotificationPreference(true, true, true);

    // Act
    var result = preference.ToString();

    // Assert
    Assert.Equal("In-App: True, Email: True, Push: True", result);
  }

  [Fact]
  public void ToString_WithAllFalse_ShouldReturnCorrectFormat()
  {
    // Arrange
    var preference = new NotificationPreference(false, false, false);

    // Act
    var result = preference.ToString();

    // Assert
    Assert.Equal("In-App: False, Email: False, Push: False", result);
  }

  [Theory]
  [InlineData(true, false, true, "In-App: True, Email: False, Push: True")]
  [InlineData(false, true, false, "In-App: False, Email: True, Push: False")]
  [InlineData(true, true, false, "In-App: True, Email: True, Push: False")]
  [InlineData(false, false, true, "In-App: False, Email: False, Push: True")]
  public void ToString_WithVariousCombinations_ShouldReturnCorrectFormat(
      bool isInApp, bool isEmail, bool isPush, string expected)
  {
    // Arrange
    var preference = new NotificationPreference(isInApp, isEmail, isPush);

    // Act
    var result = preference.ToString();

    // Assert
    Assert.Equal(expected, result);
  }

  [Fact]
  public void Equals_WithSameValues_ShouldReturnTrue()
  {
    // Arrange
    var preference1 = new NotificationPreference(true, false, true);
    var preference2 = new NotificationPreference(true, false, true);

    // Act
    var result = preference1.Equals(preference2);

    // Assert
    Assert.True(result);
  }

  [Fact]
  public void Equals_WithDifferentValues_ShouldReturnFalse()
  {
    // Arrange
    var preference1 = new NotificationPreference(true, false, true);
    var preference2 = new NotificationPreference(false, true, false);

    // Act
    var result = preference1.Equals(preference2);

    // Assert
    Assert.False(result);
  }

  [Fact]
  public void Equals_WithNull_ShouldReturnFalse()
  {
    // Arrange
    var preference = new NotificationPreference(true, true, true);

    // Act
    var result = preference.Equals(null);

    // Assert
    Assert.False(result);
  }

  [Fact]
  public void GetHashCode_WithSameValues_ShouldReturnSameHashCode()
  {
    // Arrange
    var preference1 = new NotificationPreference(true, false, true);
    var preference2 = new NotificationPreference(true, false, true);

    // Act
    var hash1 = preference1.GetHashCode();
    var hash2 = preference2.GetHashCode();

    // Assert
    Assert.Equal(hash1, hash2);
  }

  [Fact]
  public void GetHashCode_WithDifferentValues_ShouldReturnDifferentHashCode()
  {
    // Arrange
    var preference1 = new NotificationPreference(true, false, true);
    var preference2 = new NotificationPreference(false, true, false);

    // Act
    var hash1 = preference1.GetHashCode();
    var hash2 = preference2.GetHashCode();

    // Assert
    Assert.NotEqual(hash1, hash2);
  }

  [Fact]
  public void EqualityOperator_WithSameValues_ShouldReturnTrue()
  {
    // Arrange
    var preference1 = new NotificationPreference(true, false, true);
    var preference2 = new NotificationPreference(true, false, true);

    // Act
    var result = preference1 == preference2;

    // Assert
    Assert.True(result);
  }

  [Fact]
  public void InequalityOperator_WithDifferentValues_ShouldReturnTrue()
  {
    // Arrange
    var preference1 = new NotificationPreference(true, false, true);
    var preference2 = new NotificationPreference(false, true, false);

    // Act
    var result = preference1 != preference2;

    // Assert
    Assert.True(result);
  }

  [Fact]
  public void GetEqualityComponents_ShouldReturnAllProperties()
  {
    // Arrange
    var preference = new NotificationPreference(true, false, true);

    // Act - Access equality through Equals method which uses GetEqualityComponents
    var samePreference = new NotificationPreference(true, false, true);
    var differentPreference = new NotificationPreference(false, false, true);

    // Assert
    Assert.True(preference.Equals(samePreference));
    Assert.False(preference.Equals(differentPreference));
  }

  [Fact]
  public void GetEqualityComponents_WithAllCombinations_ShouldWorkCorrectly()
  {
    // Arrange & Act & Assert - Test all three properties affect equality
    var basePreference = new NotificationPreference(true, true, true);

    // Different InApp
    var diffInApp = new NotificationPreference(false, true, true);
    Assert.False(basePreference.Equals(diffInApp));

    // Different Email
    var diffEmail = new NotificationPreference(true, false, true);
    Assert.False(basePreference.Equals(diffEmail));

    // Different Push
    var diffPush = new NotificationPreference(true, true, false);
    Assert.False(basePreference.Equals(diffPush));

    // Same values
    var same = new NotificationPreference(true, true, true);
    Assert.True(basePreference.Equals(same));
  }

  [Fact]
  public void PropertyGetters_ShouldReturnCorrectValues()
  {
    // Arrange
    var preference = new NotificationPreference(true, false, true);

    // Act & Assert
    var isInApp = preference.IsInAppNotificationEnabled;
    var isEmail = preference.IsEmailNotificationEnabled;
    var isPush = preference.IsPushNotificationEnabled;

    Assert.True(isInApp);
    Assert.False(isEmail);
    Assert.True(isPush);
  }

  [Fact]
  public void Update_ThenAccessProperties_ShouldReturnUpdatedValues()
  {
    // Arrange
    var preference = new NotificationPreference(true, true, true);

    // Act
    preference.Update(false, true, false);

    // Assert
    Assert.False(preference.IsInAppNotificationEnabled);
    Assert.True(preference.IsEmailNotificationEnabled);
    Assert.False(preference.IsPushNotificationEnabled);
  }

  [Fact]
  public void ToString_AfterUpdate_ShouldReturnUpdatedValues()
  {
    // Arrange
    var preference = new NotificationPreference(true, true, true);

    // Act
    preference.Update(false, true, false);
    var result = preference.ToString();

    // Assert
    Assert.Equal("In-App: False, Email: True, Push: False", result);
  }

  [Fact]
  public void ValueObject_ShouldBeImmutableWithoutUpdate()
  {
    // Arrange
    var preference = new NotificationPreference(true, false, true);

    // Act - Access properties multiple times
    var value1 = preference.IsInAppNotificationEnabled;
    var value2 = preference.IsInAppNotificationEnabled;

    // Assert - Values should remain consistent
    Assert.Equal(value1, value2);
    Assert.True(value1);
  }

  [Fact]
  public void GetEqualityComponents_ThroughEqualsMethod_ShouldCoverAllBranches()
  {
    // Arrange
    var preference1 = new NotificationPreference(true, true, true);
    var preference2 = new NotificationPreference(true, true, true);
    var preference3 = new NotificationPreference(false, false, false);

    // Act & Assert - This will iterate through GetEqualityComponents
    Assert.True(preference1.Equals(preference2));
    Assert.False(preference1.Equals(preference3));

    // Also test with different single property changes
    var diffInApp = new NotificationPreference(false, true, true);
    var diffEmail = new NotificationPreference(true, false, true);
    var diffPush = new NotificationPreference(true, true, false);

    Assert.False(preference1.Equals(diffInApp));
    Assert.False(preference1.Equals(diffEmail));
    Assert.False(preference1.Equals(diffPush));
  }
}
