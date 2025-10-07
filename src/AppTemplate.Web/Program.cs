using AppTemplate.Application;
using AppTemplate.Application.Authentication;
using AppTemplate.Infrastructure;
using AppTemplate.Infrastructure.Authentication;
using AppTemplate.Web;
using AppTemplate.Web.Controllers.Api;
using AppTemplate.Web.Extensions;
using Microsoft.AspNetCore.Identity;
using Microsoft.OpenApi;
using Scalar.AspNetCore;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog before other services
builder.Host.UseSerilog((context, services, configuration) =>
{
  configuration
      .ReadFrom.Configuration(context.Configuration)
      .ReadFrom.Services(services)
      .WriteTo.Console(new Serilog.Formatting.Json.JsonFormatter())
      .WriteTo.OpenTelemetry(options =>
      {
        options.Endpoint = "http://localhost:4317";
        options.Protocol = Serilog.Sinks.OpenTelemetry.OtlpProtocol.Grpc;
      });

  // Add Seq sink if connection string is available
  var seqConnectionString = context.Configuration.GetConnectionString("apptemplate-seq");
  if (!string.IsNullOrEmpty(seqConnectionString))
  {
    configuration.WriteTo.Seq(seqConnectionString);
  }
});

builder.AddServiceDefaults();

// Configure Identity for user management only (no UI components)
builder.Services.AddDefaultIdentity<IdentityUser>(options =>
    options.SignIn.RequireConfirmedAccount = false)
    .AddEntityFrameworkStores<ApplicationDbContext>();

// Only add API controllers
builder.Services.AddControllers();

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
                .ConfigureJwtAuthentication(builder.Environment, builder.Configuration)
                .ConfigureRateLimiting()
                .AddValidators();

// Configure OpenAPI and Scalar
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi(options =>
{
  options.OpenApiVersion = OpenApiSpecVersion.OpenApi3_0;
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

builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();

var app = builder.Build();

app.MapDefaultEndpoints();
app.UseCustomExceptionHandler();
app.UseRequestContextLogging();

if (app.Environment.IsDevelopment())
{
  app.UseMigrationsEndPoint();
  app.UseDeveloperExceptionPage();
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
app.UseAuthentication();
app.UseAuthorization();
app.UseCustomForbiddenRequestHandler();
app.UseRateLimiter();
app.UseRateLimitExceededHandler();

app.MapControllers();

if (app.Environment.IsDevelopment())
{
  app.MapOpenApi();
  app.MapScalarApiReference();
}

app.Run();