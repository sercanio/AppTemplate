using AppTemplate.Application;
using AppTemplate.Infrastructure;
using AppTemplate.Web;
using AppTemplate.Web.Controllers.Api;
using AppTemplate.Web.Extensions;
using AppTemplate.Web.Middlewares;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi;
using Microsoft.OpenApi.Models;
using Scalar.AspNetCore;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog before other services
builder.Host.UseSerilog((context, services, configuration) =>
{
    configuration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .WriteTo.Console();
    
    // Add Seq sink if connection string is available
    var seqConnectionString = context.Configuration.GetConnectionString("apptemplate-seq");
    if (!string.IsNullOrEmpty(seqConnectionString))
    {
        configuration.WriteTo.Seq(seqConnectionString);
    }
});

builder.AddServiceDefaults();

builder.Services.AddDefaultIdentity<IdentityUser>(options =>
    options.SignIn.RequireConfirmedAccount = false)
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.AddControllersWithViews(options =>
    options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute()));

builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration.GetConnectionString("apptemplate-redis") ?? "localhost:6379";
    options.InstanceName = "AppTemplate:";
});

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddWebApi(builder.Configuration);

builder.Services.ConfigureControllers()
                .ConfigureCors(builder.Configuration)
                .ConfigureAuthenticationAndAntiforgery(builder.Environment)
                .ConfigureRateLimiting()
                .AddValidators();

// Configure OpenAPI and Scalar
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi(options =>
{
    options.OpenApiVersion = OpenApiSpecVersion.OpenApi3_0;
    // Set document info using OpenApiOptions properties if available
    // If you need to set title/description, you may need to use a document transformer:
    options.AddDocumentTransformer((document, context, cancellationToken) =>
    {
        document.Info = new Microsoft.OpenApi.Models.OpenApiInfo
        {
            Title = "AppTemplate API",
            Version = $"v{ApiVersions.V1}",
            Description = "API documentation for the AppTemplate application."
        };
        return Task.CompletedTask;
    });
});

var app = builder.Build();

app.MapDefaultEndpoints();

app.ApplyMigrations();
app.UseCustomExceptionHandler();
app.UseRequestContextLogging();

if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseHsts();
}

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}
app.UseRouting();
app.UseCors("CorsPolicy");
app.UseSessionTracking();
app.UseAuthorization();
app.UseCustomForbiddenRequestHandler();
app.UseRateLimiter();
app.UseRateLimitExceededHandler();

app.MapStaticAssets();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();
app.MapRazorPages().WithStaticAssets();

// Map OpenAPI and Scalar endpoints (only in development)
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.Run();