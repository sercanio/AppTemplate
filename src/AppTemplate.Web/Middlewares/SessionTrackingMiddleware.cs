using AppTemplate.Application.Services.Statistics;
using System.Security.Claims;

namespace AppTemplate.Web.Middlewares;

public class SessionTrackingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IActiveSessionService _sessionService;

    public SessionTrackingMiddleware(RequestDelegate next, IActiveSessionService sessionService)
    {
        _next = next;
        _sessionService = sessionService;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Record activity for authenticated users
        if (context.User.Identity?.IsAuthenticated == true)
        {
            string userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!string.IsNullOrEmpty(userId))
            {
                await _sessionService.RecordUserActivityAsync(userId);
            }
        }

        await _next(context);
    }
}

// Extension method
public static class SessionTrackingMiddlewareExtensions
{
    public static IApplicationBuilder UseSessionTracking(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<SessionTrackingMiddleware>();
    }
}