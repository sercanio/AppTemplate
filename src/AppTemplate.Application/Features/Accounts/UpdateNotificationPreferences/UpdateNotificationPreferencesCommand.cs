using AppTemplate.Application.Services.Messages;
using AppTemplate.Domain.AppUsers.ValueObjects;

namespace AppTemplate.Application.Features.Accounts.UpdateNotificationPreferences;

public sealed record UpdateNotificationPreferencesCommand(
    NotificationPreference NotificationPreference) : ICommand<UpdateNotificationPreferencesCommandResponse>;
