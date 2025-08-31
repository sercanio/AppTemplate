using AppTemplate.Domain.Notifications;
using AppTemplate.Domain.Notifications.Enums;

namespace AppTemplate.Domain.Tests.Unit.NotificationsTests;

public class NotificationsUnitTests
{
  [Fact]
  public void Constructor_ShouldSetPropertiesCorrectly()
  {
    // Arrange
    var recipientId = Guid.NewGuid();
    var title = "Test Title";
    var message = "Test Message";
    var type = NotificationTypeEnum.System;

    // Act
    var notification = new Notification(recipientId, title, message, type);

    // Assert
    Assert.Equal(recipientId, notification.RecipientId);
    Assert.Equal(title, notification.Title);
    Assert.Equal(message, notification.Message);
    Assert.Equal(type, notification.Type);
    Assert.False(notification.IsRead);
  }

  [Fact]
  public void MarkAsRead_ShouldSetIsReadToTrue()
  {
    var notification = new Notification(Guid.NewGuid(), "Title", "Message", NotificationTypeEnum.System);

    notification.MarkAsRead();

    Assert.True(notification.IsRead);
  }

  [Fact]
  public void Recipient_ShouldBeNullByDefault()
  {
    var notification = new Notification(Guid.NewGuid(), "Title", "Message", NotificationTypeEnum.System);

    Assert.Null(notification.Recipient);
  }
}
