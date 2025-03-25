using AppTemplate.Domain.AppUsers.ValueObjects;
using System.Collections.ObjectModel;

namespace AppTemplate.Application.Features.AppUsers.Queries.GetLoggedInUser;

public sealed record UserResponse
{
    public string Email { get; set; }
    public string UserName { get; set; }
    public ICollection<LoggedInUserRolesDto> Roles { get; set; } = [];
    public NotificationPreference NotificationPreference { get; set; }

    public UserResponse(
        string email,
        string userName,
        Collection<LoggedInUserRolesDto> roles,
        NotificationPreference notificationPreference)
    {
        Email = email;
        UserName = userName;
        Roles = roles;
        NotificationPreference = notificationPreference;
    }

    internal UserResponse() { }
}
