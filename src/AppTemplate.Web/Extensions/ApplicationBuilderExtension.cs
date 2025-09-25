using AppTemplate.Application.Features.Roles.Commands.Create;
using AppTemplate.Infrastructure;
using AppTemplate.Web.Middlewares;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using Microsoft.OpenApi.Models;
using Scalar.AspNetCore;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.RateLimiting;

namespace AppTemplate.Web.Extensions;

internal static class ApplicationBuilderExtensions
{
  // Removed the ApplyMigrations method as migrations are handled by the migration service

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

  public static IServiceCollection ConfigureAuthenticationAndAntiforgery(this IServiceCollection services, Microsoft.AspNetCore.Hosting.IWebHostEnvironment env, IConfiguration configuration)
  {
    // Configure multiple authentication schemes
    services.AddAuthentication()
    .AddPolicyScheme("Smart", "Authorization Bearer or Cookie", options =>
    {
      options.ForwardDefaultSelector = context =>
      {
        string authorization = context.Request.Headers.Authorization.FirstOrDefault();
        if (!string.IsNullOrEmpty(authorization) && authorization.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
          return JwtBearerDefaults.AuthenticationScheme;
        
        return CookieAuthenticationDefaults.AuthenticationScheme;
      };
    })
    .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
    {
      var jwtSettings = configuration.GetSection("Jwt");
      var key = Encoding.UTF8.GetBytes(jwtSettings["Secret"]);
      
      options.TokenValidationParameters = new TokenValidationParameters
      {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ClockSkew = TimeSpan.Zero,
        RequireExpirationTime = true
      };

      options.Events = new JwtBearerEvents
      {
        OnChallenge = context =>
        {
          context.HandleResponse();
          context.Response.StatusCode = StatusCodes.Status401Unauthorized;
          context.Response.ContentType = "application/json";
          
          var problem = new ProblemDetails
          {
            Type = "https://tools.ietf.org/html/rfc7235#section-3.1",
            Title = "Unauthorized",
            Status = StatusCodes.Status401Unauthorized,
            Detail = "You must provide a valid JWT token to access this API.",
            Instance = context.Request.Path
          };
          problem.Extensions.Add("traceId", context.HttpContext.TraceIdentifier);
          
          return context.Response.WriteAsync(JsonSerializer.Serialize(problem));
        },
        OnForbidden = context =>
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
        },
        OnAuthenticationFailed = context =>
        {
          if (context.Exception.GetType() == typeof(SecurityTokenExpiredException))
          {
            context.Response.Headers.Add("Token-Expired", "true");
          }
          return Task.CompletedTask;
        }
      };
    })
    .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
    {
      options.Cookie.Name = "AppTemplate.AuthCookie";
      
      if (env.IsDevelopment())
      {
        options.Cookie.SameSite = Microsoft.AspNetCore.Http.SameSiteMode.Lax;
        options.Cookie.SecurePolicy = Microsoft.AspNetCore.Http.CookieSecurePolicy.SameAsRequest;
      }
      else
      {
        options.Cookie.SameSite = Microsoft.AspNetCore.Http.SameSiteMode.Strict;
        options.Cookie.SecurePolicy = Microsoft.AspNetCore.Http.CookieSecurePolicy.Always;
      }

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

    // Configure antiforgery
    services.AddAntiforgery(options =>
    {
      options.Cookie.Name = "AppTemplate.AntiForgery";
      options.HeaderName = "X-XSRF-TOKEN";
      
      if (env.IsDevelopment())
      {
        options.Cookie.SameSite = Microsoft.AspNetCore.Http.SameSiteMode.Lax;
        options.Cookie.SecurePolicy = Microsoft.AspNetCore.Http.CookieSecurePolicy.SameAsRequest;
      }
      else
      {
        options.Cookie.SameSite = Microsoft.AspNetCore.Http.SameSiteMode.Strict;
        options.Cookie.SecurePolicy = Microsoft.AspNetCore.Http.CookieSecurePolicy.Always;
      }
    });

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

  // New extension methods for API documentation
  public static IServiceCollection ConfigureApiDocumentation(this IServiceCollection services)
  {
    services.AddEndpointsApiExplorer();
    services.AddOpenApi(options =>
    {
      options.OpenApiVersion = OpenApiSpecVersion.OpenApi3_0;
      // Set document info using document transformers or other supported means if needed.
      // The following is a placeholder for setting title/description if your OpenAPI library supports it.
      // Otherwise, configure this in your OpenAPI UI setup.
    });
    
    return services;
  }

  public static IEndpointRouteBuilder MapApiDocumentation(this IEndpointRouteBuilder endpoints, IWebHostEnvironment environment)
  {
    if (environment.IsDevelopment())
    {
      endpoints.MapOpenApi();
      endpoints.MapScalarApiReference(options =>
      {
        options.Title = "AppTemplate API";
        options.Theme = ScalarTheme.BluePlanet;
        options.ShowSidebar = true;
      });
    }
    
    return endpoints;
  }
}