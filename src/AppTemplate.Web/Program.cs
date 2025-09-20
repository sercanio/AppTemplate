using AppTemplate.Application;
using AppTemplate.Infrastructure;
using AppTemplate.Web;
using AppTemplate.Web.Controllers.Api;
using AppTemplate.Web.Extensions;
using AppTemplate.Web.Middlewares;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Models;
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
    var seqConnectionString = context.Configuration.GetConnectionString("seq-logging");
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

// Replace this line:
// builder.AddRedisDistributedCache("redis-cache");

// With the following, which uses the standard Redis distributed cache registration:
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration.GetConnectionString("redis-cache") ?? "localhost:6379";
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

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
  options.SwaggerDoc($"v{ApiVersions.V1}", new OpenApiInfo
  {
    Title = "AppTemplate API",
    Version = $"v{ApiVersions.V1}",
    Description = "API documentation for the AppTemplate application."
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
  app.UseSwagger();
  app.UseSwaggerUI(options =>
  {
    foreach (var (url, name) in app.DescribeApiVersions()
                                       .Select(description => ($"/swagger/{description.GroupName}/swagger.json", description.GroupName.ToUpperInvariant())))
    {
      options.SwaggerEndpoint(url, name);
    }
  });
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

app.Run();