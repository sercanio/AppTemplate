namespace AppTemplate.Domain.AppUsers.ValueObjects;

public class NotificationPreference(
    bool isInAppNotificationEnabled,
    bool isEmailNotificationEnabled,
    bool isPushNotificationEnabled) : ValueObject
{
  public bool IsInAppNotificationEnabled { get; private set; } = isInAppNotificationEnabled;
  public bool IsEmailNotificationEnabled { get; private set; } = isEmailNotificationEnabled;
  public bool IsPushNotificationEnabled { get; private set; } = isPushNotificationEnabled;

  public void Update(bool isInAppEnabled, bool isEmailEnabled, bool isPushNotificationEnabled)
  {
    IsInAppNotificationEnabled = isInAppEnabled;
    IsEmailNotificationEnabled = isEmailEnabled;
    IsPushNotificationEnabled = isPushNotificationEnabled;
  }

  public override string ToString()
  {
    return $"In-App: {IsInAppNotificationEnabled}, Email: {IsEmailNotificationEnabled}, Push: {IsPushNotificationEnabled}";
  }

  protected override IEnumerable<object?> GetEqualityComponents()
  {
    yield return IsInAppNotificationEnabled;
    yield return IsEmailNotificationEnabled;
    yield return IsPushNotificationEnabled;
  }
}
