using AppTemplate.Application.Features.Statistics.Users.Queries.GetUserRegistrationTrends;
using AppTemplate.Application.Services.Clock;
using AppTemplate.Domain.AppUsers;
using AppTemplate.Infrastructure;
using AppTemplate.Infrastructure.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace AppTemplate.Application.Tests.Integration.Features.StatisticsTests.Users;

public class GetUserRegistrationTrendsQueryHandlerIntegrationTests
{
  private ApplicationDbContext CreateDbContext()
  {
    var options = new DbContextOptionsBuilder<ApplicationDbContext>()
        .UseInMemoryDatabase(Guid.NewGuid().ToString())
        .Options;
    return new ApplicationDbContext(options, new DateTimeProvider());
  }

  private async Task<string> CreateIdentityUserAsync(ApplicationDbContext dbContext, string email, string userName = null)
  {
    var identityUser = new IdentityUser
    {
      Id = Guid.NewGuid().ToString(),
      UserName = userName ?? email,
      Email = email,
    };

    dbContext.Users.Add(identityUser);
    await dbContext.SaveChangesAsync();

    return identityUser.Id;
  }

  [Fact]
  public async Task Handle_ReturnsTrends_WhenUsersExist()
  {
    // Arrange
    var dbContext = CreateDbContext();

    // Create IdentityUsers first
    var identityId1 = await CreateIdentityUserAsync(dbContext, "user1@test.com");
    var identityId2 = await CreateIdentityUserAsync(dbContext, "user2@test.com");
    var identityId3 = await CreateIdentityUserAsync(dbContext, "user3@test.com");

    // Users in this month (September 2025)
    var user1 = AppUser.Create();
    user1.SetCreatedOnUtc(new DateTime(2025, 9, 5)); // Sept 5, 2025
    user1.SetIdentityId(identityId1);
    dbContext.AppUsers.Add(user1);

    var user2 = AppUser.Create();
    user2.SetCreatedOnUtc(new DateTime(2025, 9, 10)); // Sept 10, 2025
    user2.SetIdentityId(identityId2);
    dbContext.AppUsers.Add(user2);

    // User in last month (August 2025)
    var user3 = AppUser.Create();
    user3.SetCreatedOnUtc(new DateTime(2025, 8, 15)); // Aug 15, 2025
    user3.SetIdentityId(identityId3);
    dbContext.AppUsers.Add(user3);

    await dbContext.SaveChangesAsync();

    var repo = new AppUsersRepository(dbContext);
    var handler = new GetUserRegistrationTrendsQueryHandler(repo);

    // Act
    var result = await handler.Handle(new GetUserRegistrationTrendsQuery(), default);

    // Assert
    Assert.True(result.IsSuccess);
    Assert.NotNull(result.Value);
    Assert.Equal(1, result.Value.TotalUsersLastMonth);
    Assert.Equal(2, result.Value.TotalUsersThisMonth);
    Assert.Equal(100, result.Value.GrowthPercentage);
    Assert.Equal(30, result.Value.DailyRegistrations.Count);
  }

  [Fact]
  public async Task Handle_ReturnsZeroes_WhenNoUsersExist()
  {
    // Arrange
    var dbContext = CreateDbContext();
    var repo = new AppUsersRepository(dbContext);
    var handler = new GetUserRegistrationTrendsQueryHandler(repo);

    // Act
    var result = await handler.Handle(new GetUserRegistrationTrendsQuery(), default);

    // Assert
    Assert.True(result.IsSuccess);
    Assert.NotNull(result.Value);
    Assert.Equal(0, result.Value.TotalUsersLastMonth);
    Assert.Equal(0, result.Value.TotalUsersThisMonth);
    Assert.Equal(100, result.Value.GrowthPercentage);
    Assert.Equal(30, result.Value.DailyRegistrations.Count);
  }
}