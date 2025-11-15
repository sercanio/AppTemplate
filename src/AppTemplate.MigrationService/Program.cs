using AppTemplate.Application.Services.Clock;
using AppTemplate.Infrastructure;
using AppTemplate.MigrationService;
using Microsoft.EntityFrameworkCore.Diagnostics;

var builder = Host.CreateApplicationBuilder(args);

// Add the hosted service
builder.Services.AddHostedService<Worker>();

// Add required services for the DbContext
builder.Services.AddTransient<IDateTimeProvider, DateTimeProvider>();

// Add the DbContext with PostgreSQL connection but disable pooling for migrations
builder.AddNpgsqlDbContext<ApplicationDbContext>("Database", configureDbContextOptions: options =>
{
  // Disable pooling for the migration service
  options.EnableServiceProviderCaching(false);
  options.EnableSensitiveDataLogging(builder.Environment.IsDevelopment());

  // Ignore the pending model changes warning specifically for migration service
  options.ConfigureWarnings(warnings =>
      warnings.Ignore(RelationalEventId.PendingModelChangesWarning));
}, configureSettings: settings =>
{
  // Increase command timeout for migrations
  settings.CommandTimeout = 120; // 2 minutes
});

var host = builder.Build();
host.Run();
