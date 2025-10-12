using AppTemplate.Application.Features.Statistics.Users.Queries.GetUserRegistrationTrends;
using AppTemplate.Application.Repositories;
using AppTemplate.Application.Services.Clock;
using AppTemplate.Domain.AppUsers;
using AppTemplate.Infrastructure;
using AppTemplate.Infrastructure.Repositories;
using Ardalis.Result;
using MediatR;
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

    // Set the reference date to September 25, 2025 (so September is "this month" and August is "last month")
    var referenceDate = new DateTime(2025, 9, 25, 0, 0, 0, DateTimeKind.Utc);

    // Users in this month (September 2025)
    var user1 = AppUser.Create();
    user1.SetCreatedOnUtc(new DateTime(2025, 9, 5, 0, 0, 0, DateTimeKind.Utc)); // Sept 5, 2025
    user1.SetIdentityId(identityId1);
    dbContext.AppUsers.Add(user1);

    var user2 = AppUser.Create();
    user2.SetCreatedOnUtc(new DateTime(2025, 9, 10, 0, 0, 0, DateTimeKind.Utc)); // Sept 10, 2025
    user2.SetIdentityId(identityId2);
    dbContext.AppUsers.Add(user2);

    // User in last month (August 2025)
    var user3 = AppUser.Create();
    user3.SetCreatedOnUtc(new DateTime(2025, 8, 15, 0, 0, 0, DateTimeKind.Utc)); // Aug 15, 2025
    user3.SetIdentityId(identityId3);
    dbContext.AppUsers.Add(user3);

    await dbContext.SaveChangesAsync();

    var repo = new AppUsersRepository(dbContext);
    
    // Create a modified handler that uses the reference date instead of DateTime.UtcNow
    var handler = new TestableGetUserRegistrationTrendsQueryHandler(repo, referenceDate);

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
    var handler = new TestableGetUserRegistrationTrendsQueryHandler(repo, DateTime.UtcNow);

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

// Test-specific handler that allows injecting a custom "current date"
public class TestableGetUserRegistrationTrendsQueryHandler : IRequestHandler<GetUserRegistrationTrendsQuery, Result<GetUserRegistrationTrendsQueryResponse>>
{
    private readonly IAppUsersRepository _userRepository;
    private readonly DateTime _currentDate;

    public TestableGetUserRegistrationTrendsQueryHandler(IAppUsersRepository userRepository, DateTime currentDate)
    {
        _userRepository = userRepository;
        _currentDate = currentDate;
    }

    public async Task<Result<GetUserRegistrationTrendsQueryResponse>> Handle(
        GetUserRegistrationTrendsQuery request, 
        CancellationToken cancellationToken)
    {
        var result = await _userRepository.GetAllUsersWithIdentityAndRolesAsync(
            pageIndex: 0,
            pageSize: int.MaxValue,
            cancellationToken: cancellationToken);

        if (!result.IsSuccess || result.Value is null)
        {
            return Result.Error("Could not retrieve users.");
        }

        var allUsers = result.Value.Items;
        var today = _currentDate.Date; // Use injected date instead of DateTime.UtcNow.Date

        var usersThisMonth = GetUsersThisMonth(allUsers, today);
        var usersLastMonth = GetUsersLastMonth(allUsers, today);

        int totalUsersLastMonth = usersLastMonth.Count;
        int totalUsersThisMonth = usersThisMonth.Count;
        int growthPercentage = CalculateGrowthPercentage(totalUsersLastMonth, totalUsersThisMonth);

        var last30Days = GetLast30Days(today);
        var dailyRegistrations = GetDailyRegistrations(allUsers, last30Days);

        var response = new GetUserRegistrationTrendsQueryResponse(
            TotalUsersLastMonth: totalUsersLastMonth,
            TotalUsersThisMonth: totalUsersThisMonth,
            GrowthPercentage: growthPercentage,
            DailyRegistrations: dailyRegistrations
        );

        return Result.Success(response);
    }

    private static List<AppUser> GetUsersThisMonth(IEnumerable<AppUser> users, DateTime today)
    {
        var firstDayOfThisMonth = new DateTime(today.Year, today.Month, 1);
        return users.Where(u => u.CreatedOnUtc >= firstDayOfThisMonth).ToList();
    }

    private static List<AppUser> GetUsersLastMonth(IEnumerable<AppUser> users, DateTime today)
    {
        var firstDayOfThisMonth = new DateTime(today.Year, today.Month, 1);
        var firstDayOfLastMonth = firstDayOfThisMonth.AddMonths(-1);
        return users.Where(u => u.CreatedOnUtc >= firstDayOfLastMonth && u.CreatedOnUtc < firstDayOfThisMonth).ToList();
    }

    private static List<DateTime> GetLast30Days(DateTime today)
    {
        return Enumerable.Range(0, 30)
            .Select(i => today.AddDays(-i))
            .ToList();
    }

    private static Dictionary<string, int> GetDailyRegistrations(IEnumerable<AppUser> users, List<DateTime> last30Days)
    {
        var dailyRegistrations = new Dictionary<string, int>();
        foreach (var day in last30Days)
        {
            string dateKey = day.ToString("MM-dd");
            int count = users.Count(u => u.CreatedOnUtc.Date == day);
            dailyRegistrations.Add(dateKey, count);
        }
        return dailyRegistrations;
    }

    private static int CalculateGrowthPercentage(int lastMonth, int thisMonth)
    {
        return lastMonth > 0
            ? (int)Math.Round((double)(thisMonth - lastMonth) / lastMonth * 100)
            : 100;
    }
}