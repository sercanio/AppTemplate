using AppTemplate.Domain.AppUsers.ValueObjects;

namespace AppTemplate.Application.Features.Accounts.UpdateNotificationPreferences;

public sealed record UpdateNotificationPreferencesCommandResponse
{
    public Guid UserId { get; init; }
    public NotificationPreference NotificationPreference { get; init; }

    public UpdateNotificationPreferencesCommandResponse(Guid userId, NotificationPreference notificationPreference)
    {
        UserId = userId;
        NotificationPreference = notificationPreference;
    }

    private UpdateNotificationPreferencesCommandResponse() { }
}