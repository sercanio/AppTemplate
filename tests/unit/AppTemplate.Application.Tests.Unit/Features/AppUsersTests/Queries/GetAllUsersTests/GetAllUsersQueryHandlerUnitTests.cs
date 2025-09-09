using AppTemplate.Application.Features.AppUsers.Queries.GetAllUsers;
using AppTemplate.Application.Repositories;
using AppTemplate.Core.Infrastructure.Pagination;
using AppTemplate.Domain.AppUsers;
using Ardalis.Result;
using Moq;

namespace AppTemplate.Application.Tests.Unit.Features.AppUsersTests.Queries.GetAllUsersTests;

[Trait("Category", "Unit")]
public class GetAllUsersQueryHandlerUnitTests
{
  private readonly Mock<IAppUsersRepository> _userRepositoryMock;
  private readonly GetAllUsersQueryHandler _handler;

  public GetAllUsersQueryHandlerUnitTests()
  {
    _userRepositoryMock = new Mock<IAppUsersRepository>();
    _handler = new GetAllUsersQueryHandler(_userRepositoryMock.Object);
  }

  [Fact]
  public async Task Handle_ReturnsError_WhenRepositoryReturnsError()
  {
    var query = new GetAllUsersQuery(0, 10);

    _userRepositoryMock
        .Setup(r => r.GetAllUsersWithIdentityAndRolesAsync(0, 10, It.IsAny<CancellationToken>()))
        .ReturnsAsync(Result<PaginatedList<AppUser>>.Error("error"));

    var result = await _handler.Handle(query, default);

    Assert.False(result.IsSuccess);
    Assert.Equal("Could not retrieve users.", result.Errors.First());
  }

  [Fact]
  public async Task Handle_ReturnsError_WhenRepositoryReturnsNull()
  {
    var query = new GetAllUsersQuery(0, 10);

    _userRepositoryMock
        .Setup(r => r.GetAllUsersWithIdentityAndRolesAsync(0, 10, It.IsAny<CancellationToken>()))
        .ReturnsAsync(Result.Success<PaginatedList<AppUser>>(null));

    var result = await _handler.Handle(query, default);

    Assert.False(result.IsSuccess);
    Assert.Equal("Could not retrieve users.", result.Errors.First());
  }
}
