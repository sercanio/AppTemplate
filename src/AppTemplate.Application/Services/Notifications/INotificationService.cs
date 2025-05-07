using AppTemplate.Domain.Notifications;

namespace AppTemplate.Application.Services.Notifications;

public interface INotificationService
{
    Task SendNotificationAsync(string details);
    Task SendNotificationToUserAsync(string details, Guid userId);
    Task SaveNotificationAsync(string details, Guid userId);
    Task SendNotificationToUserGroupAsync(string details, string groupName);
    Task<List<Notification>> GetNotificationsByUserIdAsync(Guid userId);
    Task MarkNotificationsAsReadAsync(Guid UserId, CancellationToken cancellation);
}
