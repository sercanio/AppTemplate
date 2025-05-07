using AppTemplate.Application.Repositories;
using AppTemplate.Application.Services.AuditLogs;
using AppTemplate.Application.Services.Notifications;
using AppTemplate.Infrastructure.Autorization;
using AppTemplate.Infrastructure.Repositories;
using Asp.Versioning;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Myrtus.Clarity.Core.Application.Abstractions.Authentication;
using Myrtus.Clarity.Core.Application.Abstractions.Clock;
using Myrtus.Clarity.Core.Application.Abstractions.Data.Dapper;
using Myrtus.Clarity.Core.Domain.Abstractions;
using Myrtus.Clarity.Core.Infrastructure.Authentication.Azure;
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
        AddAuditing(services);
        AddPersistence(services, configuration);
        AddAuthorization(services);
        AddNotification(services);
        AddAuditing(services);
        AddSignalR(services);

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
             options.UseNpgsql(connectionString, npgsqlOptions => npgsqlOptions.UseNetTopologySuite()));
        services.AddDatabaseDeveloperPageExceptionFilter();

        services.AddScoped<ISqlConnectionFactory, SqlConnectionFactory>()
            .AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<ApplicationDbContext>())
            .AddScoped<IAppUsersRepository, AppUsersRepository>()
            .AddScoped<IRolesRepository, RolesRepository>()
            .AddScoped<IPermissionsRepository, PermissionsRepository>()
            .AddScoped<IAuditLogsRepository, AuditLogsRepository>()
            .AddScoped<INotificationsRepository, NotificationsRepository>();
    }

    private static void AddAuthorization(IServiceCollection services)
    {
        services.AddScoped<IClaimsTransformation, RoleClaimsTransformation>();
    }
    private static void AddNotification(IServiceCollection services)
    {
        services.AddTransient<INotificationService, NotificationsService>();
    }

    private static void AddAuditing(IServiceCollection services)
    {
        services.AddTransient<IAuditLogService, AuditLogService>();
    }

    private static void AddSignalR(IServiceCollection services)
    {
        services.AddSignalR();
    }
}
