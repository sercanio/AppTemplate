namespace AppTemplate.Application.Services.Authorization;

public interface IAuthorizationService
{
  Task<UserRolesResponse> GetRolesForUserAsync(string identityId);
  Task<HashSet<string>> GetPermissionsForUserAsync(string identityId);
}
