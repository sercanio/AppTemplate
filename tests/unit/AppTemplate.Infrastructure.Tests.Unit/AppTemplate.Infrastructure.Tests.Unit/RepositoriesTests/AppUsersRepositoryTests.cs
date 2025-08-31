using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using AppTemplate.Core.Infrastructure.Clock;
using AppTemplate.Domain.AppUsers;
using AppTemplate.Infrastructure.Repositories;

namespace AppTemplate.Infrastructure.Tests.Unit.RepositoriesTests;

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

    Assert.Equal(Ardalis.Result.ResultStatus.NotFound, result.Status);
    Assert.Contains("User not found.", result.Errors);
  }

  [Fact]
  public async Task GetUserByIdWithIdentityAndRrolesAsync_ReturnsSuccess_WhenUserIsFound()
  {
    var dbContext = CreateDbContext();

    // Create and add IdentityUser
    var identityUser = new IdentityUser { Id = "test-identity-id", UserName = "testuser" };
    dbContext.Users.Add(identityUser);

    var user = AppUser.Create();
    user.SetIdentityId(identityUser.Id);
    dbContext.AppUsers.Add(user);

    await dbContext.SaveChangesAsync();

    var repo = new AppUsersRepository(dbContext);

    var result = await repo.GetUserByIdWithIdentityAndRrolesAsync(user.Id);

    Assert.Equal(Ardalis.Result.ResultStatus.Ok, result.Status);
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
}
