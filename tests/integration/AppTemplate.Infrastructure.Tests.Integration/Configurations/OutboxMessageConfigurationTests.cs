using AppTemplate.Application.Services.Clock;
using AppTemplate.Domain.OutboxMessages;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.PostgreSql;
using Xunit;
using System.Text.Json;

namespace AppTemplate.Infrastructure.Tests.Integration.Configurations;

[Trait("Category", "Integration")]
public class OutboxMessageConfigurationTests : IAsyncLifetime
{
  private readonly PostgreSqlContainer _pgContainer;
  private ServiceProvider _provider;

  public OutboxMessageConfigurationTests()
  {
    _pgContainer = new PostgreSqlBuilder()
        .WithDatabase("testdb")
        .WithUsername("testuser")
        .WithPassword("testpass")
        .Build();
  }

  public async Task InitializeAsync()
  {
    await _pgContainer.StartAsync();

    var services = new ServiceCollection();
    services.AddSingleton<IDateTimeProvider, DateTimeProvider>();
    services.AddDbContext<ApplicationDbContext>(options =>
    {
      options.UseNpgsql(_pgContainer.GetConnectionString());
      // Suppress the pending model changes warning for tests
      options.ConfigureWarnings(warnings => 
      {
        warnings.Ignore(RelationalEventId.PendingModelChangesWarning);
      });
    });

    _provider = services.BuildServiceProvider();

    using var scope = _provider.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await dbContext.Database.MigrateAsync();
  }

  public async Task DisposeAsync()
  {
    await _pgContainer.DisposeAsync();
    if (_provider is not null)
      await _provider.DisposeAsync();
  }

  [Fact]
  public async Task CanInsertAndRetrieveOutboxMessage_WithJsonContent()
  {
    // Arrange
    using var scope = _provider.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

    var messageId = Guid.NewGuid();
    var now = DateTime.UtcNow;
    var type = "TestEvent";
    var content = "{\"foo\": \"bar\", \"baz\": 123}";

    var outboxMessage = new OutboxMessage(
        messageId,
        now,
        type,
        content
    );

    // Act
    dbContext.OutboxMessages.Add(outboxMessage);
    var saveResult = await dbContext.SaveChangesAsync();

    // Assert
    Assert.True(saveResult > 0, "SaveChanges should return > 0");

    // Clear change tracker to ensure fresh load
    dbContext.ChangeTracker.Clear();

    var loaded = await dbContext.OutboxMessages
        .FirstOrDefaultAsync(m => m.Id == messageId);

    Assert.NotNull(loaded);
    Assert.Equal(messageId, loaded.Id);
    Assert.Equal(type, loaded.Type);
    
    // Parse and compare JSON content instead of raw strings
    var expectedJson = JsonDocument.Parse(content);
    var actualJson = JsonDocument.Parse(loaded.Content);
    
    // Compare specific properties
    Assert.Equal(expectedJson.RootElement.GetProperty("foo").GetString(), 
                 actualJson.RootElement.GetProperty("foo").GetString());
    Assert.Equal(expectedJson.RootElement.GetProperty("baz").GetInt32(), 
                 actualJson.RootElement.GetProperty("baz").GetInt32());
    
    Assert.Equal(now.ToString("yyyy-MM-ddTHH:mm:ss"), loaded.OccurredOnUtc.ToString("yyyy-MM-ddTHH:mm:ss"));
    Assert.Null(loaded.ProcessedOnUtc);
    Assert.Null(loaded.Error);
  }

  [Fact]
  public async Task CanInsertTestOutboxMessage()
  {
    // Arrange
    using var scope = _provider.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

    var messageId = Guid.NewGuid();
    var outboxMessage = new OutboxMessage(
        messageId,
        DateTime.UtcNow,
        "TestType",
        "{\"test\":true}"
    );

    // Act & Assert
    dbContext.OutboxMessages.Add(outboxMessage);
    var result = await dbContext.SaveChangesAsync();

    Assert.True(result > 0, "Should save successfully");

    // Verify it was actually saved
    var exists = await dbContext.OutboxMessages
        .AnyAsync(m => m.Id == messageId);
    Assert.True(exists, "Message should exist in database");
  }
}
