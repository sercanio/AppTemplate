public static string GetIdentityId(this ClaimsPrincipal? principal)
{
    string? identityId = principal?.FindFirstValue(ClaimTypes.NameIdentifier);
    
    if (string.IsNullOrEmpty(identityId) && 
        principal?.Identity?.AuthenticationType == "Identity.TwoFactorUserId")
    {
        identityId = principal.Identity.Name;
    }
    
    return identityId ?? throw new ApplicationException("User identity is unavailable");
}