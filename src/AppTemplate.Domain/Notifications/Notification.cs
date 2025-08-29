using AppTemplate.Domain.AppUsers;
using AppTemplate.Domain.Notifications.Enums;
using AppTemplate.Core.Domain.Abstractions;

namespace AppTemplate.Domain.Notifications;

public class Notification : Entity<Guid>
{
  public string Title { get; private set; }
  public string Message { get; private set; }
  public NotificationTypeEnum Type { get; private set; }
  public bool IsRead { get; private set; }

  public Guid RecipientId { get; private set; }
  public AppUser Recipient { get; private set; }

  public void MarkAsRead() => IsRead = true;

  private Notification() { }

  public Notification(Guid recipientId, string title, string message, NotificationTypeEnum type)
  {
    RecipientId = recipientId;
    Title = title;
    Message = message;
    Type = type;
    IsRead = false;
  }
}
