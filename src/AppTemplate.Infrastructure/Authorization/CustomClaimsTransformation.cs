using AppTemplate.Application.Services.Authentication;
using AppTemplate.Application.Services.Authorization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.JsonWebTokens;
using System.Security.Claims;

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
    if (principal.Identity is not { IsAuthenticated: true })
    {
      return principal;
    }

    // Check if we already have role and permission claims
    if (principal.HasClaim(c => c.Type == ClaimTypes.Role) &&
        principal.HasClaim(c => c.Type == "permission"))
    {
      return principal;
    }

    string identityId;

    // For JWT-only authentication, extract from JWT claims
    var jwtIdentity = principal.Identities.FirstOrDefault(i =>
        i.IsAuthenticated &&
        i.AuthenticationType == JwtBearerDefaults.AuthenticationScheme &&
        i.HasClaim(c => c.Type == "exp")
    );

    if (jwtIdentity != null)
    {
      // Use ClaimsPrincipalExtensions for robust extraction
      identityId = principal.FindFirstValue(ClaimTypes.NameIdentifier)
          ?? principal.FindFirstValue("nameid")
          ?? principal.FindFirstValue(JwtRegisteredClaimNames.Sub)
          ?? principal.GetIdentityId();
    }
    else
    {
      // Fallback: try all known claim types
      identityId = principal.FindFirstValue(ClaimTypes.NameIdentifier)
          ?? principal.FindFirstValue("nameid")
          ?? principal.FindFirstValue(JwtRegisteredClaimNames.Sub)
          ?? principal.GetIdentityId();
    }

    if (string.IsNullOrEmpty(identityId))
    {
      return principal;
    }

    try
    {
      var rolesResponse = await _authorizationService.GetRolesForUserAsync(identityId);
      var permissions = await _authorizationService.GetPermissionsForUserAsync(identityId);

      // Create new identity with JWT Bearer authentication type
      var ci = new ClaimsIdentity(
          JwtBearerDefaults.AuthenticationScheme,
          ClaimTypes.Name,
          ClaimTypes.Role
      );

      // Add the user ID claim
      ci.AddClaim(new Claim(ClaimTypes.NameIdentifier, rolesResponse.UserId.ToString()));

      // Add role claims
      foreach (var role in rolesResponse.Roles)
      {
        ci.AddClaim(new Claim(ClaimTypes.Role, role.Name.Value));
      }

      // Add permission claims
      foreach (var perm in permissions)
      {
        ci.AddClaim(new Claim("permission", perm));
      }

      // You need to fetch the username (from your user service or from the principal)
      var username = principal.Identity?.Name ?? ""; // Or fetch from your user service

      ci.AddClaim(new Claim(ClaimTypes.NameIdentifier, rolesResponse.UserId.ToString()));
      ci.AddClaim(new Claim(ClaimTypes.Name, username));

      principal.AddIdentity(ci);
    }
    catch (Exception)
    {
      // Log the exception if needed, but don't throw
      // This prevents authentication from failing completely
      return principal;
    }

    return principal;
  }
}
