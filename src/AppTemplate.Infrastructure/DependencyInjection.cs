using AppTemplate.Application.Data.Dapper;
using AppTemplate.Application.Repositories;
using AppTemplate.Application.Services.Authentication;
using AppTemplate.Application.Services.Authorization;
using AppTemplate.Application.Services.Clock;
using AppTemplate.Application.Services.EmailSenders;
using AppTemplate.Application.Services.Notifications;
using AppTemplate.Application.Services.OutboxMessages;
using AppTemplate.Application.Services.Statistics;
using AppTemplate.Domain;
using AppTemplate.Domain.OutboxMessages;
using AppTemplate.Infrastructure.Authentication;
using AppTemplate.Infrastructure.Authorization;
using AppTemplate.Infrastructure.Data.Dapper;
using AppTemplate.Infrastructure.Repositories;
using Asp.Versioning;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Quartz;

namespace AppTemplate.Infrastructure;

public static class DependencyInjection
{
  public static IServiceCollection AddInfrastructure(
          this IServiceCollection services,
          IConfiguration configuration)
  {
    services.AddDbContext<ApplicationDbContext>(options =>
    {
      options.UseNpgsql(configuration.GetConnectionString("Database"));

      // Configure warnings to ignore the pending model changes warning
      options.ConfigureWarnings(warnings =>
      {
        warnings.Ignore(RelationalEventId.PendingModelChangesWarning);
      });
    });

    if (configuration == null)
      throw new ArgumentNullException(nameof(configuration), "Configuration cannot be null in AddInfrastructure.");

    services.AddTransient<IDateTimeProvider, DateTimeProvider>();
    services.AddScoped<IUserContext, UserContext>();

    AddConnectionProviders(services);
    AddBackgroundJobs(services, configuration);
    AddApiVersioning(services);
    AddPersistence(services, configuration);
    AddAuthorization(services);
    AddNotification(services);
    AddSignalR(services);
    AddAuthenticationStatisticsServices(services, configuration);
    AddEmailServices(services);
    ConfigureJwtTokenService(services);

    return services;
  }

  private static void AddConnectionProviders(IServiceCollection services)
  {
    services.AddScoped<ISqlConnectionFactory, SqlConnectionFactory>();
  }

  private static void AddBackgroundJobs(IServiceCollection services, IConfiguration configuration)
  {
    services.Configure<OutboxOptions>(configuration.GetSection("Outbox"))
            .AddQuartz()
            .AddQuartzHostedService(options => options.WaitForJobsToComplete = true)
            .ConfigureOptions<ProcessOutboxMessagesJobSetup>();
  }

  private static void AddApiVersioning(IServiceCollection services)
  {
    services
        .AddApiVersioning(options =>
        {
          options.DefaultApiVersion = new ApiVersion(1);
          options.ReportApiVersions = true;
          options.ApiVersionReader = new UrlSegmentApiVersionReader();
        })
        .AddMvc()
        .AddApiExplorer(options =>
        {
          options.GroupNameFormat = "'v'VVV";
          options.SubstituteApiVersionInUrl = true;
        });
  }

  private static void AddPersistence(IServiceCollection services, IConfiguration configuration)
  {
    services.AddDatabaseDeveloperPageExceptionFilter();

    services.AddScoped<ISqlConnectionFactory, SqlConnectionFactory>()
        .AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<ApplicationDbContext>())
        .AddScoped<IAppUsersRepository, AppUsersRepository>()
        .AddScoped<IRolesRepository, RolesRepository>()
        .AddScoped<IPermissionsRepository, PermissionsRepository>()
        .AddScoped<INotificationsRepository, NotificationsRepository>();
  }

  private static void AddAuthorization(IServiceCollection services)
  {
    services.AddScoped<AppTemplate.Application.Services.Authorization.IAuthorizationService, AuthorizationService>();
    services.AddTransient<IClaimsTransformation, CustomClaimsTransformation>()
            .AddTransient<IAuthorizationHandler, PermissionAuthorizationHandler>()
            .AddTransient<IAuthorizationPolicyProvider, PermissionAuthorizationPolicyProvider>();
  }

  private static IServiceCollection ConfigureJwtTokenService(this IServiceCollection services)
  {
    services.AddScoped<IJwtTokenService, JwtTokenService>();
    return services;
  }

  private static void AddNotification(IServiceCollection services)
  {
    services.AddTransient<INotificationService, NotificationsService>();
  }

  private static void AddSignalR(IServiceCollection services)
  {
    services.AddSignalR();
  }

  private static void AddAuthenticationStatisticsServices(IServiceCollection services, IConfiguration configuration)
  {
    services.AddSingleton<IActiveSessionService, ActiveSessionService>();

    // Register authentication events service
    services.AddScoped<AuthenticationEventsService>();

    // Configure authentication events
    services.ConfigureApplicationCookie(options =>
    {
      var serviceProvider = services.BuildServiceProvider();
      var authEventsService = serviceProvider.GetRequiredService<AuthenticationEventsService>();

      options.Events.OnSignedIn = authEventsService.OnSignedIn;
      options.Events.OnSigningOut = authEventsService.OnSignedOut;
    });
  }

  private static void AddEmailServices(IServiceCollection services)
  {
    services.AddScoped<IEmailSender, AzureEmailSender>();
    services.AddSingleton<EmailTemplateService>();
  }
}
