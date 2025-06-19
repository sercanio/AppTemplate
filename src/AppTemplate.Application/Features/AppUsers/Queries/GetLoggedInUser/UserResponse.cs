using AppTemplate.Domain.AppUsers.ValueObjects;
using System.Collections.ObjectModel;

namespace AppTemplate.Application.Features.AppUsers.Queries.GetLoggedInUser;

public sealed record UserResponse
{
    public string Email { get; set; }
    public string UserName { get; set; }
    public ICollection<LoggedInUserRolesDto> Roles { get; set; } = [];
    public NotificationPreference NotificationPreference { get; set; }
    public bool EmailConfirmed { get; set; } = false;

    public UserResponse(
        string email,
        string userName,
        Collection<LoggedInUserRolesDto> roles,
        NotificationPreference notificationPreference,
        bool emailConfirmed)
    {
        Email = email;
        UserName = userName;
        Roles = roles;
        NotificationPreference = notificationPreference;
        EmailConfirmed = emailConfirmed;
    }

    internal UserResponse() { }
}
