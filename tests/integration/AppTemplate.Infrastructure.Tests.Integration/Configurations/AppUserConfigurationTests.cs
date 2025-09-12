using AppTemplate.Application.Services.Clock;
using AppTemplate.Domain.AppUsers;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Testcontainers.PostgreSql;
using Xunit;

namespace AppTemplate.Infrastructure.Tests.Integration.Configurations;

[Trait("Category", "Integration")]
public class AppUserConfigurationTests : IAsyncLifetime
{
  private readonly PostgreSqlContainer _pgContainer;
  private ApplicationDbContext _dbContext;

  public AppUserConfigurationTests()
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

    _dbContext = CreateDbContext();
    await _dbContext.Database.EnsureCreatedAsync();
  }

  public async Task DisposeAsync()
  {
    await _pgContainer.DisposeAsync();
  }

  private ApplicationDbContext CreateDbContext()
  {
    var options = new DbContextOptionsBuilder<ApplicationDbContext>()
        .UseNpgsql(_pgContainer.GetConnectionString())
        .Options;

    return new ApplicationDbContext(options, new DateTimeProvider());
  }

  [Fact]
  public async Task IdentityId_IsRequired_AndUnique()
  {
    // Setup container and first context
    var dbContext = CreateDbContext();

    var identityUser = new IdentityUser
    {
      Id = "unique-id",
      UserName = "testuser",
      Email = "test@example.com"
    };
    dbContext.Users.Add(identityUser);
    await dbContext.SaveChangesAsync();

    var user1 = AppUser.Create();
    user1.SetIdentityId(identityUser.Id);
    dbContext.AppUsers.Add(user1);
    await dbContext.SaveChangesAsync();

    // Use a new context for the second insert
    var dbContext2 = CreateDbContext();
    var user2 = AppUser.Create();
    user2.SetIdentityId(identityUser.Id);
    dbContext2.AppUsers.Add(user2);

    await Assert.ThrowsAsync<DbUpdateException>(async () => await dbContext2.SaveChangesAsync());
  }

  [Fact]
  public async Task NotificationPreference_IsOwnedType()
  {
    var identityUser = new IdentityUser
    {
      Id = "owned-type-test",
      UserName = "testuser2",
      Email = "test2@example.com"
    };
    _dbContext.Users.Add(identityUser);
    await _dbContext.SaveChangesAsync();

    var user = AppUser.Create();
    user.SetIdentityId(identityUser.Id);
    _dbContext.AppUsers.Add(user);
    await _dbContext.SaveChangesAsync();

    var savedUser = await _dbContext.AppUsers.FirstOrDefaultAsync(u => u.IdentityId == "owned-type-test");
    Assert.NotNull(savedUser);
    Assert.NotNull(savedUser.NotificationPreference);
  }

  [Fact]
  public async Task UserName_IsRequired_AndUnique()
  {
    // Unique test
    var user1 = new IdentityUser { Id = "u1", UserName = "uniqueuser", Email = "user1@example.com" };
    _dbContext.Users.Add(user1);
    await _dbContext.SaveChangesAsync();

    var user2 = new IdentityUser { Id = "u2", UserName = "uniqueuser", Email = "user2@example.com" };
    _dbContext.Users.Add(user2);
    await Assert.ThrowsAsync<DbUpdateException>(async () => await _dbContext.SaveChangesAsync());

    // Required test
    var user3 = new IdentityUser { Id = "u3", UserName = null, Email = "user3@example.com" };
    _dbContext.Users.Add(user3);
    await Assert.ThrowsAsync<DbUpdateException>(async () => await _dbContext.SaveChangesAsync());
  }

  [Fact]
  public async Task Email_IsRequired_AndUnique()
  {
    // Unique test
    var user1 = new IdentityUser { Id = "u4", UserName = "user4", Email = "uniqueemail@example.com" };
    _dbContext.Users.Add(user1);
    await _dbContext.SaveChangesAsync();

    var user2 = new IdentityUser { Id = "u5", UserName = "user5", Email = "uniqueemail@example.com" };
    _dbContext.Users.Add(user2);
    await Assert.ThrowsAsync<DbUpdateException>(async () => await _dbContext.SaveChangesAsync());

    // Required test
    var user3 = new IdentityUser { Id = "u6", UserName = "user6", Email = null };
    _dbContext.Users.Add(user3);
    await Assert.ThrowsAsync<DbUpdateException>(async () => await _dbContext.SaveChangesAsync());
  }
}