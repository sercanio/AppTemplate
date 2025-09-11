using AppTemplate.Application.Features.Roles.Queries.GetRoleById;
using AppTemplate.Domain.Roles;
using AppTemplate.Infrastructure;
using AppTemplate.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace AppTemplate.Application.Tests.Integration.Features.RolesTests.Queries.GetRoleByIdTests;

public class GetRoleByIdQueryHandlerIntgrationTests
{
  private ApplicationDbContext CreateDbContext()
  {
    var options = new DbContextOptionsBuilder<ApplicationDbContext>()
        .UseInMemoryDatabase(Guid.NewGuid().ToString())
        .Options;
    return new ApplicationDbContext(options, new AppTemplate.Core.Infrastructure.Clock.DateTimeProvider());
  }

  [Fact]
  public async Task Handle_ReturnsRoleWithPermissions_WhenRoleExists()
  {
    // Arrange
    var dbContext = CreateDbContext();

    var role = Role.Create("Admin", "Administrator", Guid.NewGuid());
    var permission = new Permission(Guid.NewGuid(), "TestPermission", "TestFeature");
    role.AddPermission(permission, Guid.NewGuid());
    dbContext.Roles.Add(role);
    dbContext.Permissions.Add(permission);
    await dbContext.SaveChangesAsync();

    var repo = new RolesRepository(dbContext);
    var handler = new GetRoleByIdQueryHandler(repo);

    var query = new GetRoleByIdQuery(role.Id);

    // Act
    var result = await handler.Handle(query, default);

    // Assert
    Assert.Equal(Ardalis.Result.ResultStatus.Ok, result.Status);
    Assert.NotNull(result.Value);
    Assert.Equal(role.Id, result.Value.Id);
    Assert.Equal(role.Name.Value, result.Value.Name);
    Assert.Equal(role.DisplayName.Value, result.Value.DisplayName);
    Assert.Equal(role.IsDefault, result.Value.IsDefault);
    Assert.Single(result.Value.Permissions);
    Assert.Equal(permission.Name, result.Value.Permissions.First().Name);
  }

  [Fact]
  public async Task Handle_ReturnsNotFound_WhenRoleDoesNotExist()
  {
    // Arrange
    var dbContext = CreateDbContext();
    var repo = new RolesRepository(dbContext);
    var handler = new GetRoleByIdQueryHandler(repo);

    var query = new GetRoleByIdQuery(Guid.NewGuid());

    // Act
    var result = await handler.Handle(query, default);

    // Assert
    Assert.Equal(Ardalis.Result.ResultStatus.NotFound, result.Status);
  }
}
