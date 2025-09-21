using AppTemplate.Application.Services.Clock;
using AppTemplate.Domain.OutboxMessages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.PostgreSql;
using Xunit;

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
        options.UseNpgsql(_pgContainer.GetConnectionString()));

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

    dbContext.OutboxMessages.Add(outboxMessage);
    await dbContext.SaveChangesAsync();

    var loaded = await dbContext.OutboxMessages.FirstOrDefaultAsync(m => m.Id == messageId);
    Assert.NotNull(loaded);
    Assert.Equal(type, loaded.Type);
    Assert.Equal(content, loaded.Content);
  }

  [Fact]
  public async Task CanInsertTestOutboxMessage()
  {
    using var scope = _provider.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

    // Insert a test outbox message
    var outboxMessage = new OutboxMessage(
        Guid.NewGuid(),
        DateTime.UtcNow,
        "TestType",
        "{\"test\":true}"
    );
    dbContext.OutboxMessages.Add(outboxMessage);
    await dbContext.SaveChangesAsync();
  }
}
