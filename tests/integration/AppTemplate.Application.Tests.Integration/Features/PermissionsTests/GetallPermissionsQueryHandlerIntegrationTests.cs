using AppTemplate.Application.Features.Permissions.Queries.GetAllPermissions;
using AppTemplate.Application.Services.Clock;
using AppTemplate.Domain.Roles;
using AppTemplate.Infrastructure;
using AppTemplate.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace AppTemplate.Application.Tests.Integration.Features.PermissionsTests;

public class GetallPermissionsQueryHandlerIntegrationTests
{
  private ApplicationDbContext CreateDbContext()
  {
    var options = new DbContextOptionsBuilder<ApplicationDbContext>()
        .UseInMemoryDatabase(Guid.NewGuid().ToString())
        .Options;
    return new ApplicationDbContext(options, new DateTimeProvider());
  }

  [Fact]
  public async Task Handle_ReturnsAllPermissions_WhenPermissionsExist()
  {
    // Arrange
    var dbContext = CreateDbContext();

    var permission1 = new Permission(Guid.NewGuid(), "FeatureA", "PermA");
    var permission2 = new Permission(Guid.NewGuid(), "FeatureB", "PermB");
    dbContext.Permissions.Add(permission1);
    dbContext.Permissions.Add(permission2);
    await dbContext.SaveChangesAsync();

    var repo = new PermissionsRepository(dbContext);
    var handler = new GetallPermissionsQueryHandler(repo);

    var query = new GetAllPermissionsQuery(0, 10);

    // Act
    var result = await handler.Handle(query, default);

    // Assert
    Assert.True(result.IsSuccess);
    Assert.NotNull(result.Value);
    Assert.Equal(2, result.Value.Items.Count);
    Assert.Contains(result.Value.Items, p => p.Name == "PermA" && p.Feature == "FeatureA");
    Assert.Contains(result.Value.Items, p => p.Name == "PermB" && p.Feature == "FeatureB");
  }

  [Fact]
  public async Task Handle_ReturnsEmpty_WhenNoPermissionsExist()
  {
    // Arrange
    var dbContext = CreateDbContext();
    var repo = new PermissionsRepository(dbContext);
    var handler = new GetallPermissionsQueryHandler(repo);

    var query = new GetAllPermissionsQuery(0, 10);

    // Act
    var result = await handler.Handle(query, default);

    // Assert
    Assert.True(result.IsSuccess);
    Assert.NotNull(result.Value);
    Assert.Empty(result.Value.Items);
  }
}
