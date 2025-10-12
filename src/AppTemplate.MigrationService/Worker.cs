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
            hostApplicationLifetime.StopApplication();
        }
    }

    private async Task ApplyMigrationsAsync(ApplicationDbContext dbContext, CancellationToken cancellationToken)
    {
        logger.LogInformation("Checking for pending migrations...");
        
        // Check if database exists
        var canConnect = await dbContext.Database.CanConnectAsync(cancellationToken);
        if (!canConnect)
        {
            logger.LogInformation("Database does not exist. It will be created during migration.");
        }

        // Check if migration history table exists
        var migrationHistoryExists = await CheckMigrationHistoryExistsAsync(dbContext, cancellationToken);
        
        if (!migrationHistoryExists)
        {
            logger.LogInformation("Migration history table does not exist. Creating database schema...");
            
            // Create the database schema using EnsureCreated
            await dbContext.Database.EnsureCreatedAsync(cancellationToken);
            logger.LogInformation("Database schema created using EnsureCreated.");
            
            // Now we need to properly create the migration history table and mark migrations as applied
            await CreateMigrationHistoryAndMarkAppliedAsync(dbContext, cancellationToken);
            
            logger.LogInformation("Migration history setup completed.");
            return;
        }

        // Normal migration flow - if migration history exists, apply pending migrations
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

    private async Task<bool> CheckMigrationHistoryExistsAsync(ApplicationDbContext dbContext, CancellationToken cancellationToken)
    {
        try
        {
            await dbContext.Database.ExecuteSqlRawAsync(
                "SELECT 1 FROM \"__EFMigrationsHistory\" LIMIT 1", 
                cancellationToken);
            return true;
        }
        catch
        {
            return false;
        }
    }

    private async Task CreateMigrationHistoryAndMarkAppliedAsync(ApplicationDbContext dbContext, CancellationToken cancellationToken)
    {
        try
        {
            // First, ensure the migrations history table exists by calling MigrateAsync with no pending migrations
            // This will create the __EFMigrationsHistory table if it doesn't exist
            logger.LogInformation("Ensuring migration history table exists...");
            
            // Create the migration history table structure
            await dbContext.Database.ExecuteSqlRawAsync(@"
                CREATE TABLE IF NOT EXISTS ""__EFMigrationsHistory"" (
                    ""MigrationId"" character varying(150) NOT NULL,
                    ""ProductVersion"" character varying(32) NOT NULL,
                    CONSTRAINT ""PK___EFMigrationsHistory"" PRIMARY KEY (""MigrationId"")
                )", cancellationToken);

            // Get all migrations and mark them as applied
            var allMigrations = dbContext.Database.GetMigrations();
            foreach (var migration in allMigrations)
            {
                logger.LogInformation("Marking migration as applied: {Migration}", migration);
                await dbContext.Database.ExecuteSqlRawAsync(
                    "INSERT INTO \"__EFMigrationsHistory\" (\"MigrationId\", \"ProductVersion\") VALUES ({0}, {1}) ON CONFLICT (\"MigrationId\") DO NOTHING",
                    migration, "9.0.9");
            }
            
            logger.LogInformation("Marked {Count} migrations as applied.", allMigrations.Count());
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating migration history");
            throw;
        }
    }
}
