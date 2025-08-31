using AppTemplate.Domain.Notifications.Enums;

namespace AppTemplate.Domain.Tests.Unit.NotificationsTests;

public class NotificationTypeEnumUnitTests
{
  [Fact]
  public void Enum_ShouldContainExpectedValues()
  {
    Assert.Equal(0, (int)NotificationTypeEnum.Like);
    Assert.Equal(1, (int)NotificationTypeEnum.Bookmark);
    Assert.Equal(2, (int)NotificationTypeEnum.Follow);
    Assert.Equal(3, (int)NotificationTypeEnum.System);
  }

  [Fact]
  public void CanAssignAndCompareEnumValues()
  {
    var type = NotificationTypeEnum.Follow;
    Assert.Equal(NotificationTypeEnum.Follow, type);
    Assert.NotEqual(NotificationTypeEnum.Like, type);
  }
}