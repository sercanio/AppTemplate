using AppTemplate.Application.Features.Roles.Queries.GetRoleById;
using AppTemplate.Application.Repositories;
using AppTemplate.Domain.Roles;
using Ardalis.Result;
using Moq;

namespace AppTemplate.Application.Tests.Unit.Features.RolesTests.Queries.GetRoleByIdTests;

[Trait("Category", "Unit")]
public class GetRoleByIdQueryHandlerUnitTests
{
  private readonly Mock<IRolesRepository> _rolesRepositoryMock = new();
  private readonly GetRoleByIdQueryHandler _handler;

  public GetRoleByIdQueryHandlerUnitTests()
  {
    _handler = new GetRoleByIdQueryHandler(_rolesRepositoryMock.Object);
  }

  [Fact]
  public async Task Handle_ReturnsNotFound_WhenRoleNotFound()
  {
    var roleId = Guid.NewGuid();
    var query = new GetRoleByIdQuery(roleId);

    _rolesRepositoryMock
        .Setup(r => r.GetRoleByIdWithPermissionsAsync(roleId, It.IsAny<CancellationToken>()))
        .ReturnsAsync(Result<Role>.NotFound("Role not found."));

    var result = await _handler.Handle(query, default);

    Assert.Equal(ResultStatus.NotFound, result.Status);
  }

  [Fact]
  public async Task Handle_ReturnsRole_WhenRoleExists()
  {
    var roleId = Guid.NewGuid();
    var role = Role.Create("Admin", "Administrator", Guid.NewGuid());
    var permission = new Permission(Guid.NewGuid(), "TestPermission", "TestFeature");
    role.AddPermission(permission, Guid.NewGuid());

    _rolesRepositoryMock
        .Setup(r => r.GetRoleByIdWithPermissionsAsync(roleId, It.IsAny<CancellationToken>()))
        .ReturnsAsync(Result.Success(role));

    var query = new GetRoleByIdQuery(roleId);

    var result = await _handler.Handle(query, default);

    Assert.Equal(ResultStatus.Ok, result.Status);
    Assert.NotNull(result.Value);
    Assert.Equal(role.Id, result.Value.Id);
    Assert.Equal(role.Name.Value, result.Value.Name);
    Assert.Equal(role.DisplayName.Value, result.Value.DisplayName);
    Assert.Equal(role.IsDefault, result.Value.IsDefault);
    Assert.Single(result.Value.Permissions);
    Assert.Equal(permission.Name, result.Value.Permissions.First().Name);
  }
}
