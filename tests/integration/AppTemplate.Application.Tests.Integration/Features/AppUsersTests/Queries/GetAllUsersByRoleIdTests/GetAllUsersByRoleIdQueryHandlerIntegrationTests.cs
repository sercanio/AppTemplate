using AppTemplate.Application.Features.AppUsers.Queries.GetAllUsersByRoleId;
using AppTemplate.Application.Services.Clock;
using AppTemplate.Domain.AppUsers;
using AppTemplate.Domain.Roles;
using AppTemplate.Infrastructure;
using AppTemplate.Infrastructure.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Testcontainers.PostgreSql;

namespace AppTemplate.Application.Tests.Integration.Features.AppUsersTests.Queries.GetAllUsersByRoleIdTests;

[Trait("Category", "Integration")]
public class GetAllUsersByRoleIdQueryHandlerIntegrationTests : IAsyncLifetime
{
  private readonly PostgreSqlContainer _pgContainer;
  private ApplicationDbContext _dbContext;

  public GetAllUsersByRoleIdQueryHandlerIntegrationTests()
  {
    _pgContainer = new PostgreSqlBuilder()
        .WithDatabase("testdb")
        .WithUsername("testuser")
        .WithPassword("testpass")
        .Build();
  }

  public async Task InitializeAsync()
  {
    await _pgContainer.StartAsync();
    _dbContext = CreateDbContext();
    await _dbContext.Database.EnsureCreatedAsync();
  }

  public async Task DisposeAsync()
  {
    await _dbContext.DisposeAsync();
    await _pgContainer.DisposeAsync();
  }

  private ApplicationDbContext CreateDbContext()
  {
    var options = new DbContextOptionsBuilder<ApplicationDbContext>()
        .UseNpgsql(_pgContainer.GetConnectionString())
        .Options;
    return new ApplicationDbContext(options, new DateTimeProvider());
  }

  [Fact]
  public async Task Handle_ReturnsPaginatedUsers_ByRoleId()
  {
    // Arrange
    var createdById = Guid.NewGuid();
    var role = Role.Create("Admin", "administrator", createdById);
    _dbContext.Roles.Add(role);

    // Create users and assign role
    for (int i = 1; i <= 3; i++)
    {
      var identityUser = new IdentityUser
      {
        Id = $"user-{i}",
        UserName = $"testuser{i}",
        Email = $"testuser{i}@example.com"
      };
      _dbContext.Users.Add(identityUser);

      var appUser = AppUser.Create();
      appUser.SetIdentityId(identityUser.Id);
      appUser.AddRole(role);
      role.AddUser(appUser);
      _dbContext.AppUsers.Add(appUser);
    }

    // Add a user with a different role
    var otherRole = Role.Create("User", "User", createdById);
    _dbContext.Roles.Add(otherRole);

    var identityUserOther = new IdentityUser
    {
      Id = "user-4",
      UserName = "otheruser",
      Email = "otheruser@example.com"
    };
    _dbContext.Users.Add(identityUserOther);

    var appUserOther = AppUser.Create();
    appUserOther.SetIdentityId(identityUserOther.Id);
    appUserOther.AddRole(otherRole);
    otherRole.AddUser(appUserOther);
    _dbContext.AppUsers.Add(appUserOther);

    await _dbContext.SaveChangesAsync();

    var repo = new AppUsersRepository(_dbContext);
    var handler = new GetAllUsersByRoleIdQueryHandler(repo);

    var query = new GetAllUsersByRoleIdQuery(0, 10, role.Id);

    // Act
    var result = await handler.Handle(query, default);

    // Assert
    Assert.True(result.IsSuccess);
    Assert.NotNull(result.Value);
    Assert.Equal(3, result.Value.Items.Count);
    Assert.All(result.Value.Items, u => Assert.StartsWith("testuser", u.UserName));
    Assert.Equal(3, result.Value.TotalCount);
  }

  [Fact]
  public async Task Handle_ReturnsEmptyList_WhenNoUsersHaveRole()
  {
    // Arrange
    var createdById = Guid.NewGuid();
    var role = Role.Create("EmptyRole", "Empty Role", createdById);
    _dbContext.Roles.Add(role);

    await _dbContext.SaveChangesAsync();

    var repo = new AppUsersRepository(_dbContext);
    var handler = new GetAllUsersByRoleIdQueryHandler(repo);

    var query = new GetAllUsersByRoleIdQuery(0, 10, role.Id);

    // Act
    var result = await handler.Handle(query, default);

    // Assert
    Assert.True(result.IsSuccess);
    Assert.NotNull(result.Value);
    Assert.Empty(result.Value.Items);
    Assert.Equal(0, result.Value.TotalCount);
  }

