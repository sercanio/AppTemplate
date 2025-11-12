using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;

namespace AppTemplate.Application.Services.Authentication;

public static class ClaimsPrincipalExtensions
{
  public static Guid GetUserId(this ClaimsPrincipal? principal)
  {
    string? userId = principal?.FindFirstValue(JwtRegisteredClaimNames.Sub);

    return Guid.TryParse(userId, out Guid parsedUserId) ?
        parsedUserId :
        throw new ApplicationException("User id is unavailable");
  }

  public static string GetIdentityId(this ClaimsPrincipal principal)
  {
    if (principal == null)
      throw new ArgumentNullException(nameof(principal));

    // 1) Try the standard NameIdentifier claim (cookie or mapped JWT).
    var id = principal.FindFirstValue(ClaimTypes.NameIdentifier);
    if (!string.IsNullOrWhiteSpace(id))
      return id;

    // 2) Fallback: if we're in a 2FA or "Remember Me" flow, grab the .Name
    var fallbackSchemes = new[]
    {
            IdentityConstants.TwoFactorUserIdScheme,
            IdentityConstants.TwoFactorRememberMeScheme
        };

    var twoFactorIdentity = principal.Identities
        .FirstOrDefault(i => fallbackSchemes.Contains(i.AuthenticationType)
                          && !string.IsNullOrWhiteSpace(i.Name));

    if (twoFactorIdentity != null)
      return twoFactorIdentity.Name!;

    // 3) (Optional) If you ever add JWT-bearer without mapping NameIdentifier, you
    // could check JwtRegisteredClaimNames.Sub here.

    throw new ApplicationException("User identity is unavailable");
  }
}
