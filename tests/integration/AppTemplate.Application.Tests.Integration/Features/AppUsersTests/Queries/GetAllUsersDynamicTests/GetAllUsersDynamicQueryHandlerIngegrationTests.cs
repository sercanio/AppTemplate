using AppTemplate.Application.Data.DynamicQuery;
using AppTemplate.Application.Features.AppUsers.Queries.GetAllUsersDynamic;
using AppTemplate.Application.Services.Clock;
using AppTemplate.Domain.AppUsers;
using AppTemplate.Domain.Roles;
using AppTemplate.Infrastructure;
using AppTemplate.Infrastructure.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace AppTemplate.Application.Tests.Integration.Features.AppUsersTests.Queries.GetAllUsersDynamicTests;

[Trait("Category", "Integration")]
public class GetAllUsersDynamicQueryHandlerIntegrationTests
{
  private ApplicationDbContext CreateDbContext(string databaseName)
  {
    var options = new DbContextOptionsBuilder<ApplicationDbContext>()
        .UseInMemoryDatabase(databaseName)
        .Options;
    return new ApplicationDbContext(options, new DateTimeProvider());
  }

  [Fact]
  public async Task Handle_ReturnsPaginatedUsers_WhenUsersExist()
  {
    // Arrange
    var databaseName = Guid.NewGuid().ToString();

    // Setup data in one context
    using (var setupContext = CreateDbContext(databaseName))
    {
      var identityUser = new IdentityUser
      {
        Id = "user-1",
        UserName = "testuser1",
        Email = "testuser1@example.com"
      };
      setupContext.Users.Add(identityUser);

      var appUser = AppUser.Create();
      appUser.SetIdentityId(identityUser.Id);
      setupContext.AppUsers.Add(appUser);

      await setupContext.SaveChangesAsync();
    }

    // Use a fresh context for the test to simulate real-world usage
    using var testContext = CreateDbContext(databaseName);
    var repo = new AppUsersRepository(testContext);
    var handler = new GetAllUsersDynamicQueryHandler(repo);
    var dynamicQuery = new DynamicQuery(); // No filters, get all
    var query = new GetAllUsersDynamicQuery(0, 10, dynamicQuery);

    // Act
    var result = await handler.Handle(query, default);

    // Assert
    Assert.True(result.IsSuccess);
    Assert.NotNull(result.Value);
    Assert.Single(result.Value.Items);
    Assert.Equal("testuser1", result.Value.Items.First().UserName);
  }

  [Fact]
  public async Task Handle_ReturnsPaginatedList_WhenRepositoryReturnsUsers()
  {
    // Arrange
    var databaseName = Guid.NewGuid().ToString();
    var dbContext = CreateDbContext(databaseName);

    // Add IdentityUser and AppUser
    var identityUser = new IdentityUser
    {
      Id = "user-1",
      UserName = "testuser1",
      Email = "testuser1@example.com"
    };
    dbContext.Users.Add(identityUser);

    var appUser = AppUser.Create();
    appUser.SetIdentityId(identityUser.Id);
    dbContext.AppUsers.Add(appUser);

    await dbContext.SaveChangesAsync();

    var repo = new AppUsersRepository(dbContext);
    var handler = new GetAllUsersDynamicQueryHandler(repo);
    var dynamicQuery = new DynamicQuery(); // No filters, get all
    var query = new GetAllUsersDynamicQuery(0, 10, dynamicQuery);

    // Act
    var result = await handler.Handle(query, default);

    // Assert
    Assert.True(result.IsSuccess);
    Assert.NotNull(result.Value);
    Assert.Single(result.Value.Items);
    Assert.Equal("testuser1", result.Value.Items.First().UserName);

    // Clean up
    dbContext.Dispose();
  }

  [Fact]
  public async Task Handle_ReturnsEmptyList_WhenNoUsersExist()
  {
    // Arrange
    var databaseName = Guid.NewGuid().ToString();
    using var dbContext = CreateDbContext(databaseName);

    var repo = new AppUsersRepository(dbContext);
    var handler = new GetAllUsersDynamicQueryHandler(repo);
    var dynamicQuery = new DynamicQuery();
    var query = new GetAllUsersDynamicQuery(0, 10, dynamicQuery);

    // Act
    var result = await handler.Handle(query, default);

    // Assert
    Assert.True(result.IsSuccess);
    Assert.NotNull(result.Value);
    Assert.Empty(result.Value.Items);
    Assert.Equal(0, result.Value.TotalCount);
  }

  [Fact]
  public async Task Handle_ReturnsPaginatedUsers_WithMultipleUsers()
  {
    // Arrange
    var databaseName = Guid.NewGuid().ToString();
    using var dbContext = CreateDbContext(databaseName);

    // Add multiple users
    for (int i = 1; i <= 5; i++)
    {
      var identityUser = new IdentityUser
      {
        Id = $"user-{i}",
        UserName = $"testuser{i}",
        Email = $"testuser{i}@example.com"
      };
      dbContext.Users.Add(identityUser);

      var appUser = AppUser.Create();
      appUser.SetIdentityId(identityUser.Id);
      dbContext.AppUsers.Add(appUser);
    }

    await dbContext.SaveChangesAsync();

    var repo = new AppUsersRepository(dbContext);
    var handler = new GetAllUsersDynamicQueryHandler(repo);
    var dynamicQuery = new DynamicQuery(); // No filters, get all
    var query = new GetAllUsersDynamicQuery(0, 3, dynamicQuery); // Page size 3

    // Act
    var result = await handler.Handle(query, default);

    // Assert
    Assert.True(result.IsSuccess);
    Assert.NotNull(result.Value);
    Assert.Equal(3, result.Value.Items.Count);
    Assert.Equal(5, result.Value.TotalCount);
    Assert.Equal(0, result.Value.PageIndex);
    Assert.Equal(3, result.Value.PageSize);
  }

