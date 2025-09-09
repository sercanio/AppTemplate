using AppTemplate.Core.Application.Abstractions.Clock;
using AppTemplate.Core.Infrastructure.Clock;
using AppTemplate.Domain.Roles;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.PostgreSql;
using Xunit;

namespace AppTemplate.Infrastructure.Tests.Integration.Configurations;

[Trait("Category", "Integration")]
public class PermissionConfigurationTests : IAsyncLifetime
{
    private readonly PostgreSqlContainer _pgContainer;
    private ServiceProvider _provider;

    public PermissionConfigurationTests()
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
    }

    public async Task DisposeAsync()
    {
        await _pgContainer.DisposeAsync();
        if (_provider is not null)
            await _provider.DisposeAsync();
    }

    [Fact]
    public async Task AllSeededPermissions_ShouldExistWithCorrectProperties()
    {
        using var scope = _provider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var allDomainPermissions = new[]
        {
            Permission.UsersAdmin,
            Permission.UsersRead,
            Permission.UsersCreate,
            Permission.UsersUpdate,
            Permission.UsersDelete,
            Permission.RolesAdmin,
            Permission.RolesRead,
            Permission.RolesCreate,
            Permission.RolesUpdate,
            Permission.RolesDelete,
            Permission.PermissionsRead,
            Permission.AuditLogsRead,
            Permission.NotificationsRead,
            Permission.NotificationsUpdate,
            Permission.StatisticsRead,
            Permission.UserFollowsRead,
            Permission.UserFollowsCreate,
            Permission.UserFollowsUpdate,
            Permission.UserFollowsDelete,
            Permission.EntriesAdmin,
            Permission.EntriesRead,
            Permission.EntriesCreate,
            Permission.EntriesUpdate,
            Permission.EntriesDelete,
            Permission.TitlesAdmin,
            Permission.TitlesRead,
            Permission.TitlesCreate,
            Permission.TitlesUpdate,
            Permission.TitlesDelete,
            Permission.TitleFollowsRead,
            Permission.TitleFollowsCreate,
            Permission.TitleFollowsUpdate,
            Permission.TitleFollowsDelete,
            Permission.EntryLikesRead,
            Permission.EntryLikesCreate,
            Permission.EntryLikesUpdate,
            Permission.EntryLikesDelete,
            Permission.EntryBookmarksRead,
            Permission.EntryBookmarksCreate,
            Permission.EntryBookmarksUpdate,
            Permission.EntryBookmarksDelete,
            Permission.EntryReportsAdmin,
            Permission.EntryReportsRead,
            Permission.EntryReportsCreate,
            Permission.EntryReportsUpdate,
            Permission.EntryReportsDelete,
            Permission.FeaturedEntriesAdmin,
            Permission.FeaturedEntriesRead,
            Permission.FeaturedEntriesCreate,
            Permission.FeaturedEntriesUpdate,
            Permission.FeaturedEntriesDelete
        };

        foreach (var domainPermission in allDomainPermissions)
        {
            var dbPermission = await dbContext.Permissions
                .FirstOrDefaultAsync(p => p.Id == domainPermission.Id);

            Assert.NotNull(dbPermission);
            Assert.Equal(domainPermission.Feature, dbPermission.Feature);
            Assert.Equal(domainPermission.Name, dbPermission.Name);
        }

        // Also check total count matches
        var dbCount = await dbContext.Permissions.CountAsync();
        Assert.Equal(allDomainPermissions.Length, dbCount);
    }
}
