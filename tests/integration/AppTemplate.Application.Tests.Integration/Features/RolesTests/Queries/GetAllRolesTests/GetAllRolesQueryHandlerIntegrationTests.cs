using AppTemplate.Application.Features.Roles.Queries.GetAllRoles;
using AppTemplate.Domain.Roles;
using AppTemplate.Infrastructure;
using AppTemplate.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace AppTemplate.Application.Tests.Integration.Features.RolesTests.Queries.GetAllRolesTests;

public class GetAllRolesQueryHandlerIntegrationTests
{
  private ApplicationDbContext CreateDbContext()
  {
    var options = new DbContextOptionsBuilder<ApplicationDbContext>()
        .UseInMemoryDatabase(Guid.NewGuid().ToString())
        .Options;
    return new ApplicationDbContext(options, new AppTemplate.Core.Infrastructure.Clock.DateTimeProvider());
  }

  [Fact]
  public async Task Handle_ReturnsAllRoles_WhenRolesExist()
  {
    // Arrange
    var dbContext = CreateDbContext();

    var role1 = Role.Create("Admin", "Administrator", Guid.NewGuid());
    var role2 = Role.Create("User", "User", Guid.NewGuid());
    dbContext.Roles.Add(role1);
    dbContext.Roles.Add(role2);
    await dbContext.SaveChangesAsync();

    var repo = new RolesRepository(dbContext);
    var handler = new GetAllRolesQueryHandler(repo);

    var query = new GetAllRolesQuery(0, 10);

    // Act
    var result = await handler.Handle(query, default);

    // Assert
    Assert.True(result.IsSuccess);
    Assert.NotNull(result.Value);
    Assert.Equal(2, result.Value.Items.Count);
    Assert.Equal("Admin", result.Value.Items[0].Name);
    Assert.Equal("User", result.Value.Items[1].Name);
  }

  [Fact]
  public async Task Handle_ReturnsEmpty_WhenNoRolesExist()
  {
    // Arrange
    var dbContext = CreateDbContext();
    var repo = new RolesRepository(dbContext);
    var handler = new GetAllRolesQueryHandler(repo);

    var query = new GetAllRolesQuery(0, 10);

    // Act
    var result = await handler.Handle(query, default);

    // Assert
    Assert.True(result.IsSuccess);
    Assert.NotNull(result.Value);
    Assert.Empty(result.Value.Items);
  }
}
