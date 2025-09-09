using AppTemplate.Application.Features.AppUsers.Queries.GetAllUsersByRoleId;
using AppTemplate.Application.Repositories;
using AppTemplate.Core.Infrastructure.Pagination;
using AppTemplate.Domain.AppUsers;
using Ardalis.Result;
using Moq;

[Trait("Category", "Unit")]
public class GetAllUsersByRoleIdQueryHandlerUnitTests
{
  private readonly Mock<IAppUsersRepository> _userRepositoryMock;
  private readonly GetAllUsersByRoleIdQueryHandler _handler;

  public GetAllUsersByRoleIdQueryHandlerUnitTests()
  {
    _userRepositoryMock = new Mock<IAppUsersRepository>();
    _handler = new GetAllUsersByRoleIdQueryHandler(_userRepositoryMock.Object);
  }

  [Fact]
  public async Task Handle_ReturnsError_WhenRepositoryReturnsError()
  {
    // Arrange
    var roleId = Guid.NewGuid();
    var query = new GetAllUsersByRoleIdQuery(0, 10, roleId);

    _userRepositoryMock
        .Setup(r => r.GetAllUsersByRoleIdWithIdentityAndRolesAsync(roleId, 0, 10, It.IsAny<CancellationToken>()))
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
    var roleId = Guid.NewGuid();
    var query = new GetAllUsersByRoleIdQuery(0, 10, roleId);

    _userRepositoryMock
        .Setup(r => r.GetAllUsersByRoleIdWithIdentityAndRolesAsync(roleId, 0, 10, It.IsAny<CancellationToken>()))
        .ReturnsAsync(Result.Success<PaginatedList<AppUser>>(null));

    // Act
    var result = await _handler.Handle(query, default);

    // Assert
    Assert.False(result.IsSuccess);
    Assert.Equal("Could not retrieve users.", result.Errors.First());
  }
}