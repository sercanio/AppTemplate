using AppTemplate.Application;
using AppTemplate.Infrastructure;
using AppTemplate.Presentation;
using AppTemplate.Presentation.Extensions;
using OpenTelemetry.Metrics;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
builder.Host.ConfigureSerilog();

// Configure services
builder.Services.ConfigureIdentity()
                .ConfigureControllers()
                .ConfigureRedisCache(builder.Configuration)
                .AddApplication()
                .AddInfrastructure(builder.Configuration)
                .AddWebApi()
                .ConfigureCors(builder.Configuration)
                .ConfigureJwtAuthentication(builder.Environment, builder.Configuration)
                .ConfigureRateLimiting()
                .AddValidators()
                .ConfigureOpenApiWithScalar();

builder.Services.AddOpenTelemetry()
    .WithMetrics(metrics =>
    {
      metrics.AddPrometheusExporter();
      metrics.AddAspNetCoreInstrumentation();
      metrics.AddHttpClientInstrumentation();
    });

var app = builder.Build();

app.UseCustomExceptionHandler();
app.UseRequestContextLogging();

app.ConfigureDevelopmentEnvironment(app.Environment);
app.ConfigureMiddlewarePipeline(app.Environment);

app.MapControllers();
app.MapDevelopmentEndpoints(app.Environment);
app.MapPrometheusScrapingEndpoint();

app.Run();
