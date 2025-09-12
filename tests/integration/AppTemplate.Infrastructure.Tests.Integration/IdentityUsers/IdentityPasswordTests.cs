using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.PostgreSql;
using Xunit;
using AppTemplate.Application.Services.Clock;

namespace AppTemplate.Infrastructure.Tests.Integration.IdentityUsers;

[Trait("Category", "Integration")]
public class IdentityPasswordTests : IAsyncLifetime
{
  private readonly PostgreSqlContainer _pgContainer;
  private ServiceProvider _provider;

  public IdentityPasswordTests()
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

    services.AddDefaultIdentity<IdentityUser>(options =>
    {
      options.Password.RequireDigit = true;
      options.Password.RequiredLength = 8;
      options.Password.RequireNonAlphanumeric = true;
      options.Password.RequireUppercase = true;
      options.Password.RequireLowercase = true;
    })
    .AddEntityFrameworkStores<ApplicationDbContext>();

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
  public async Task Password_Must_Meet_Requirements()
  {
    using var scope = _provider.CreateScope();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();

    var user = new IdentityUser { UserName = "testuser", Email = "test@example.com" };

    // Invalid password (too short, no digit, etc.)
    var result = await userManager.CreateAsync(user, "short");
    Assert.False(result.Succeeded);

    // Valid password
    var validUser = new IdentityUser { UserName = "validuser", Email = "valid@example.com" };
    var validResult = await userManager.CreateAsync(validUser, "Valid123!");
    Assert.True(validResult.Succeeded);
  }

  [Fact]
  public async Task InvalidPassword_ShouldFailUserCreation()
  {
    using var scope = _provider.CreateScope();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();

    var user = new IdentityUser { UserName = "badpassworduser", Email = "badpassword@example.com" };

    // Password does not meet requirements (no digit, too short, no uppercase, etc.)
    var result = await userManager.CreateAsync(user, "short");
    Assert.False(result.Succeeded);
    Assert.Contains(result.Errors, e => e.Description.Contains("Password"));
  }
}
