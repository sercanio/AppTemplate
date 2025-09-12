using AppTemplate.Application.Data.Pagination;
using AppTemplate.Application.Features.Statistics.Roles.Queries.GetRoleStatistics;
using AppTemplate.Application.Repositories;
using AppTemplate.Domain.Roles;
using AppTemplate.Domain.Roles.ValueObjects;
using Moq;

namespace AppTemplate.Application.Tests.Unit.Features.StatisticsTests.Roles;

[Trait("Category", "Unit")]
public class GetRoleStatisticsQueryHandlerUnitTests
{
  private readonly Mock<IRolesRepository> _rolesRepositoryMock = new();
  private readonly Mock<IPermissionsRepository> _permissionsRepositoryMock = new();
  private readonly GetRoleStatisticsQueryHandler _handler;

  public GetRoleStatisticsQueryHandlerUnitTests()
  {
    _handler = new GetRoleStatisticsQueryHandler(_rolesRepositoryMock.Object, _permissionsRepositoryMock.Object);
  }

  [Fact]
  public async Task Handle_ReturnsCorrectStatistics_WhenDataExists()
  {
    // Arrange
    var role1 = new Role(Guid.NewGuid(), new RoleName("Admin"), new RoleName("Admin"), null, false);
    var role2 = new Role(Guid.NewGuid(), new RoleName("User"), new RoleName("User"), null, false);

    var permission1 = new Permission(Guid.NewGuid(), "FeatureA", "PermA");
    var permission2 = new Permission(Guid.NewGuid(), "FeatureA", "PermB");
    var permission3 = new Permission(Guid.NewGuid(), "FeatureB", "PermC");

    // Add permissions to roles
    typeof(Role).GetField("_permissions", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)
        .SetValue(role1, new List<Permission> { permission1, permission2 });
    typeof(Role).GetField("_permissions", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)
        .SetValue(role2, new List<Permission> { permission3 });

    // Add users to roles
    var user1 = AppTemplate.Domain.AppUsers.AppUser.Create();
    var user2 = AppTemplate.Domain.AppUsers.AppUser.Create();
    typeof(Role).GetField("_users", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)
        .SetValue(role1, new List<AppTemplate.Domain.AppUsers.AppUser> { user1 });
    typeof(Role).GetField("_users", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)
        .SetValue(role2, new List<AppTemplate.Domain.AppUsers.AppUser> { user2 });

    var rolesList = new List<Role> { role1, role2 };
    var permissionsList = new List<Permission> { permission1, permission2, permission3 };

    _rolesRepositoryMock
        .Setup(r => r.GetAllAsync(0,
          int.MaxValue,
          null,
          false,
          It.IsAny<Func<IQueryable<Role>, IQueryable<Role>>>(),
          true,
          It.IsAny<CancellationToken>()))
        .ReturnsAsync(new PaginatedList<Role>(rolesList, rolesList.Count, 0, int.MaxValue));

    _permissionsRepositoryMock
        .Setup(r => r.GetAllAsync(0,
          int.MaxValue,
          null,
          false,
          null,
          true,
        It.IsAny<CancellationToken>()))
        .ReturnsAsync(new PaginatedList<Permission>(permissionsList, permissionsList.Count, 0, int.MaxValue));

    // Act
    var result = await _handler.Handle(new GetRoleStatisticsQuery(), default);

    // Assert
    Assert.True(result.IsSuccess);
    Assert.NotNull(result.Value);
    Assert.Equal(2, result.Value.TotalRoles);
    Assert.Equal(3, result.Value.TotalPermissions);
    Assert.Equal(2, result.Value.PermissionsPerRole["Admin"]);
    Assert.Equal(1, result.Value.PermissionsPerRole["User"]);
    Assert.Equal(1, result.Value.UsersPerRole["Admin"]);
    Assert.Equal(1, result.Value.UsersPerRole["User"]);
    Assert.Equal(2, result.Value.PermissionsByFeature["FeatureA"]);
    Assert.Equal(1, result.Value.PermissionsByFeature["FeatureB"]);
  }

  [Fact]
  public async Task Handle_ReturnsZero_WhenNoData()
  {
    _rolesRepositoryMock
        .Setup(r => r.GetAllAsync(0,
          int.MaxValue,
          null, 
          false,
          It.IsAny<Func<IQueryable<Role>, IQueryable<Role>>>(),
          true,
          It.IsAny<CancellationToken>()))
        .ReturnsAsync(new PaginatedList<Role>(new List<Role>(), 0, 0, int.MaxValue));

    _permissionsRepositoryMock
        .Setup(r => r.GetAllAsync(0,
          int.MaxValue,
          null,
          false,
          null,
          true,
          It.IsAny<CancellationToken>()))
        .ReturnsAsync(new PaginatedList<Permission>(new List<Permission>(), 0, 0, int.MaxValue));

    var result = await _handler.Handle(new GetRoleStatisticsQuery(), default);

    Assert.True(result.IsSuccess);
    Assert.NotNull(result.Value);
    Assert.Equal(0, result.Value.TotalRoles);
    Assert.Equal(0, result.Value.TotalPermissions);
    Assert.Empty(result.Value.PermissionsPerRole);
    Assert.Empty(result.Value.UsersPerRole);
    Assert.Empty(result.Value.PermissionsByFeature);
  }
}
