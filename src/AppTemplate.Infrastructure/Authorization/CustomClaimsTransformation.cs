using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using AppTemplate.Application.Services.Authorization;
using AppTemplate.Core.Application.Abstractions.Authentication.Azure;

namespace AppTemplate.Infrastructure.Authorization;

public sealed class CustomClaimsTransformation : IClaimsTransformation
{
    private readonly IAuthorizationService _authorizationService;

    public CustomClaimsTransformation(IAuthorizationService authorizationService)
    {
        _authorizationService = authorizationService;
    }

    public async Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
    {
        if (principal.Identity is not { IsAuthenticated: true } ||
            principal.HasClaim(c => c.Type == ClaimTypes.Role) ||
            principal.HasClaim(c => c.Type == "permission"))
        {
            return principal;
        }

        var identityId = principal.GetIdentityId();

        var rolesResponse = await _authorizationService.GetRolesForUserAsync(identityId);
        var permissions = await _authorizationService.GetPermissionsForUserAsync(identityId);

        var ci = new ClaimsIdentity(
            CookieAuthenticationDefaults.AuthenticationScheme,
            ClaimTypes.Name,
            ClaimTypes.Role
        );

        ci.AddClaim(new Claim(ClaimTypes.NameIdentifier, rolesResponse.UserId.ToString()));

        foreach (var role in rolesResponse.Roles)
        {
            ci.AddClaim(new Claim(ClaimTypes.Role, role.Name.Value));
        }

        foreach (var perm in permissions)
        {
            ci.AddClaim(new Claim("permission", perm));
        }

        principal.AddIdentity(ci);
        return principal;
    }
}
