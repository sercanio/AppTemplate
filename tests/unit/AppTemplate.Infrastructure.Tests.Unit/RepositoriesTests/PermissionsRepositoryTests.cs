using AppTemplate.Application.Services.Clock;
using AppTemplate.Domain.Roles;
using AppTemplate.Infrastructure.Repositories;
using Ardalis.Result;
using Microsoft.EntityFrameworkCore;

namespace AppTemplate.Infrastructure.Tests.Unit.RepositoriesTests;

[Trait("Category", "Unit")]
public class PermissionsRepositoryTests
{
  private ApplicationDbContext CreateDbContext()
  {
    var options = new DbContextOptionsBuilder<ApplicationDbContext>()
        .UseInMemoryDatabase(Guid.NewGuid().ToString())
        .Options;
    var dateTimeProvider = new DateTimeProvider();
    return new ApplicationDbContext(options, dateTimeProvider);
  }

  [Fact]
  public async Task GetAllPermissionsAsync_ReturnsEmpty_WhenNoPermissionsExist()
  {
    var dbContext = CreateDbContext();
    var repo = new PermissionsRepository(dbContext);

    var result = await repo.GetAllPermissionsAsync(0, 10);

    Assert.Equal(ResultStatus.Ok, result.Status);
    Assert.NotNull(result.Value);
    Assert.Empty(result.Value.Items);
  }

  [Fact]
  public async Task GetAllPermissionsAsync_ReturnsAllPermissions()
  {
    var dbContext = CreateDbContext();
    var permission1 = new Permission(Guid.NewGuid(), "users", "users:read");
    var permission2 = new Permission(Guid.NewGuid(), "roles", "roles:read");
    dbContext.Permissions.Add(permission1);
    dbContext.Permissions.Add(permission2);
    await dbContext.SaveChangesAsync();

    var repo = new PermissionsRepository(dbContext);

    var result = await repo.GetAllPermissionsAsync(0, 10);

    Assert.Equal(ResultStatus.Ok, result.Status);
    Assert.NotNull(result.Value);
    Assert.True(result.Value.Items.Count >= 2);
    Assert.Contains(result.Value.Items, p => p.Name == "users:read");
    Assert.Contains(result.Value.Items, p => p.Name == "roles:read");
  }
}
