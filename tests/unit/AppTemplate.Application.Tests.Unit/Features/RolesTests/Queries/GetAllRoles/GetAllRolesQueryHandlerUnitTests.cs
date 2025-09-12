using AppTemplate.Application.Data.Pagination;
using AppTemplate.Application.Features.Roles.Queries.GetAllRoles;
using AppTemplate.Application.Repositories;
using AppTemplate.Domain.Roles;
using Ardalis.Result;
using Moq;

namespace AppTemplate.Application.Tests.Unit.Features.RolesTests.Queries.GetAllRoles;

[Trait("Category", "Unit")]
public class GetAllRolesQueryHandlerUnitTests
{
  private readonly Mock<IRolesRepository> _rolesRepositoryMock = new();
  private readonly GetAllRolesQueryHandler _handler;

  public GetAllRolesQueryHandlerUnitTests()
  {
    _handler = new GetAllRolesQueryHandler(_rolesRepositoryMock.Object);
  }

  [Fact]
  public async Task Handle_ReturnsError_WhenRepositoryReturnsError()
  {
    var query = new GetAllRolesQuery(0, 10);

    _rolesRepositoryMock
        .Setup(r => r.GetAllRolesAsync(0, 10, It.IsAny<CancellationToken>()))
        .ReturnsAsync(Result<PaginatedList<Role>>.Error("error"));

    var result = await _handler.Handle(query, default);

    Assert.False(result.IsSuccess);
    Assert.Equal("Could not retrieve roles.", result.Errors.First());
  }

  [Fact]
  public async Task Handle_ReturnsRoles_WhenRepositoryReturnsRoles()
  {
    var query = new GetAllRolesQuery(0, 10);

    var roles = new List<Role>
        {
            Role.Create("Admin", "Administrator", Guid.NewGuid()),
            Role.Create("User", "User", Guid.NewGuid())
        };
    var paginatedList = new PaginatedList<Role>(roles, roles.Count, 0, 10);

    _rolesRepositoryMock
        .Setup(r => r.GetAllRolesAsync(0, 10, It.IsAny<CancellationToken>()))
        .ReturnsAsync(Result.Success(paginatedList));

    var result = await _handler.Handle(query, default);

    Assert.True(result.IsSuccess);
    Assert.NotNull(result.Value);
    Assert.Equal(2, result.Value.Items.Count);
    Assert.Equal("Admin", result.Value.Items[0].Name);
    Assert.Equal("User", result.Value.Items[1].Name);
  }
}
