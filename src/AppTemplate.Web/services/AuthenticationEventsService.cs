using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using AppTemplate.Application.Services.Statistics;
using System.Security.Claims;

namespace AppTemplate.Web.Services;

public class AuthenticationEventsService
{
    private readonly IActiveSessionService _sessionService;

    public AuthenticationEventsService(IActiveSessionService sessionService)
    {
        _sessionService = sessionService;
    }

    public async Task OnSignedIn(CookieSignedInContext context)
    {
        if (context.Principal?.Identity?.IsAuthenticated == true && 
            context.Principal.FindFirstValue(ClaimTypes.NameIdentifier) is string userId &&
            !string.IsNullOrEmpty(userId))
        {
            await _sessionService.RecordUserActivityAsync(userId);
        }
    }

    public async Task OnSignedOut(CookieSigningOutContext context)
    {
        if (context.HttpContext.User.Identity?.IsAuthenticated == true && 
            context.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier) is string userId &&
            !string.IsNullOrEmpty(userId))
        {
            await _sessionService.RemoveUserSessionAsync(userId);
        }
    }
}