using EcoFind.Application;
using EcoFind.Infrastructure;
using EcoFind.Infrastructure.Autorization;
using EcoFind.Web;
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
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.RateLimiting;

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
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "EcoFind API",
        Version = "v1",
        Description = "API documentation for the EcoFind application."
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
    // Use SameSite = Lax for antiforgery cookies to avoid HTTPS issues over HTTP.
    builder.Services.ConfigureApplicationCookie(options =>
    {
        options.Cookie.Name = "EcoFindAuthCookie";
        // You can set this to Lax so it works over HTTP.
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
        options.Cookie.Name = "EcoFind.AntiForgery";
        options.Cookie.SameSite = SameSiteMode.Lax; // Use Lax for development over HTTP.
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
        options.Cookie.Name = "EcoFindAuthCookie";
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
        options.Cookie.Name = "EcoFind.AntiForgery";
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

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "EcoFind API V1");
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

app.Run();
