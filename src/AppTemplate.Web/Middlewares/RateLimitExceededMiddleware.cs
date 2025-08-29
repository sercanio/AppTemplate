using AppTemplate.Core.Application.Abstractions.Localization.Services;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;

namespace AppTemplate.Web.Middlewares;

public class RateLimitExceededMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILocalizationService _localizationService;

    public RateLimitExceededMiddleware(RequestDelegate next, ILocalizationService localizationService)
    {
        _next = next;
        _localizationService = localizationService;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        await _next(context);

        if (context.Response.StatusCode == StatusCodes.Status429TooManyRequests)
        {
            var retryAfter = context.Response.Headers.RetryAfter.ToString();
            var language = GetLanguageFromHeader(context);

            var problemDetails = new ProblemDetails
            {
                Status = StatusCodes.Status429TooManyRequests,
                Type = "https://httpstatuses.com/429",
                Title = "Too Many Requests",
                Detail = _localizationService.GetLocalizedString("Errors.TooManyRequests", language),
                Instance = context.Request.Path
            };

            if (!string.IsNullOrEmpty(retryAfter))
            {
                problemDetails.Extensions["retryAfter"] = retryAfter;
            }

            context.Response.ContentType = "application/json; charset=utf-8";

            var options = new JsonSerializerOptions
            {
                Encoder = JavaScriptEncoder.Create(UnicodeRanges.All),
                WriteIndented = true
            };

            var jsonResponse = JsonSerializer.Serialize(problemDetails, options);
            await context.Response.WriteAsync(jsonResponse, Encoding.UTF8);
        }
    }

    private string GetLanguageFromHeader(HttpContext context)
    {
        var acceptLanguageHeader = context.Request.Headers["Accept-Language"].ToString();
        if (!string.IsNullOrEmpty(acceptLanguageHeader))
        {
            var languages = acceptLanguageHeader.Split(',');
            if (languages.Length > 0)
            {
                return languages[0];
            }
        }
        return CultureInfo.CurrentCulture.Name;
    }
}
