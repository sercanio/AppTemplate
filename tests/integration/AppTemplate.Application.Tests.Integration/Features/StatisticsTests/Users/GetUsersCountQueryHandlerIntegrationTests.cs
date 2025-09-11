using AppTemplate.Application.Features.Statistics.Users.Queries.GetUsersCount;
using AppTemplate.Domain.AppUsers;
using AppTemplate.Core.Infrastructure.Clock;
using AppTemplate.Infrastructure;
using AppTemplate.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace AppTemplate.Application.Tests.Integration.Features.StatisticsTests.Users;

public class GetUsersCountQueryHandlerIntegrationTests
{
  private ApplicationDbContext CreateDbContext()
  {
    var options = new DbContextOptionsBuilder<ApplicationDbContext>()
        .UseInMemoryDatabase(Guid.NewGuid().ToString())
        .Options;
    return new ApplicationDbContext(options, new DateTimeProvider());
  }

  [Fact]
  public async Task Handle_ReturnsCorrectCount_WhenUsersExist()
  {
    // Arrange
    var dbContext = CreateDbContext();
    var user1 = AppUser.Create();
    var identityId1 = Guid.NewGuid().ToString();
    user1.SetIdentityId(identityId1);
    await dbContext.AddAsync(user1);

    var user2 = AppUser.Create();
    var identityId2 = Guid.NewGuid().ToString();
    user2.SetIdentityId(identityId2);
    await dbContext.AddAsync(user2);

    var user3 = AppUser.Create();
    var identityId3 = Guid.NewGuid().ToString();
    user3.SetIdentityId(identityId3);
    await dbContext.AddAsync(user3);

    await dbContext.SaveChangesAsync();

    var repo = new AppUsersRepository(dbContext);
    var handler = new GetUsersCountQueryHandler(repo);

    // Act
    var result = await handler.Handle(new GetUsersCountQuery(), default);

    // Assert
    Assert.True(result.IsSuccess);
    Assert.NotNull(result.Value);
    Assert.Equal(3, result.Value.Count);
  }

  [Fact]
  public async Task Handle_ReturnsZero_WhenNoUsersExist()
  {
    // Arrange
    var dbContext = CreateDbContext();
    var repo = new AppUsersRepository(dbContext);
    var handler = new GetUsersCountQueryHandler(repo);

    // Act
    var result = await handler.Handle(new GetUsersCountQuery(), default);

    // Assert
    Assert.True(result.IsSuccess);
    Assert.NotNull(result.Value);
    Assert.Equal(0, result.Value.Count);
  }
}
