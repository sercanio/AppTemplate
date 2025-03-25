using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Caching.Memory;
using System.Security.Claims;

namespace AppTemplate.Infrastructure.Autorization;

public class RoleClaimsTransformation : IClaimsTransformation
{
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly IMemoryCache _cache;
    private readonly TimeSpan _cacheExpiration = TimeSpan.FromMinutes(1);

    public RoleClaimsTransformation(RoleManager<IdentityRole> roleManager, IMemoryCache cache)
    {
        _roleManager = roleManager;
        _cache = cache;
    }

    public async Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
    {
        if (principal.Identity is not ClaimsIdentity identity)
        {
            return principal;
        }

        var roleNames = identity.FindAll(identity.RoleClaimType)
                                .Select(r => r.Value)
                                .Distinct();

        var additionalClaims = new List<Claim>();

        foreach (var roleName in roleNames)
        {
            string cacheKey = $"RoleClaims-{roleName}";

            if (!_cache.TryGetValue(cacheKey, out List<Claim>? roleClaims))
            {
                var role = await _roleManager.FindByNameAsync(roleName);
                if (role != null)
                {
                    roleClaims = (await _roleManager.GetClaimsAsync(role)).ToList();
                    _cache.Set(cacheKey, roleClaims, _cacheExpiration);
                }
            }

            if (roleClaims != null)
            {
                foreach (var claim in roleClaims)
                {
                    if (!identity.HasClaim(c => c.Type == claim.Type && c.Value == claim.Value))
                    {
                        additionalClaims.Add(claim);
                    }
                }
            }
        }

        if (additionalClaims.Any())
        {
            identity.AddClaims(additionalClaims);
        }

        return principal;
    }
}
