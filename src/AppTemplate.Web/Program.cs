using AppTemplate.Application;
using AppTemplate.Infrastructure;
using AppTemplate.Web;
using AppTemplate.Web.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
builder.Host.ConfigureSerilog();

builder.AddServiceDefaults();

// Configure services
builder.Services.ConfigureIdentity()
                .ConfigureControllers()
                .ConfigureRedisCache(builder.Configuration)
                .AddApplication()
                .AddInfrastructure(builder.Configuration)
                .AddWebApi(builder.Configuration)
                .ConfigureCors(builder.Configuration)
                .ConfigureJwtAuthentication(builder.Environment, builder.Configuration)
                .ConfigureRateLimiting()
                .AddValidators()
                .ConfigureOpenApiWithScalar();

var app = builder.Build();

app.MapDefaultEndpoints();
app.UseCustomExceptionHandler();
app.UseRequestContextLogging();

app.ConfigureDevelopmentEnvironment(app.Environment);
app.ConfigureMiddlewarePipeline(app.Environment);

app.MapControllers();
app.MapDevelopmentEndpoints(app.Environment);

app.Run();