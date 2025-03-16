using EcoFind.Domain.AppUsers.ValueObjects;
using Myrtus.Clarity.Core.Application.Abstractions.Messaging;

namespace EcoFind.Application.Features.Accounts.UpdateNotificationPreferences;

public sealed record UpdateNotificationPreferencesCommand(
    NotificationPreference NotificationPreference) : ICommand<UpdateNotificationPreferencesCommandResponse>;
