using AppTemplate.Application.Features.Statistics.Users.Queries.GetUserRegistrationTrends;
using AppTemplate.Application.Repositories;
using AppTemplate.Core.Domain.Abstractions;
using AppTemplate.Core.Infrastructure.Pagination;
using AppTemplate.Domain.AppUsers;
using Ardalis.Result;
using Moq;

namespace AppTemplate.Application.Tests.Unit.Features.StatisticsTests.Users;

[Trait("Category", "Unit")]
public class GetUserRegistrationTrendsQueryHandlerUnitTests
{
  private readonly Mock<IAppUsersRepository> _userRepositoryMock = new();
  private readonly GetUserRegistrationTrendsQueryHandler _handler;

  public GetUserRegistrationTrendsQueryHandlerUnitTests()
  {
    _handler = new GetUserRegistrationTrendsQueryHandler(_userRepositoryMock.Object);
  }

  [Fact]
  public async Task Handle_ReturnsError_WhenRepositoryReturnsError()
  {
    var query = new GetUserRegistrationTrendsQuery();
    _userRepositoryMock
        .Setup(r => r.GetAllUsersWithIdentityAndRolesAsync(0, int.MaxValue, It.IsAny<CancellationToken>()))
        .ReturnsAsync(Result<PaginatedList<AppUser>>.Error("error"));

    var result = await _handler.Handle(query, default);

    Assert.False(result.IsSuccess);
    Assert.Contains("Could not retrieve users.", result.Errors);
  }

  [Fact]
  public async Task Handle_ReturnsError_WhenRepositoryReturnsNull()
  {
    var query = new GetUserRegistrationTrendsQuery();
    _userRepositoryMock
        .Setup(r => r.GetAllUsersWithIdentityAndRolesAsync(0, int.MaxValue, It.IsAny<CancellationToken>()))
        .ReturnsAsync(Result.Success<PaginatedList<AppUser>>(null));

    var result = await _handler.Handle(query, default);

    Assert.False(result.IsSuccess);
    Assert.Contains("Could not retrieve users.", result.Errors);
  }

  [Fact]
  public async Task Handle_ReturnsTrends_WhenUsersExist()
  {
    var today = DateTime.UtcNow.Date;
    var lastMonth = today.AddMonths(-1);

    var users = new List<AppUser>
        {
            new AppUserBuilder().WithCreatedOnUtc(today).Build(),
            new AppUserBuilder().WithCreatedOnUtc(today.AddDays(-1)).Build(),
            new AppUserBuilder().WithCreatedOnUtc(lastMonth.AddDays(1)).Build(),
        };

    var paginatedList = new PaginatedList<AppUser>(users, users.Count, 0, int.MaxValue);

    _userRepositoryMock
        .Setup(r => r.GetAllUsersWithIdentityAndRolesAsync(0, int.MaxValue, It.IsAny<CancellationToken>()))
        .ReturnsAsync(Result.Success<PaginatedList<AppUser>>(paginatedList));

    var query = new GetUserRegistrationTrendsQuery();

    var result = await _handler.Handle(query, default);

    Assert.True(result.IsSuccess);
    Assert.NotNull(result.Value);
    Assert.Equal(1, result.Value.TotalUsersLastMonth);
    Assert.Equal(2, result.Value.TotalUsersThisMonth);
    Assert.Equal(100, result.Value.GrowthPercentage); 
    Assert.Equal(30, result.Value.DailyRegistrations.Count);
  }

  // Helper builder for AppUser with custom CreatedOnUtc
  private class AppUserBuilder
  {
    private DateTime _createdOnUtc = DateTime.UtcNow;
    public AppUserBuilder WithCreatedOnUtc(DateTime date)
    {
      _createdOnUtc = date;
      return this;
    }
    public AppUser Build()
    {
      var user = AppUser.Create();
      var field = typeof(AppUser).GetField("<CreatedOnUtc>k__BackingField", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)
          ?? typeof(Entity<Guid>).GetField("<CreatedOnUtc>k__BackingField", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);

      if (field == null)
        throw new InvalidOperationException("Could not find backing field for CreatedOnUtc.");

      field.SetValue(user, _createdOnUtc);
      return user;
    }
  }
}
