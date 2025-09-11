using AppTemplate.Application.Features.Statistics.Roles.Queries.GetRoleStatistics;
using AppTemplate.Core.Infrastructure.Clock;
using AppTemplate.Domain.AppUsers;
using AppTemplate.Domain.Roles;
using AppTemplate.Domain.Roles.ValueObjects;
using AppTemplate.Infrastructure;
using AppTemplate.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace AppTemplate.Application.Tests.Integration.Features.StatisticsTests.Roles;

public class GetRoleStatisticsQueryHandlerIntegrationTests
{
  private ApplicationDbContext CreateDbContext()
  {
    var options = new DbContextOptionsBuilder<ApplicationDbContext>()
        .UseInMemoryDatabase(Guid.NewGuid().ToString())
        .Options;
    return new ApplicationDbContext(options, new DateTimeProvider());
  }

  [Fact]
  public async Task Handle_ReturnsCorrectStatistics_WhenDataExists()
  {
    // Arrange
    var dbContext = CreateDbContext();

    var permission1 = new Permission(Guid.NewGuid(), "FeatureA", "PermA");
    var permission2 = new Permission(Guid.NewGuid(), "FeatureA", "PermB");
    var permission3 = new Permission(Guid.NewGuid(), "FeatureB", "PermC");
    dbContext.Permissions.AddRange(permission1, permission2, permission3);

    var user1 = AppUser.Create();
    user1.SetIdentityId("user1");
    var user2 = AppUser.Create();
    user2.SetIdentityId("user2");
    dbContext.AppUsers.AddRange(user1, user2);

    var role1 = new Role(Guid.NewGuid(), new RoleName("Admin"), new RoleName("Admin"), null, false);
    var role2 = new Role(Guid.NewGuid(), new RoleName("User"), new RoleName("User"), null, false);

    // Add permissions to roles
    typeof(Role).GetField("_permissions", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)
        .SetValue(role1, new List<Permission> { permission1, permission2 });
    typeof(Role).GetField("_permissions", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)
        .SetValue(role2, new List<Permission> { permission3 });

    // Add users to roles
    typeof(Role).GetField("_users", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)
        .SetValue(role1, new List<AppUser> { user1 });
    typeof(Role).GetField("_users", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)
        .SetValue(role2, new List<AppUser> { user2 });

    dbContext.Roles.AddRange(role1, role2);

    await dbContext.SaveChangesAsync();

    var rolesRepo = new RolesRepository(dbContext);
    var permissionsRepo = new PermissionsRepository(dbContext);
    var handler = new GetRoleStatisticsQueryHandler(rolesRepo, permissionsRepo);

    // Act
    var result = await handler.Handle(new GetRoleStatisticsQuery(), default);

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
    // Arrange
    var dbContext = CreateDbContext();
    var rolesRepo = new RolesRepository(dbContext);
    var permissionsRepo = new PermissionsRepository(dbContext);
    var handler = new GetRoleStatisticsQueryHandler(rolesRepo, permissionsRepo);

    // Act
    var result = await handler.Handle(new GetRoleStatisticsQuery(), default);

    // Assert
    Assert.True(result.IsSuccess);
    Assert.NotNull(result.Value);
    Assert.Equal(0, result.Value.TotalRoles);
    Assert.Equal(0, result.Value.TotalPermissions);
    Assert.Empty(result.Value.PermissionsPerRole);
    Assert.Empty(result.Value.UsersPerRole);
    Assert.Empty(result.Value.PermissionsByFeature);
  }
}