  [Fact]
  public async Task Handle_FiltersDeletedRoles_InResponse()
  {
    // Arrange
    var databaseName = Guid.NewGuid().ToString();
    using var dbContext = CreateDbContext(databaseName);

    var createdById = Guid.NewGuid();
    var activeRole = Role.Create("ActiveRole", "Active Role", createdById);
    var deletedRole = Role.Create("DeletedRole", "Deleted Role", createdById);
    dbContext.Roles.AddRange(activeRole, deletedRole);

    var identityUser = new IdentityUser
    {
      Id = "user-1",
      UserName = "testuser",
      Email = "test@example.com"
    };
    dbContext.Users.Add(identityUser);

    var appUser = AppUser.Create();
    appUser.SetIdentityId(identityUser.Id);
    appUser.AddRole(activeRole);
    appUser.AddRole(deletedRole);
    activeRole.AddUser(appUser);
    deletedRole.AddUser(appUser);
    dbContext.AppUsers.Add(appUser);

    await dbContext.SaveChangesAsync();

    // Soft delete the role
    Role.Delete(deletedRole, createdById);
    await dbContext.SaveChangesAsync();

    var repo = new AppUsersRepository(dbContext);
    var handler = new GetAllUsersDynamicQueryHandler(repo);
    var dynamicQuery = new DynamicQuery();
    var query = new GetAllUsersDynamicQuery(0, 10, dynamicQuery);

    // Act
    var result = await handler.Handle(query, default);

    // Assert
    Assert.True(result.IsSuccess);
    Assert.NotNull(result.Value);
    Assert.Single(result.Value.Items);
    Assert.Single(result.Value.Items[0].Roles);
    Assert.Equal("ActiveRole", result.Value.Items[0].Roles.First().Name);
    Assert.DoesNotContain(result.Value.Items[0].Roles, r => r.Name == "DeletedRole");
  }

  [Fact]
  public async Task Handle_IncludesUsersWithMultipleRoles()
  {
    // Arrange
    var databaseName = Guid.NewGuid().ToString();
    using var dbContext = CreateDbContext(databaseName);

    var createdById = Guid.NewGuid();
    var role1 = Role.Create("Admin", "Administrator", createdById);
    var role2 = Role.Create("Manager", "Manager", createdById);
    var role3 = Role.Create("User", "User", createdById);
    dbContext.Roles.AddRange(role1, role2, role3);

    var identityUser = new IdentityUser
    {
      Id = "user-1",
      UserName = "multiroleuser",
      Email = "multi@example.com"
    };
    dbContext.Users.Add(identityUser);

    var appUser = AppUser.Create();
    appUser.SetIdentityId(identityUser.Id);
    appUser.AddRole(role1);
    appUser.AddRole(role2);
    appUser.AddRole(role3);
    role1.AddUser(appUser);
    role2.AddUser(appUser);
    role3.AddUser(appUser);
    dbContext.AppUsers.Add(appUser);

    await dbContext.SaveChangesAsync();

    var repo = new AppUsersRepository(dbContext);
    var handler = new GetAllUsersDynamicQueryHandler(repo);
    var dynamicQuery = new DynamicQuery();
    var query = new GetAllUsersDynamicQuery(0, 10, dynamicQuery);

    // Act
    var result = await handler.Handle(query, default);

    // Assert
    Assert.True(result.IsSuccess);
    Assert.NotNull(result.Value);
    Assert.Single(result.Value.Items);
    Assert.Equal(3, result.Value.Items[0].Roles.Count);
    Assert.Contains(result.Value.Items[0].Roles, r => r.Name == "Admin");
    Assert.Contains(result.Value.Items[0].Roles, r => r.Name == "Manager");
    Assert.Contains(result.Value.Items[0].Roles, r => r.Name == "User");
  }

  [Fact]
  public async Task Handle_SupportsPagination()
  {
    // Arrange
    var databaseName = Guid.NewGuid().ToString();
    using var dbContext = CreateDbContext(databaseName);

    // Create 15 users
    for (int i = 1; i <= 15; i++)
    {
      var identityUser = new IdentityUser
      {
        Id = $"user-{i}",
        UserName = $"testuser{i:D2}",
        Email = $"testuser{i}@example.com"
      };
      dbContext.Users.Add(identityUser);

      var appUser = AppUser.Create();
      appUser.SetIdentityId(identityUser.Id);
      dbContext.AppUsers.Add(appUser);
    }

    await dbContext.SaveChangesAsync();

    var repo = new AppUsersRepository(dbContext);
    var handler = new GetAllUsersDynamicQueryHandler(repo);
    var dynamicQuery = new DynamicQuery();

    // Act - Get second page with 5 items per page
    var query = new GetAllUsersDynamicQuery(1, 5, dynamicQuery);
    var result = await handler.Handle(query, default);

    // Assert
    Assert.True(result.IsSuccess);
    Assert.NotNull(result.Value);
    Assert.Equal(5, result.Value.Items.Count);
    Assert.Equal(15, result.Value.TotalCount);
    Assert.Equal(1, result.Value.PageIndex);
    Assert.Equal(5, result.Value.PageSize);
    Assert.True(result.Value.HasPreviousPage);
    Assert.True(result.Value.HasNextPage);
  }
}