using AppTemplate.Application.Features.Statistics.Users.Queries.GetUsersCount;
using AppTemplate.Application.Repositories;
using Moq;

namespace AppTemplate.Application.Tests.Unit.Features.StatisticsTests.Users.Queries.GetUsersCount;

[Trait("Category", "Unit")]
public class GetUsersCountQueryHandlerUnitTests
{
  private readonly Mock<IAppUsersRepository> _userRepositoryMock = new();
  private readonly GetUsersCountQueryHandler _handler;

  public GetUsersCountQueryHandlerUnitTests()
  {
    _handler = new GetUsersCountQueryHandler(_userRepositoryMock.Object);
  }

  [Fact]
  public async Task Handle_ReturnsCorrectCount_WhenRepositoryReturnsCount()
  {
    var query = new GetUsersCountQuery();
    _userRepositoryMock
        .Setup(r => r.GetUsersCountAsync(It.IsAny<bool>(), It.IsAny<CancellationToken>()))
        .ReturnsAsync(42);

    var result = await _handler.Handle(query, default);

    Assert.True(result.IsSuccess);
    Assert.NotNull(result.Value);
    Assert.Equal(42, result.Value.Count);
  }

  [Fact]
  public async Task Handle_ReturnsZero_WhenRepositoryReturnsZero()
  {
    var query = new GetUsersCountQuery();
    _userRepositoryMock
        .Setup(r => r.GetUsersCountAsync(It.IsAny<bool>(), It.IsAny<CancellationToken>()))
        .ReturnsAsync(0);

    var result = await _handler.Handle(query, default);

    Assert.True(result.IsSuccess);
    Assert.NotNull(result.Value);
    Assert.Equal(0, result.Value.Count);
  }
}
