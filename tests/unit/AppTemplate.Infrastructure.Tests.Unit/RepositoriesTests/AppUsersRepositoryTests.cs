using AppTemplate.Application.Data.DynamicQuery;
using AppTemplate.Application.Services.Clock;
using AppTemplate.Domain.AppUsers;
using AppTemplate.Domain.Roles;
using AppTemplate.Infrastructure.Repositories;
using Ardalis.Result;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace AppTemplate.Infrastructure.Tests.Unit.RepositoriesTests;

[Trait("Category", "Unit")]
public class AppUsersRepositoryTests
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
  public async Task GetUserByIdWithIdentityAndRrolesAsync_ReturnsNotFound_WhenUserIsNull()
  {
    var dbContext = CreateDbContext();
    var repo = new AppUsersRepository(dbContext);

    var result = await repo.GetUserByIdWithIdentityAndRrolesAsync(Guid.NewGuid());

    Assert.Equal(ResultStatus.NotFound, result.Status);
    Assert.Contains("User not found.", result.Errors);
  }

  [Fact]
  public async Task GetUserByIdWithIdentityAndRrolesAsync_ReturnsSuccess_WhenUserIsFound()
  {
    var dbContext = CreateDbContext();

    var identityUser = new IdentityUser { Id = "test-identity-id", UserName = "testuser", Email = "testuser@example.com" };
    dbContext.Users.Add(identityUser);

    var user = AppUser.Create();
    user.SetIdentityId(identityUser.Id);
    dbContext.AppUsers.Add(user);

    await dbContext.SaveChangesAsync();

    var repo = new AppUsersRepository(dbContext);

    var result = await repo.GetUserByIdWithIdentityAndRrolesAsync(user.Id);

    Assert.Equal(ResultStatus.Ok, result.Status);
    Assert.NotNull(result.Value);
    Assert.Equal(user.Id, result.Value.Id);
  }

  [Fact]
  public async Task GetUserByIdentityIdWithIdentityAndRolesAsync_ReturnsNotFound_WhenUserIsNull()
  {
    var dbContext = CreateDbContext();
    var repo = new AppUsersRepository(dbContext);

    var result = await repo.GetUserByIdentityIdWithIdentityAndRolesAsync("non-existent-id");

    Assert.Equal(ResultStatus.NotFound, result.Status);
    Assert.Contains("User not found.", result.Errors);
  }

  [Fact]
  public async Task GetUserByIdentityIdWithIdentityAndRolesAsync_ReturnsSuccess_WhenUserIsFound()
  {
    var dbContext = CreateDbContext();

    var identityUser = new IdentityUser { Id = "identity-123", UserName = "user123", Email = "user123@example.com" };
    dbContext.Users.Add(identityUser);

    var user = AppUser.Create();
    user.SetIdentityId(identityUser.Id);
    dbContext.AppUsers.Add(user);

    await dbContext.SaveChangesAsync();

    var repo = new AppUsersRepository(dbContext);

    var result = await repo.GetUserByIdentityIdWithIdentityAndRolesAsync(identityUser.Id);

    Assert.Equal(ResultStatus.Ok, result.Status);
    Assert.NotNull(result.Value);
    Assert.Equal(user.Id, result.Value.Id);
  }

  [Fact]
  public async Task GetAllUsersWithIdentityAndRolesAsync_ReturnsPaginatedList()
  {
    var dbContext = CreateDbContext();

    var identityUser1 = new IdentityUser { Id = "id-1", UserName = "user1", Email = "user1@example.com" };
    var identityUser2 = new IdentityUser { Id = "id-2", UserName = "user2", Email = "user2@example.com" };
    dbContext.Users.AddRange(identityUser1, identityUser2);

    var user1 = AppUser.Create();
    user1.SetIdentityId(identityUser1.Id);
    var user2 = AppUser.Create();
    user2.SetIdentityId(identityUser2.Id);
    dbContext.AppUsers.AddRange(user1, user2);

    await dbContext.SaveChangesAsync();

    var repo = new AppUsersRepository(dbContext);

    var result = await repo.GetAllUsersWithIdentityAndRolesAsync(0, 10);

    Assert.Equal(ResultStatus.Ok, result.Status);
    Assert.NotNull(result.Value);
    Assert.Equal(2, result.Value.TotalCount);
    Assert.Equal(2, result.Value.Items.Count);
  }

  [Fact]
  public async Task GetAllUsersByRoleIdWithIdentityAndRolesAsync_ReturnsUsersWithRole()
  {
    var dbContext = CreateDbContext();

    var identityUser = new IdentityUser { Id = "id-1", UserName = "user1", Email = "user1@example.com" };
    dbContext.Users.Add(identityUser);

    var role = Role.Create("TestRole", "Test Role", Guid.NewGuid());
    dbContext.Roles.Add(role);

    var user = AppUser.Create();
    user.SetIdentityId(identityUser.Id);
    user.AddRole(role);
    dbContext.AppUsers.Add(user);

    await dbContext.SaveChangesAsync();

    var repo = new AppUsersRepository(dbContext);

    var result = await repo.GetAllUsersByRoleIdWithIdentityAndRolesAsync(role.Id, 0, 10);

    Assert.Equal(ResultStatus.Ok, result.Status);
    Assert.NotNull(result.Value);
    Assert.Single(result.Value.Items);
    Assert.Equal(user.Id, result.Value.Items[0].Id);
  }

  [Fact]
  public async Task GetAllUsersDynamicWithIdentityAndRolesAsync_ReturnsFilteredUsers()
  {
    var dbContext = CreateDbContext();

    var identityUser1 = new IdentityUser { Id = "id-1", UserName = "user1", Email = "user1@example.com" };
    var identityUser2 = new IdentityUser { Id = "id-2", UserName = "user2", Email = "user2@example.com" };
    dbContext.Users.AddRange(identityUser1, identityUser2);

    var user1 = AppUser.Create();
    user1.SetIdentityId(identityUser1.Id);
    var user2 = AppUser.Create();
    user2.SetIdentityId(identityUser2.Id);
    dbContext.AppUsers.AddRange(user1, user2);

    await dbContext.SaveChangesAsync();

    var repo = new AppUsersRepository(dbContext);

    var dynamicQuery = new DynamicQuery
    {
      Filter = new Filter
      {
        Field = "IdentityId",
        Operator = "eq",
        Value = "id-1"
      }
    };

    var result = await repo.GetAllUsersDynamicWithIdentityAndRolesAsync(dynamicQuery, 0, 10);

    Assert.Equal(ResultStatus.Ok, result.Status);
    Assert.NotNull(result.Value);
    Assert.Single(result.Value.Items);
    Assert.Equal(user1.Id, result.Value.Items[0].Id);
  }

  [Fact]
  public async Task GetUserWithRolesAndIdentityByIdAsync_ReturnsNotFound_WhenUserIsNull()
  {
    var dbContext = CreateDbContext();
    var repo = new AppUsersRepository(dbContext);

    var result = await repo.GetUserWithRolesAndIdentityByIdAsync(Guid.NewGuid());

    Assert.Equal(ResultStatus.NotFound, result.Status);
    Assert.Contains("User not found.", result.Errors);
  }

  [Fact]
  public async Task GetUserWithRolesAndIdentityByIdAsync_ReturnsSuccess_WhenUserIsFound()
  {
    var dbContext = CreateDbContext();

    var identityUser = new IdentityUser { Id = "id-1", UserName = "user1", Email = "user1@example.com" };
    dbContext.Users.Add(identityUser);

    var user = AppUser.Create();
    user.SetIdentityId(identityUser.Id);
    dbContext.AppUsers.Add(user);

    await dbContext.SaveChangesAsync();

    var repo = new AppUsersRepository(dbContext);

    var result = await repo.GetUserWithRolesAndIdentityByIdAsync(user.Id);

    Assert.Equal(ResultStatus.Ok, result.Status);
    Assert.NotNull(result.Value);
    Assert.Equal(user.Id, result.Value.Id);
  }

  [Fact]
  public async Task GetUserWithRolesAndIdentityByIdentityIdAsync_ReturnsNotFound_WhenUserIsNull()
  {
    var dbContext = CreateDbContext();
    var repo = new AppUsersRepository(dbContext);

    var result = await repo.GetUserWithRolesAndIdentityByIdentityIdAsync("non-existent-id");

    Assert.Equal(ResultStatus.NotFound, result.Status);
    Assert.Contains("User not found.", result.Errors);
  }

  [Fact]
  public async Task GetUserWithRolesAndIdentityByIdentityIdAsync_ReturnsSuccess_WhenUserIsFound()
  {
    var dbContext = CreateDbContext();

    var identityUser = new IdentityUser { Id = "id-1", UserName = "user1", Email = "user1@example.com" };
    dbContext.Users.Add(identityUser);

    var user = AppUser.Create();
    user.SetIdentityId(identityUser.Id);
    dbContext.AppUsers.Add(user);

    await dbContext.SaveChangesAsync();

    var repo = new AppUsersRepository(dbContext);

    var result = await repo.GetUserWithRolesAndIdentityByIdentityIdAsync(identityUser.Id);

    Assert.Equal(ResultStatus.Ok, result.Status);
    Assert.NotNull(result.Value);
    Assert.Equal(user.Id, result.Value.Id);
  }

  [Fact]
  public async Task GetUsersCountAsync_ReturnsCount()
  {
    var dbContext = CreateDbContext();

    var user1 = AppUser.Create();
    user1.SetIdentityId("id-1");
    dbContext.AppUsers.Add(user1);

    var user2 = AppUser.Create();
    user2.SetIdentityId("id-2");
    dbContext.AppUsers.Add(user2);

    await dbContext.SaveChangesAsync();

    var repo = new AppUsersRepository(dbContext);

    var count = await repo.GetUsersCountAsync();

    Assert.Equal(2, count);
  }

  [Fact]
  public async Task AddAsync_AddsUser()
  {
    var dbContext = CreateDbContext();
    var repo = new AppUsersRepository(dbContext);

    var user = AppUser.Create();
    user.SetIdentityId("id-1");

    await repo.AddAsync(user);
    await dbContext.SaveChangesAsync();

    var savedUser = await dbContext.AppUsers.FirstOrDefaultAsync(u => u.Id == user.Id);
    Assert.NotNull(savedUser);
    Assert.Equal("id-1", savedUser.IdentityId);
  }

  [Fact]
  public async Task Update_UpdatesUser()
  {
    var dbContext = CreateDbContext();
    var repo = new AppUsersRepository(dbContext);

    var user = AppUser.Create();
    user.SetIdentityId("id-1");
    dbContext.AppUsers.Add(user);
    await dbContext.SaveChangesAsync();

    user.SetIdentityId("id-2");
    repo.Update(user);
    await dbContext.SaveChangesAsync();

    var updatedUser = await dbContext.AppUsers.FirstOrDefaultAsync(u => u.Id == user.Id);
    Assert.NotNull(updatedUser);
    Assert.Equal("id-2", updatedUser.IdentityId);
  }

  [Fact]
  public async Task Delete_DeletesUser()
  {
    var dbContext = CreateDbContext();
    var repo = new AppUsersRepository(dbContext);

    var user = AppUser.Create();
    user.SetIdentityId("id-1");
    dbContext.AppUsers.Add(user);
    await dbContext.SaveChangesAsync();

    repo.Delete(user, isSoftDelete: false);
    await dbContext.SaveChangesAsync();

    var deletedUser = await dbContext.AppUsers.FirstOrDefaultAsync(u => u.Id == user.Id);
    Assert.Null(deletedUser);
  }
}
