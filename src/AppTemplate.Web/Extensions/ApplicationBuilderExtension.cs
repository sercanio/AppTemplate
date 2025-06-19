using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.RateLimiting;
using AppTemplate.Application.Features.Roles.Commands.Create;
using AppTemplate.Infrastructure;
using AppTemplate.Web.Middlewares;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Myrtus.Clarity.Core.Application.Abstractions.Module;

namespace AppTemplate.Web.Extensions;

internal static class ApplicationBuilderExtensions
{
    public static void ApplyMigrations(this IApplicationBuilder app)
    {
        using var scope = app.ApplicationServices.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        dbContext.Database.Migrate();
    }

    public static void UseCustomExceptionHandler(this IApplicationBuilder app)
    {
        app.UseMiddleware<ExceptionHandlingMiddleware>();
    }

    public static void UseCustomForbiddenRequestHandler(this IApplicationBuilder app)
    {
        app.UseMiddleware<ForbiddenResponseMiddleware>();
    }

    public static void UseRateLimitExceededHandler(this IApplicationBuilder app)
    {
        app.UseMiddleware<RateLimitExceededMiddleware>();
    }

    public static IApplicationBuilder UseRequestContextLogging(this IApplicationBuilder app)
    {
        app.UseMiddleware<RequestContextLoggingMiddleware>();
        return app;
    }

    // Service-level configuration extensions
    public static IServiceCollection ConfigureCors(this IServiceCollection services, IConfiguration configuration)
    {
        // Read allowed origins from configuration.
        string[] allowedOrigins = configuration.GetSection("AllowedOrigins").Get<string[]>();
        services.AddCors(options =>
        {
            options.AddPolicy("CorsPolicy", policy =>
            {
                policy.WithOrigins(allowedOrigins)
                      .AllowAnyMethod()
                      .AllowAnyHeader()
                      .AllowCredentials()
                      .WithExposedHeaders("Content-Disposition");
            });
        });
        return services;
    }

    public static IServiceCollection ConfigureControllers(this IServiceCollection services)
    {
        services.AddControllers().AddJsonOptions(options =>
        {
            options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        });
        return services;
    }

    public static IServiceCollection AddValidators(this IServiceCollection services)
    {
        services.AddValidatorsFromAssemblyContaining<CreateRoleValidationhandler>();
        return services;
    }

