using AppTemplate.Application.Repositories;
using AppTemplate.Application.Services.Authentication;
using AppTemplate.Application.Services.EmailSenders;
using AppTemplate.Application.Services.Notifications;
using AppTemplate.Application.Services.Statistics;
using AppTemplate.Infrastructure.Authorization;
using AppTemplate.Infrastructure.Autorization;
using AppTemplate.Infrastructure.Repositories;
using Asp.Versioning;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Myrtus.Clarity.Core.Application.Abstractions.Authentication;
using Myrtus.Clarity.Core.Application.Abstractions.Clock;
using Myrtus.Clarity.Core.Application.Abstractions.Data.Dapper;
using Myrtus.Clarity.Core.Domain.Abstractions;
using Myrtus.Clarity.Core.Infrastructure.Authentication.Azure;
using Myrtus.Clarity.Core.Infrastructure.Authorization;
using Myrtus.Clarity.Core.Infrastructure.Clock;
using Myrtus.Clarity.Core.Infrastructure.Data.Dapper;
using Myrtus.Clarity.Core.Infrastructure.Outbox;
using Quartz;

namespace AppTemplate.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
            this IServiceCollection services,
            IConfiguration configuration)
    {
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
        // Get the connection string for PostgresSQL.
        var connectionString = configuration.GetConnectionString("Database")
            ?? throw new InvalidOperationException("Connection string 'Database' not found.");

        // Use Npgsql (PostgresSQL provider) instead of SQLite.
        services.AddDbContext<ApplicationDbContext>(options =>
             options.UseNpgsql(connectionString));
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
        services.AddScoped<AuthorizationService>();

        services.AddTransient<IClaimsTransformation, CustomClaimsTransformation>()
                .AddTransient<IAuthorizationHandler, PermissionAuthorizationHandler>()
                .AddTransient<IAuthorizationPolicyProvider, PermissionAuthorizationPolicyProvider>();
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
