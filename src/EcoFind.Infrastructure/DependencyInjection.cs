using EcoFind.Application.Repositories;
using EcoFind.Application.Repositories.NoSQL;
using EcoFind.Infrastructure.Autorization;
using EcoFind.Infrastructure.Notifications.Services;
using EcoFind.Infrastructure.Repositories;
using EcoFind.Infrastructure.Repositories.NoSQL;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using Myrtus.Clarity.Core.Application.Abstractions.Auditing;
using Myrtus.Clarity.Core.Application.Abstractions.Authentication;
using Myrtus.Clarity.Core.Application.Abstractions.Clock;
using Myrtus.Clarity.Core.Application.Abstractions.Data.Dapper;
using Myrtus.Clarity.Core.Application.Abstractions.Notification;
using Myrtus.Clarity.Core.Domain.Abstractions;
using Myrtus.Clarity.Core.Infrastructure.Auditing.Services;
using Myrtus.Clarity.Core.Infrastructure.Authentication.Azure;
using Myrtus.Clarity.Core.Infrastructure.Clock;
using Myrtus.Clarity.Core.Infrastructure.Data.Dapper;
using Myrtus.Clarity.Core.Infrastructure.Outbox;
using Quartz;

namespace EcoFind.Infrastructure;

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
            .AddScoped<IPostsRepository, PostsRepository>()
            .AddScoped<IAuditLogRepository, AuditLogRepository>();

        // MongoDB configuration remains the same...
        string mongoConnectionString = configuration.GetConnectionString("MongoDb")
            ?? throw new ArgumentNullException(nameof(configuration));
        string mongoDatabaseName = configuration.GetSection("MongoDb:Database").Value
            ?? throw new ArgumentNullException(nameof(configuration));

        services.AddSingleton<IMongoClient>(_ => new MongoClient(mongoConnectionString))
                .AddSingleton(sp => sp.GetRequiredService<IMongoClient>().GetDatabase(mongoDatabaseName))
                .AddScoped<INoSqlRepository<AuditLog>, NoSqlRepository<AuditLog>>(sp =>
                    new NoSqlRepository<AuditLog>(sp.GetRequiredService<IMongoDatabase>(), "AuditLogs"))
                .AddScoped<INoSqlRepository<Notification>, NoSqlRepository<Notification>>(sp =>
                    new NoSqlRepository<Notification>(sp.GetRequiredService<IMongoDatabase>(), "Notifications"));
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
