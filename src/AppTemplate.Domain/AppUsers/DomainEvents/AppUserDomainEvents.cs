namespace AppTemplate.Domain.Users.DomainEvents;

public sealed record AppUserDomainEvents
{
    public static readonly string Created = "AppUserCreated";
    public static readonly string Deleted = "AppUserDeleted";
    public static readonly string UpdatedName = "AppUserNameUpdated";
    public static readonly string UpdatedEmail = "AppUserEmailUpdated";
    public static readonly string UpdatedPassword = "AppUserPasswordUpdated";
    public static readonly string AddedRole = "AppUserRoleAdded";
    public static readonly string RemovedRole = "AppUserRoleRemoved";
    public static readonly string UserLoggedIn = "AppUserLoggedIn";
}