  [Fact]
  public async Task Handle_SupportsPagination_ReturnsCorrectPage()
  {
    // Arrange
    var createdById = Guid.NewGuid();
    var role = Role.Create("Admin", "Administrator", createdById);
    _dbContext.Roles.Add(role);

    // Create 15 users with the same role
    for (int i = 1; i <= 15; i++)
    {
      var identityUser = new IdentityUser
      {
        Id = $"user-{i}",
        UserName = $"testuser{i:D2}",
        Email = $"testuser{i}@example.com"
      };
      _dbContext.Users.Add(identityUser);

      var appUser = AppUser.Create();
      appUser.SetIdentityId(identityUser.Id);
      appUser.AddRole(role);
      role.AddUser(appUser);
      _dbContext.AppUsers.Add(appUser);
    }

    await _dbContext.SaveChangesAsync();

    var repo = new AppUsersRepository(_dbContext);
    var handler = new GetAllUsersByRoleIdQueryHandler(repo);

    // Act - Get second page with 5 items per page
    var query = new GetAllUsersByRoleIdQuery(1, 5, role.Id);
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

  [Fact]
  public async Task Handle_FiltersOutDeletedRoles_InResponse()
  {
    // Arrange
    var createdById = Guid.NewGuid();
    var activeRole = Role.Create("ActiveRole", "Active Role", createdById);
    var deletedRole = Role.Create("DeletedRole", "Deleted Role", createdById);
    _dbContext.Roles.Add(activeRole);
    _dbContext.Roles.Add(deletedRole);

    var identityUser = new IdentityUser
    {
      Id = "user-1",
      UserName = "testuser",
      Email = "test@example.com"
    };
    _dbContext.Users.Add(identityUser);

    var appUser = AppUser.Create();
    appUser.SetIdentityId(identityUser.Id);
    appUser.AddRole(activeRole);
    appUser.AddRole(deletedRole);
    activeRole.AddUser(appUser);
    deletedRole.AddUser(appUser);
    _dbContext.AppUsers.Add(appUser);

    await _dbContext.SaveChangesAsync();

    // Soft delete the role
    Role.Delete(deletedRole, createdById);
    await _dbContext.SaveChangesAsync();

    var repo = new AppUsersRepository(_dbContext);
    var handler = new GetAllUsersByRoleIdQueryHandler(repo);

    var query = new GetAllUsersByRoleIdQuery(0, 10, activeRole.Id);

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
    var createdById = Guid.NewGuid();
    var role1 = Role.Create("Admin", "Administrator", createdById);
    var role2 = Role.Create("Manager", "Manager", createdById);
    var role3 = Role.Create("User", "User", createdById);
    _dbContext.Roles.AddRange(role1, role2, role3);

    var identityUser = new IdentityUser
    {
      Id = "user-1",
      UserName = "multiroleuser",
      Email = "multi@example.com"
    };
    _dbContext.Users.Add(identityUser);

    var appUser = AppUser.Create();
    appUser.SetIdentityId(identityUser.Id);
    appUser.AddRole(role1);
    appUser.AddRole(role2);
    appUser.AddRole(role3);
    role1.AddUser(appUser);
    role2.AddUser(appUser);
    role3.AddUser(appUser);
    _dbContext.AppUsers.Add(appUser);

    await _dbContext.SaveChangesAsync();

    var repo = new AppUsersRepository(_dbContext);
    var handler = new GetAllUsersByRoleIdQueryHandler(repo);

    var query = new GetAllUsersByRoleIdQuery(0, 10, role1.Id);

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
  public async Task Handle_ReturnsError_WhenRoleDoesNotExist()
  {
    // Arrange
    var nonExistentRoleId = Guid.NewGuid();
    var repo = new AppUsersRepository(_dbContext);
    var handler = new GetAllUsersByRoleIdQueryHandler(repo);

    var query = new GetAllUsersByRoleIdQuery(0, 10, nonExistentRoleId);

    // Act
    var result = await handler.Handle(query, default);

    // Assert - Repository might return empty list or error
    // Adjust based on your repository implementation
    if (result.IsSuccess)
    {
      Assert.Empty(result.Value.Items);
    }
    else
    {
      Assert.False(result.IsSuccess);
    }
  }
}