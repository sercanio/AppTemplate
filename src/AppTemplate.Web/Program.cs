using AppTemplate.Application;
using AppTemplate.Infrastructure;
using AppTemplate.Infrastructure.Autorization;
using AppTemplate.Web;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.OpenApi.Models;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Bson.Serialization;
using MongoDB.Bson;
using Myrtus.Clarity.Core.Infrastructure.Authorization;
using Myrtus.Clarity.Core.Infrastructure.SignalR.Hubs;
using Myrtus.Clarity.Core.Application.Abstractions.Module;
using System.Reflection;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.RateLimiting;
using AppTemplate.Web.services;
using Asp.Versioning.ApiExplorer;
using AppTemplate.Web.Controllers.Api;

var builder = WebApplication.CreateBuilder(args);

// Register the Guid serializer with Standard representation.
BsonSerializer.RegisterSerializer(new GuidSerializer(GuidRepresentation.Standard));

// Configure Identity and roles.
builder.Services.AddDefaultIdentity<IdentityUser>(options =>
    options.SignIn.RequireConfirmedAccount = false)
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.AddSingleton<IAuthorizationPolicyProvider, PermissionAuthorizationPolicyProvider>();
builder.Services.AddSingleton<IAuthorizationHandler, PermissionAuthorizationHandler>();

builder.Services.AddControllersWithViews(options =>
{
    options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute());
});

// Register Antiforgery (header name is set; further config below)
builder.Services.AddAntiforgery(options =>
{
    options.HeaderName = "X-XSRF-TOKEN";
});

// Register Email Sender and Memory Cache.
builder.Services.AddTransient<IEmailSender, AzureEmailSender>();
builder.Services.AddMemoryCache();

// Register application, infrastructure, and WebAPI services.
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddWebApi(builder.Configuration);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    // Define a swagger doc for version "v1"
    options.SwaggerDoc($"v{ApiVersions.V1}", new OpenApiInfo
    {
        Title = "AppTemplate API",
        Version = $"v{ApiVersions.V1}",
        Description = "API documentation for the AppTemplate application."
    });
});

builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter("Fixed", limiterOptions =>
    {
        limiterOptions.Window = TimeSpan.FromSeconds(60);
        limiterOptions.PermitLimit = 30;
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

// Configure cookie, antiforgery, and CORS based on environment.
if (builder.Environment.IsDevelopment())
{
    // In development, use relaxed settings.
    builder.Services.ConfigureApplicationCookie(options =>
    {
        options.Cookie.Name = "AppTemplateAuthCookie";
        options.Cookie.SameSite = SameSiteMode.Lax;
        options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
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
            else
            {
                context.Response.Redirect(context.RedirectUri);
                return Task.CompletedTask;
            }
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
            else
            {
                context.Response.Redirect(context.RedirectUri);
                return Task.CompletedTask;
            }
        };
    });

    builder.Services.AddAntiforgery(options =>
    {
        options.Cookie.Name = "AppTemplate.AntiForgery";
        options.Cookie.SameSite = SameSiteMode.Lax;
        options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
        options.HeaderName = "X-XSRF-TOKEN";
    });

    var allowedOrigins = builder.Configuration.GetSection("AllowedOrigins").Get<string[]>();
    if (allowedOrigins == null || allowedOrigins.Length == 0)
    {
        throw new ArgumentNullException();
    }

    builder.Services.AddCors(options =>
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
}
else
{
    // Production settings: assume HTTPS.
    builder.Services.ConfigureApplicationCookie(options =>
    {
        options.Cookie.Name = "AppTemplateAuthCookie";
        options.Cookie.SameSite = SameSiteMode.Strict;
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
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
            else
            {
                context.Response.Redirect(context.RedirectUri);
                return Task.CompletedTask;
            }
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
            else
            {
                context.Response.Redirect(context.RedirectUri);
                return Task.CompletedTask;
            }
        };
    });

    builder.Services.AddAntiforgery(options =>
    {
        options.Cookie.Name = "AppTemplate.AntiForgery";
        options.Cookie.SameSite = SameSiteMode.Strict;
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        options.HeaderName = "X-XSRF-TOKEN";
    });

    var allowedOrigins = builder.Configuration.GetSection("AllowedOrigins").Get<string[]>();
    if (allowedOrigins == null || allowedOrigins.Length == 0)
    {
        throw new ArgumentNullException();
    }

    builder.Services.AddCors(options =>
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
}

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

//
// --- Begin Dynamic Module Loading Integration ---
//

var modulesPath = builder.Environment.IsDevelopment()
    ? Path.GetFullPath(Path.Combine(builder.Environment.ContentRootPath, "..", "..", "modules"))
    : Path.GetFullPath(Path.Combine(builder.Environment.ContentRootPath, "modules"));

var moduleInstances = new List<IClarityModule>();

if (Directory.Exists(modulesPath))
{
    var moduleFiles = Directory.GetFiles(modulesPath, "*.dll", SearchOption.AllDirectories)
        .Where(file => !file.Contains("\\obj\\") && !file.Contains("\\ref\\"));

    foreach (var moduleFile in moduleFiles)
    {
        Console.WriteLine($"Loading module: {moduleFile}");
        try
        {
            var assembly = Assembly.LoadFrom(moduleFile);

            // Add controllers from the module into the MVC pipeline.
            builder.Services.AddControllers().AddApplicationPart(assembly).AddControllersAsServices();

            // Find all types that implement IClarityModule.
            var moduleTypes = assembly.GetTypes()
                .Where(t => typeof(IClarityModule).IsAssignableFrom(t)
                            && !t.IsInterface && !t.IsAbstract);

            // Instantiate each module so it can register its services.
            foreach (var type in moduleTypes)
            {
                var moduleInstance = (IClarityModule)Activator.CreateInstance(type)!;
                moduleInstance.ConfigureServices(builder.Services, builder.Configuration);
                moduleInstances.Add(moduleInstance);
            }
        }
        catch (ReflectionTypeLoadException ex)
        {
            Console.WriteLine($"Error loading module {moduleFile}: {ex.LoaderExceptions.FirstOrDefault()?.Message}");
            foreach (var loaderException in ex.LoaderExceptions)
            {
                Console.WriteLine($"Loader Exception: {loaderException.Message}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading module {moduleFile}: {ex.Message}");
        }
    }
}

//
// --- End Dynamic Module Loading Integration ---
//

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        foreach ((string url, string name) in
            from ApiVersionDescription description in app.DescribeApiVersions()
            let url = $"/swagger/{description.GroupName}/swagger.json"
            let name = description.GroupName.ToUpperInvariant()
            select (url, name))
        {
            options.SwaggerEndpoint(url, name);
        }
    });
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseCors("CorsPolicy");
app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthorization();

app.MapStaticAssets();

app.UseRateLimiter();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.MapRazorPages()
   .WithStaticAssets();

app.MapHub<AuditLogHub>("/auditLogHub");
app.MapHub<NotificationHub>("/notificationHub");

//
// --- Invoke Module Configuration ---
//
foreach (var moduleInstance in moduleInstances)
{
    moduleInstance.Configure(app);
}

app.Run();
