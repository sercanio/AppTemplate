using AppTemplate.Application.Services.Clock;
using AppTemplate.Domain.AppUsers;
using AppTemplate.Domain.Notifications;
using AppTemplate.Domain.Notifications.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.PostgreSql;
using Xunit;

namespace AppTemplate.Infrastructure.Tests.Integration.Configurations;

[Trait("Category", "Integration")]
public class NotificationConfigurationTests : IAsyncLifetime
{
    private readonly PostgreSqlContainer _pgContainer;
    private ServiceProvider _provider;

    public NotificationConfigurationTests()
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
        await dbContext.Database.EnsureCreatedAsync();

        // Seed a dummy IdentityUser before AppUser
        var identityUser = new IdentityUser
        {
            Id = "test-identity-id",
            UserName = "testuser",
            Email = "testuser@example.com",
            NormalizedUserName = "TESTUSER",
            NormalizedEmail = "TESTUSER@EXAMPLE.COM",
            EmailConfirmed = true,
            SecurityStamp = Guid.NewGuid().ToString("D")
        };
        dbContext.Users.Add(identityUser);
        await dbContext.SaveChangesAsync();

        // Now seed AppUser referencing the above IdentityUser
        var user = AppUser.Create();
        user.SetIdentityId(identityUser.Id);
        dbContext.AppUsers.Add(user);
        await dbContext.SaveChangesAsync();
    }

    public async Task DisposeAsync()
    {
        await _pgContainer.DisposeAsync();
        if (_provider is not null)
            await _provider.DisposeAsync();
    }

    [Fact]
    public async Task Notification_RequiredProperties_And_MaxLengths_AreEnforced()
    {
        using var scope = _provider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var user = await dbContext.AppUsers.FirstAsync();

        // Title and Message required
        var notification = new Notification(
            recipientId: user.Id,
            title: null!,
            message: "msg",
            type: NotificationTypeEnum.System);

        dbContext.Notifications.Add(notification);
        await Assert.ThrowsAsync<DbUpdateException>(async () => await dbContext.SaveChangesAsync());

        notification = new Notification(
            recipientId: user.Id,
            title: "title",
            message: null!,
            type: NotificationTypeEnum.System);

        dbContext.Notifications.Add(notification);
        await Assert.ThrowsAsync<DbUpdateException>(async () => await dbContext.SaveChangesAsync());

        // Title max length
        var longTitle = new string('a', 257);
        notification = new Notification(
            recipientId: user.Id,
            title: longTitle,
            message: "msg",
            type: NotificationTypeEnum.System);

        dbContext.Notifications.Add(notification);
        await Assert.ThrowsAsync<DbUpdateException>(async () => await dbContext.SaveChangesAsync());

        // Message max length
        var longMessage = new string('a', 1001);
        notification = new Notification(
            recipientId: user.Id,
            title: "title",
            message: longMessage,
            type: NotificationTypeEnum.System);

        dbContext.Notifications.Add(notification);
        await Assert.ThrowsAsync<DbUpdateException>(async () => await dbContext.SaveChangesAsync());
    }

    [Fact]
    public async Task Notification_NavigationProperty_And_CascadeDelete_Works()
    {
        using var scope = _provider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var user = await dbContext.AppUsers.FirstAsync();

        var notification = new Notification(
            recipientId: user.Id,
            title: "title",
            message: "msg",
            type: NotificationTypeEnum.System);

        dbContext.Notifications.Add(notification);
        await dbContext.SaveChangesAsync();

        // Confirm navigation property
        var loaded = await dbContext.Notifications.Include(n => n.Recipient).FirstOrDefaultAsync(n => n.Id == notification.Id);
        Assert.NotNull(loaded);
        Assert.NotNull(loaded.Recipient);
        Assert.Equal(user.Id, loaded.Recipient.Id);

        // Cascade delete: deleting user deletes notification
        dbContext.AppUsers.Remove(user);
        await dbContext.SaveChangesAsync();

        var deletedNotification = await dbContext.Notifications.FirstOrDefaultAsync(n => n.Id == notification.Id);
        Assert.Null(deletedNotification);
    }

    [Fact]
    public async Task Notification_QueryFilter_ExcludesSoftDeleted()
    {
        using var scope = _provider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        // Before adding AppUser with identityId "another-identity-id"
        var identityUser = new IdentityUser
        {
            Id = "another-identity-id",
            UserName = "anotheruser",
            Email = "anotheruser@example.com",
            NormalizedUserName = "ANOTHERUSER",
            NormalizedEmail = "ANOTHERUSER@EXAMPLE.COM",
            EmailConfirmed = true,
            SecurityStamp = Guid.NewGuid().ToString("D")
        };
        dbContext.Users.Add(identityUser);
        await dbContext.SaveChangesAsync();

        var user = AppUser.Create();
        user.SetIdentityId(identityUser.Id);
        dbContext.AppUsers.Add(user);
        await dbContext.SaveChangesAsync();

        var notification = new Notification(
            recipientId: user.Id,
            title: "title",
            message: "msg",
            type: NotificationTypeEnum.System);

        dbContext.Notifications.Add(notification);
        await dbContext.SaveChangesAsync();

        // Soft delete by setting DeletedOnUtc
        var entry = dbContext.Entry(notification);
        entry.Property("DeletedOnUtc").CurrentValue = DateTime.UtcNow;
        await dbContext.SaveChangesAsync();

        // Should not be returned by default query
        var found = await dbContext.Notifications.FirstOrDefaultAsync(n => n.Id == notification.Id);
        Assert.Null(found);

        // Should be returned if query filter is ignored
        var all = await dbContext.Notifications.IgnoreQueryFilters().FirstOrDefaultAsync(n => n.Id == notification.Id);
        Assert.NotNull(all);
    }

    [Fact]
    public async Task Notification_Indexes_AreCreated()
    {
        using var scope = _provider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var conn = dbContext.Database.GetDbConnection();
        await conn.OpenAsync();

        using var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            SELECT COUNT(*) FROM pg_indexes 
            WHERE tablename = '""Notifications""' OR tablename = 'Notifications';
        ";
        var result = await cmd.ExecuteScalarAsync();
        int indexCount = Convert.ToInt32(result);

        Assert.True(indexCount > 0, "Expected at least one index on Notifications table.");
    }
}