    public static IServiceCollection ConfigureAuthenticationAndAntiforgery(this IServiceCollection services, Microsoft.AspNetCore.Hosting.IWebHostEnvironment env)
    {
        if (env.IsDevelopment())
        {
            services.ConfigureApplicationCookie(options =>
            {
                options.Cookie.Name = "AppTemplateAuthCookie";
                options.Cookie.SameSite = Microsoft.AspNetCore.Http.SameSiteMode.Lax;
                options.Cookie.SecurePolicy = Microsoft.AspNetCore.Http.CookieSecurePolicy.SameAsRequest;
                options.Events.OnRedirectToLogin = context =>
                {
                    if (context.Request.Path.StartsWithSegments("/api"))
                    {
                        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                        context.Response.ContentType = "application/json";
                        var problem = new ProblemDetails
                        {
                            Type = "https://tools.ietf.org/html/rfc7235#section-3.1",
                            Title = "Unauthorized",
                            Status = StatusCodes.Status401Unauthorized,
                            Detail = "You must be logged in to access this API.",
                            Instance = context.Request.Path
                        };
                        problem.Extensions.Add("traceId", context.HttpContext.TraceIdentifier);
                        return context.Response.WriteAsync(JsonSerializer.Serialize(problem));
                    }
                    context.Response.Redirect(context.RedirectUri);
                    return Task.CompletedTask;
                };
                options.Events.OnRedirectToAccessDenied = context =>
                {
                    if (context.Request.Path.StartsWithSegments("/api"))
                    {
                        context.Response.StatusCode = StatusCodes.Status403Forbidden;
                        context.Response.ContentType = "application/json";
                        var problem = new ProblemDetails
                        {
                            Type = "https://tools.ietf.org/html/rfc7231#section-6.5.3",
                            Title = "Forbidden",
                            Status = StatusCodes.Status403Forbidden,
                            Detail = "You do not have permission to access this resource.",
                            Instance = context.Request.Path
                        };
                        problem.Extensions.Add("traceId", context.HttpContext.TraceIdentifier);
                        return context.Response.WriteAsync(JsonSerializer.Serialize(problem));
                    }
                    context.Response.Redirect(context.RedirectUri);
                    return Task.CompletedTask;
                };
            });

            services.AddAntiforgery(options =>
            {
                options.Cookie.Name = "AppTemplate.AntiForgery";
                options.Cookie.SameSite = Microsoft.AspNetCore.Http.SameSiteMode.Lax;
                options.Cookie.SecurePolicy = Microsoft.AspNetCore.Http.CookieSecurePolicy.SameAsRequest;
                options.HeaderName = "X-XSRF-TOKEN";
            });
        }
        else
        {
            services.ConfigureApplicationCookie(options =>
            {
                options.Cookie.Name = "AppTemplateAuthCookie";
                options.Cookie.SameSite = Microsoft.AspNetCore.Http.SameSiteMode.Strict;
                options.Cookie.SecurePolicy = Microsoft.AspNetCore.Http.CookieSecurePolicy.Always;
                options.Events.OnRedirectToLogin = context =>
                {
                    if (context.Request.Path.StartsWithSegments("/api"))
                    {
                        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                        context.Response.ContentType = "application/json";
                        var problem = new ProblemDetails
                        {
                            Type = "https://tools.ietf.org/html/rfc7235#section-3.1",
                            Title = "Unauthorized",
                            Status = StatusCodes.Status401Unauthorized,
                            Detail = "You must be logged in to access this API.",
                            Instance = context.Request.Path
                        };
                        problem.Extensions.Add("traceId", context.HttpContext.TraceIdentifier);
                        return context.Response.WriteAsync(JsonSerializer.Serialize(problem));
                    }
                    context.Response.Redirect(context.RedirectUri);
                    return Task.CompletedTask;
                };
                options.Events.OnRedirectToAccessDenied = context =>
                {
                    if (context.Request.Path.StartsWithSegments("/api"))
                    {
                        context.Response.StatusCode = StatusCodes.Status403Forbidden;
                        context.Response.ContentType = "application/json";
                        var problem = new ProblemDetails
                        {
                            Type = "https://tools.ietf.org/html/rfc7231#section-6.5.3",
                            Title = "Forbidden",
                            Status = StatusCodes.Status403Forbidden,
                            Detail = "You do not have permission to access this resource.",
                            Instance = context.Request.Path
                        };
                        problem.Extensions.Add("traceId", context.HttpContext.TraceIdentifier);
                        return context.Response.WriteAsync(JsonSerializer.Serialize(problem));
                    }
                    context.Response.Redirect(context.RedirectUri);
                    return Task.CompletedTask;
                };
            });

            services.AddAntiforgery(options =>
            {
                options.Cookie.Name = "AppTemplate.AntiForgery";
                options.Cookie.SameSite = Microsoft.AspNetCore.Http.SameSiteMode.Strict;
                options.Cookie.SecurePolicy = Microsoft.AspNetCore.Http.CookieSecurePolicy.Always;
                options.HeaderName = "X-XSRF-TOKEN";
            });
        }

        return services;
    }

    // Rate limiting configuration moved to an extension method.
    public static IServiceCollection ConfigureRateLimiting(this IServiceCollection services)
    {
        services.AddRateLimiter(options =>
        {
            options.AddFixedWindowLimiter("Fixed", limiterOptions =>
            {
                limiterOptions.Window = TimeSpan.FromSeconds(60);
                limiterOptions.PermitLimit = 180;
                limiterOptions.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                limiterOptions.QueueLimit = 0;
            });

            options.OnRejected = async (context, cancellationToken) =>
            {
                context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                if (context.HttpContext.Request.Path.StartsWithSegments("/api"))
                {
                    context.HttpContext.Response.ContentType = "application/json";
                    var problem = new ProblemDetails
                    {
                        Type = "https://tools.ietf.org/html/rfc6585#section-3",
                        Title = "Rate Limit Exceeded",
                        Status = StatusCodes.Status429TooManyRequests,
                        Detail = "You have exceeded the allowed number of requests. Please try again later.",
                        Instance = context.HttpContext.Request.Path
                    };
                    if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
                    {
                        problem.Extensions.Add("retryAfter", retryAfter.TotalSeconds);
                    }
                    await context.HttpContext.Response.WriteAsync(JsonSerializer.Serialize(problem), cancellationToken);
                }
                else
                {
                    context.HttpContext.Response.ContentType = "text/html";
                    await context.HttpContext.Response.WriteAsync(@"
                            <html>
                                <head><title>Too Many Requests</title></head>
                                <body style='background-color: #000; color: #fff;'>
                                    <p>You have exceeded the allowed number of requests. Please try again later.</p>
                                </body>
                            </html>", cancellationToken);
                }
            };
        });
        return services;
    }
}

