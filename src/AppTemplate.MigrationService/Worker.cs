using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using AppTemplate.Infrastructure;

namespace AppTemplate.MigrationService;

public class Worker(
    IServiceProvider serviceProvider,
    IHostApplicationLifetime hostApplicationLifetime,
    ILogger<Worker> logger) : BackgroundService
{
    public const string ActivitySourceName = "Migrations";
    private static readonly ActivitySource s_activitySource = new(ActivitySourceName);

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        using var activity = s_activitySource.StartActivity("Migrating database", ActivityKind.Client);

        try
        {
            using var scope = serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            logger.LogInformation("Starting database migration...");
            
            // Check if database exists, if not create it
            await EnsureDatabaseExistsAsync(dbContext, cancellationToken);
            
            // Apply migrations
            await ApplyMigrationsAsync(dbContext, cancellationToken);
            
            logger.LogInformation("Database migration completed successfully.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while applying database migrations.");
            activity?.AddException(ex);
            throw;
        }
        finally
        {
            // Stop the application after migration is complete
            hostApplicationLifetime.StopApplication();
        }
    }

    private async Task EnsureDatabaseExistsAsync(ApplicationDbContext dbContext, CancellationToken cancellationToken)
    {
        logger.LogInformation("Checking if database exists...");
        
        var canConnect = await dbContext.Database.CanConnectAsync(cancellationToken);
        if (!canConnect)
        {
            logger.LogInformation("Database does not exist. Creating database...");
            await dbContext.Database.EnsureCreatedAsync(cancellationToken);
            logger.LogInformation("Database created successfully.");
        }
        else
        {
            logger.LogInformation("Database already exists.");
        }
    }

    private async Task ApplyMigrationsAsync(ApplicationDbContext dbContext, CancellationToken cancellationToken)
    {
        logger.LogInformation("Checking for pending migrations...");
        
        var pendingMigrations = await dbContext.Database.GetPendingMigrationsAsync(cancellationToken);
        var pendingMigrationsList = pendingMigrations.ToList();
        
        if (pendingMigrationsList.Count != 0)
        {
            logger.LogInformation("Found {Count} pending migrations. Applying them now...", pendingMigrationsList.Count);
            
            foreach (var migration in pendingMigrationsList)
            {
                logger.LogInformation("Applying migration: {Migration}", migration);
            }
            
            await dbContext.Database.MigrateAsync(cancellationToken);
            logger.LogInformation("All migrations applied successfully.");
        }
        else
        {
            logger.LogInformation("No pending migrations found. Database is up to date.");
        }
    }
}
