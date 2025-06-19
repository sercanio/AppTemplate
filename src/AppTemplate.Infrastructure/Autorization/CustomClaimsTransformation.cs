using AppTemplate.Infrastructure.Authorization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.DependencyInjection;
using Myrtus.Clarity.Core.Infrastructure.Authentication.Azure;
using System.Security.Claims;
using System.Threading.Tasks;

namespace AppTemplate.Infrastructure.Authorization;

public sealed class CustomClaimsTransformation : IClaimsTransformation
{
    private readonly IServiceProvider _services;

    public CustomClaimsTransformation(IServiceProvider services)
    {
        _services = services;
    }

    public async Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
    {
        if (principal.Identity is not { IsAuthenticated: true } ||
            principal.HasClaim(c => c.Type == ClaimTypes.Role) ||
            principal.HasClaim(c => c.Type == "permission"))
        {
            return principal;
        }

        using var scope = _services.CreateScope();
        var authSvc = scope.ServiceProvider.GetRequiredService<AuthorizationService>();
        var identityId = principal.GetIdentityId();

        var rolesResponse = await authSvc.GetRolesForUserAsync(identityId);
        var permissions = await authSvc.GetPermissionsForUserAsync(identityId);

        var ci = new ClaimsIdentity(
            CookieAuthenticationDefaults.AuthenticationScheme,
            ClaimTypes.Name,
            ClaimTypes.Role
        );

        ci.AddClaim(new Claim(ClaimTypes.NameIdentifier, rolesResponse.UserId.ToString()));

        foreach (var role in rolesResponse.Roles)
        {
            // assuming Role.Name.Value is your role string
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
