using Microsoft.AspNetCore.Mvc;
using System.Globalization;
using System.Net;
using System.Text;
using System.Text.Json;
using Myrtus.Clarity.Core.Application.Abstractions.Localization.Services;

namespace AppTemplate.Web.Middlewares;

public class ForbiddenResponseMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ForbiddenResponseMiddleware> _logger;
    private readonly ILocalizationService _localizationService;

    public ForbiddenResponseMiddleware(RequestDelegate next, ILogger<ForbiddenResponseMiddleware> logger, ILocalizationService localizationService)
    {
        _next = next;
        _logger = logger;
        _localizationService = localizationService;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        await _next(context);

        if (context.Response.StatusCode == (int)HttpStatusCode.Forbidden)
        {
            _logger.LogWarning("Forbidden request: {Path}", context.Request.Path);

            var language = GetLanguageFromHeader(context);
            var problemDetails = new ProblemDetails
            {
                Status = (int)HttpStatusCode.Forbidden,
                Type = "https://datatracker.ietf.org/doc/html/rfc7231#section-6.5.3",
                Title = "Forbidden",
                Detail = _localizationService.GetLocalizedString("Errors.Forbidden", language),
                Instance = context.Request.Path
            };

            context.Response.ContentType = "application/json; charset=utf-8";
            var options = new JsonSerializerOptions
            {
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
                WriteIndented = true
            };
            var json = JsonSerializer.Serialize(problemDetails, options);
            await context.Response.WriteAsync(json, Encoding.UTF8);
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
