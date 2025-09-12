using AppTemplate.Application.Data.Pagination;
using AppTemplate.Application.Features.Permissions.Queries.GetAllPermissions;
using AppTemplate.Application.Repositories;
using AppTemplate.Domain.Roles;
using Ardalis.Result;
using Moq;

namespace AppTemplate.Application.Tests.Unit.Features.PermissionsTests;

[Trait("Category", "Unit")]
public class GetallPermissionsQueryHandlerUnitTests
{
  private readonly Mock<IPermissionsRepository> _permissionsRepositoryMock = new();
  private readonly GetallPermissionsQueryHandler _handler;

  public GetallPermissionsQueryHandlerUnitTests()
  {
    _handler = new GetallPermissionsQueryHandler(_permissionsRepositoryMock.Object);
  }

  [Fact]
  public async Task Handle_ReturnsError_WhenRepositoryReturnsError()
  {
    var query = new GetAllPermissionsQuery(0, 10);

    _permissionsRepositoryMock
        .Setup(r => r.GetAllPermissionsAsync(0, 10, It.IsAny<CancellationToken>()))
        .ReturnsAsync(Result<PaginatedList<Permission>>.Error("error"));

    var result = await _handler.Handle(query, default);

    Assert.False(result.IsSuccess);
    Assert.Contains("Could not retrieve permissions.", result.Errors);
  }

  [Fact]
  public async Task Handle_ReturnsError_WhenRepositoryReturnsNull()
  {
    var query = new GetAllPermissionsQuery(0, 10);

    _permissionsRepositoryMock
    .Setup(r => r.GetAllPermissionsAsync(0, 10, It.IsAny<CancellationToken>()))
    .ReturnsAsync(Result.Success<PaginatedList<Permission>>(null));

    var result = await _handler.Handle(query, default);

    Assert.False(result.IsSuccess);
    Assert.Contains("Could not retrieve permissions.", result.Errors);
  }

  [Fact]
  public async Task Handle_ReturnsPermissions_WhenRepositoryReturnsPermissions()
  {
    var query = new GetAllPermissionsQuery(0, 10);

    var permissions = new List<Permission>
        {
            new Permission(Guid.NewGuid(), "FeatureA", "PermA"),
            new Permission(Guid.NewGuid(), "FeatureB", "PermB")
        };
    var paginatedList = new PaginatedList<Permission>(permissions, permissions.Count, 0, 10);

    _permissionsRepositoryMock
    .Setup(r => r.GetAllPermissionsAsync(0, 10, It.IsAny<CancellationToken>()))
    .ReturnsAsync(Result.Success<PaginatedList<Permission>>(paginatedList));

    var result = await _handler.Handle(query, default);

    Assert.True(result.IsSuccess);
    Assert.NotNull(result.Value);
    Assert.Equal(2, result.Value.Items.Count);
    Assert.Equal("FeatureA", result.Value.Items[0].Feature);
    Assert.Equal("PermA", result.Value.Items[0].Name);
    Assert.Equal("FeatureB", result.Value.Items[1].Feature);
    Assert.Equal("PermB", result.Value.Items[1].Name);
  }
}
