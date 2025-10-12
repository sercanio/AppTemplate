using AppTemplate.Application.Services.Clock;
using AppTemplate.Domain.Roles;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.PostgreSql;
using Xunit;

namespace AppTemplate.Infrastructure.Tests.Integration.Configurations;

[Trait("Category", "Integration")]
public class RoleConfigurationTests : IAsyncLifetime
{
    private readonly PostgreSqlContainer _pgContainer;
    private ServiceProvider _provider;

    public RoleConfigurationTests()
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
    public async Task SeededRoles_ShouldExist()
    {
        using var scope = _provider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var adminRole = await dbContext.Roles.FirstOrDefaultAsync(r => r.Id == Role.Admin.Id);
        var defaultRole = await dbContext.Roles.FirstOrDefaultAsync(r => r.Id == Role.DefaultRole.Id);

        Assert.NotNull(adminRole);
        Assert.NotNull(defaultRole);
        Assert.Equal(Role.Admin.Name.Value, adminRole.Name.Value);
        Assert.Equal(Role.DefaultRole.Name.Value, defaultRole.Name.Value);
    }

    [Fact]
    public async Task SeededRolePermissions_ShouldExist()
    {
        using var scope = _provider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var adminRole = await dbContext.Roles
            .Include(r => r.Permissions)
            .FirstOrDefaultAsync(r => r.Id == Role.Admin.Id);

        Assert.NotNull(adminRole);
        Assert.True(adminRole.Permissions.Count > 0);

        // Example: check one known permission
        Assert.Contains(adminRole.Permissions, p => p.Id == Permission.UsersRead.Id);
    }

    [Fact]
    public async Task SeededRoleUsers_ShouldExist()
    {
        using var scope = _provider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var userId = Guid.Parse("55c7f429-0916-4d84-8b76-d45185d89aa7");
        var appUser = await dbContext.AppUsers
            .Include(u => u.Roles)
            .FirstOrDefaultAsync(u => u.Id == userId);

        Assert.NotNull(appUser);
        Assert.Contains(appUser.Roles, r => r.Id == Role.Admin.Id);
        Assert.Contains(appUser.Roles, r => r.Id == Role.DefaultRole.Id);
    }
}
