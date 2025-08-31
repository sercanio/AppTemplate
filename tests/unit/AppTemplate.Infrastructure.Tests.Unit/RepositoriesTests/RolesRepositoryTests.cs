using Ardalis.Result;
using Microsoft.EntityFrameworkCore;
using AppTemplate.Domain.Roles;
using AppTemplate.Infrastructure.Repositories;
using AppTemplate.Core.Infrastructure.Clock;

namespace AppTemplate.Infrastructure.Tests.Unit.RepositoriesTests;

[Trait("Category", "Unit")]
public class RolesRepositoryTests
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
  public async Task GetRoleByIdWithPermissionsAsync_ReturnsNotFound_WhenRoleIsNull()
  {
    var dbContext = CreateDbContext();
    var repo = new RolesRepository(dbContext);

    var result = await repo.GetRoleByIdWithPermissionsAsync(Guid.NewGuid());

    Assert.Equal(ResultStatus.NotFound, result.Status);
    Assert.Contains("Role not found.", result.Errors);
  }

  [Fact]
  public async Task GetRoleByIdWithPermissionsAsync_ReturnsSuccess_WhenRoleIsFound()
  {
    var dbContext = CreateDbContext();
    var role = Role.Create("Admin", "Admin", Guid.NewGuid());
    role.AddPermission(Permission.RolesRead, Guid.NewGuid());
    dbContext.Roles.Add(role);
    await dbContext.SaveChangesAsync();

    var repo = new RolesRepository(dbContext);

    var result = await repo.GetRoleByIdWithPermissionsAsync(role.Id);

    Assert.Equal(ResultStatus.Ok, result.Status);
    Assert.NotNull(result.Value);
    Assert.Equal(role.Id, result.Value.Id);
    Assert.Contains(result.Value.Permissions, p => p.Name == Permission.RolesRead.Name);
  }

  [Fact]
  public async Task GetAllRolesAsync_ReturnsAllRoles()
  {
    var dbContext = CreateDbContext();
    var role1 = Role.Create("Admin", "Admin", Guid.NewGuid());
    var role2 = Role.Create("User", "User", Guid.NewGuid());
    dbContext.Roles.Add(role1);
    dbContext.Roles.Add(role2);
    await dbContext.SaveChangesAsync();

    var repo = new RolesRepository(dbContext);

    var result = await repo.GetAllRolesAsync(0, 10);

    Assert.Equal(ResultStatus.Ok, result.Status);
    Assert.NotNull(result.Value);
    Assert.True(result.Value.Items.Count >= 2);
    Assert.Contains(result.Value.Items, r => r.Name.Value == "Admin");
    Assert.Contains(result.Value.Items, r => r.Name.Value == "User");
  }
}