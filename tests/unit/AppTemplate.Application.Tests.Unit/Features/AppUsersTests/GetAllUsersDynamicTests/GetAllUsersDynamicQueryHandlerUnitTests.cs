using AppTemplate.Application.Features.AppUsers.Queries.GetAllUsersDynamic;
using AppTemplate.Application.Repositories;
using AppTemplate.Core.Infrastructure.DynamicQuery;
using AppTemplate.Core.Infrastructure.Pagination;
using AppTemplate.Domain.AppUsers;
using Ardalis.Result;
using Moq;

namespace AppTemplate.Application.Tests.Unit.Features.AppUsersTests.GetAllUsersDynamicTests;

[Trait("Category", "Unit")]
public class GetAllUsersDynamicQueryHandlerUnitTests
{
  private readonly Mock<IAppUsersRepository> _userRepositoryMock;
  private readonly GetAllUsersDynamicQueryHandler _handler;

  public GetAllUsersDynamicQueryHandlerUnitTests()
  {
      _userRepositoryMock = new Mock<IAppUsersRepository>();
      _handler = new GetAllUsersDynamicQueryHandler(_userRepositoryMock.Object);
  }

  [Fact]
  public async Task Handle_ReturnsError_WhenRepositoryReturnsError()
  {
      // Arrange
      var dynamicQuery = new DynamicQuery();
      var query = new GetAllUsersDynamicQuery(1, 10, dynamicQuery);

      _userRepositoryMock
          .Setup(r => r.GetAllUsersDynamicWithIdentityAndRolesAsync(dynamicQuery, 1, 10, It.IsAny<CancellationToken>()))
          .ReturnsAsync(Result<PaginatedList<AppUser>>.Error("error"));

      // Act
      var result = await _handler.Handle(query, default);

      // Assert
      Assert.False(result.IsSuccess);
      Assert.Equal("Could not retrieve users.", result.Errors.First());
  }

  [Fact]
  public async Task Handle_ReturnsError_WhenRepositoryReturnsNull()
  {
      // Arrange
      var dynamicQuery = new DynamicQuery();
      var query = new GetAllUsersDynamicQuery(1, 10, dynamicQuery);

      _userRepositoryMock
          .Setup(r => r.GetAllUsersDynamicWithIdentityAndRolesAsync(dynamicQuery, 1, 10, It.IsAny<CancellationToken>()))
          .ReturnsAsync(Result.Success<PaginatedList<AppUser>>(null));

      // Act
      var result = await _handler.Handle(query, default);

      // Assert
      Assert.False(result.IsSuccess);
      Assert.Equal("Could not retrieve users.", result.Errors.First());
  }
}
