using AppTemplate.Application.Features.AppUsers.Queries.GetUser;
using AppTemplate.Application.Repositories;
using AppTemplate.Domain.AppUsers;
using Ardalis.Result;
using Moq;

namespace AppTemplate.Application.Tests.Unit.Features.AppUsersTests.Queries.GetUserTests;

[Trait("Category", "Unit")]
public class GetUserQueryHandlerUnitTests
{
    private readonly Mock<IAppUsersRepository> _userRepositoryMock;
    private readonly GetUserQueryHandler _handler;

    public GetUserQueryHandlerUnitTests()
    {
        _userRepositoryMock = new Mock<IAppUsersRepository>();
        _handler = new GetUserQueryHandler(_userRepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_ReturnsNotFound_WhenUserNotFound()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _userRepositoryMock
            .Setup(r => r.GetUserByIdWithIdentityAndRrolesAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<AppUser>.NotFound("User not found"));

        var query = new GetUserQuery(userId);

        // Act
        var result = await _handler.Handle(query, default);

        // Assert
        Assert.Equal(ResultStatus.NotFound, result.Status);
        Assert.Null(result.Value);
    }

    [Fact]
    public async Task Handle_ReturnsNotFound_WhenUserIsNull()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _userRepositoryMock
            .Setup(r => r.GetUserByIdWithIdentityAndRrolesAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success<AppUser>(null));

        var query = new GetUserQuery(userId);

        // Act
        var result = await _handler.Handle(query, default);

        // Assert
        Assert.Equal(ResultStatus.NotFound, result.Status);
        Assert.Null(result.Value);
    }

}
