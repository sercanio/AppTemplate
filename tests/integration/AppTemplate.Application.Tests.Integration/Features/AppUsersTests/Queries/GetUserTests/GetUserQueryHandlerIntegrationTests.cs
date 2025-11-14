using AppTemplate.Application.Features.AppUsers.Queries.GetUser;
using AppTemplate.Application.Services.Clock;
using AppTemplate.Domain.AppUsers;
using AppTemplate.Domain.Roles;
using AppTemplate.Infrastructure;
using AppTemplate.Infrastructure.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace AppTemplate.Application.Tests.Integration.Features.AppUsersTests.Queries.GetUserTests;

[Trait("Category", "Integration")]
public class GetUserQueryHandlerIntegrationTests
{
  private ApplicationDbContext CreateDbContext()
  {
    var options = new DbContextOptionsBuilder<ApplicationDbContext>()
        .UseInMemoryDatabase(Guid.NewGuid().ToString())
        .Options;
    return new ApplicationDbContext(options, new DateTimeProvider());
  }

  [Fact]
  public async Task Handle_ReturnsUserResponse_WithIdentityUser_WhenUserFound()
  {
    // Arrange
    var dbContext = CreateDbContext();

    var identityUser = new IdentityUser
    {
      Id = "test-identity-id",
      UserName = "testuser",
      Email = "testuser@example.com"
    };
    dbContext.Users.Add(identityUser);

    var appUser = AppUser.Create();
    appUser.SetIdentityId(identityUser.Id);
    dbContext.AppUsers.Add(appUser);

    await dbContext.SaveChangesAsync();

    var repo = new AppUsersRepository(dbContext);
    var handler = new GetUserQueryHandler(repo);

    var query = new GetUserQuery(appUser.Id);

    // Act
    var result = await handler.Handle(query, default);

    // Assert
    Assert.Equal(Ardalis.Result.ResultStatus.Ok, result.Status);
    Assert.NotNull(result.Value);
    Assert.Equal(appUser.Id, result.Value.Id);
    Assert.Equal(identityUser.UserName, result.Value.UserName);
  }

  [Fact]
  public async Task Handle_ReturnsNotFound_WhenUserDoesNotExist()
  {
    // Arrange
    var dbContext = CreateDbContext();
    var repo = new AppUsersRepository(dbContext);
    var handler = new GetUserQueryHandler(repo);

    var query = new GetUserQuery(Guid.NewGuid());

    // Act
    var result = await handler.Handle(query, default);

    // Assert
    Assert.Equal(Ardalis.Result.ResultStatus.NotFound, result.Status);
    Assert.Null(result.Value);
  }

  [Fact]
  public async Task Handle_ReturnsUserWithRoles()
  {
    // Arrange
    var dbContext = CreateDbContext();

    var createdById = Guid.NewGuid();
    var role1 = Role.Create("Admin", "Administrator", createdById);
    var role2 = Role.Create("User", "Standard User", createdById, isDefault: true);
    dbContext.Roles.AddRange(role1, role2);

    var identityUser = new IdentityUser
    {
      Id = "test-identity-id",
      UserName = "testuser",
      Email = "testuser@example.com"
    };
    dbContext.Users.Add(identityUser);

    var appUser = AppUser.Create();
    appUser.SetIdentityId(identityUser.Id);
    appUser.AddRole(role1);
    appUser.AddRole(role2);
    role1.AddUser(appUser);
    role2.AddUser(appUser);
    dbContext.AppUsers.Add(appUser);

    await dbContext.SaveChangesAsync();

    var repo = new AppUsersRepository(dbContext);
    var handler = new GetUserQueryHandler(repo);

    var query = new GetUserQuery(appUser.Id);

    // Act
    var result = await handler.Handle(query, default);

    // Assert
    Assert.Equal(Ardalis.Result.ResultStatus.Ok, result.Status);
    Assert.NotNull(result.Value);
    Assert.Equal(2, result.Value.Roles.Count);
    Assert.Contains(result.Value.Roles, r => r.Name == "Admin" && !r.IsDefault);
    Assert.Contains(result.Value.Roles, r => r.Name == "User" && r.IsDefault);
  }

  [Fact]
  public async Task Handle_ReturnsUserWithEmptyRoles_WhenUserHasNoRoles()
  {
    // Arrange
    var dbContext = CreateDbContext();

    var identityUser = new IdentityUser
    {
      Id = "test-identity-id",
      UserName = "noroleuser",
      Email = "norole@example.com"
    };
    dbContext.Users.Add(identityUser);

    var appUser = AppUser.Create();
    appUser.SetIdentityId(identityUser.Id);
    dbContext.AppUsers.Add(appUser);

    await dbContext.SaveChangesAsync();

    var repo = new AppUsersRepository(dbContext);
    var handler = new GetUserQueryHandler(repo);

    var query = new GetUserQuery(appUser.Id);

    // Act
    var result = await handler.Handle(query, default);

    // Assert
    Assert.Equal(Ardalis.Result.ResultStatus.Ok, result.Status);
    Assert.NotNull(result.Value);
    Assert.Empty(result.Value.Roles);
  }

  [Fact]
  public async Task Handle_ReturnsCorrectRoleProperties()
  {
    // Arrange
    var dbContext = CreateDbContext();

    var createdById = Guid.NewGuid();
    var role = Role.Create("SuperAdmin", "Super Administrator", createdById, isDefault: false);
    dbContext.Roles.Add(role);

    var identityUser = new IdentityUser
    {
      Id = "test-identity-id",
      UserName = "superadmin",
      Email = "superadmin@example.com"
    };
    dbContext.Users.Add(identityUser);

    var appUser = AppUser.Create();
    appUser.SetIdentityId(identityUser.Id);
    appUser.AddRole(role);
    role.AddUser(appUser);
    dbContext.AppUsers.Add(appUser);

    await dbContext.SaveChangesAsync();

    var repo = new AppUsersRepository(dbContext);
    var handler = new GetUserQueryHandler(repo);

    var query = new GetUserQuery(appUser.Id);

    // Act
    var result = await handler.Handle(query, default);

    // Assert
    Assert.Equal(Ardalis.Result.ResultStatus.Ok, result.Status);
    Assert.NotNull(result.Value);
    Assert.Single(result.Value.Roles);

    var mappedRole = result.Value.Roles.First();
    Assert.Equal(role.Id, mappedRole.Id);
    Assert.Equal("SuperAdmin", mappedRole.Name);
    Assert.Equal("Super Administrator", mappedRole.DisplayName);
    Assert.False(mappedRole.IsDefault);
  }

  [Fact]
  public async Task Handle_HandlesMultipleRolesWithMixedDefaultFlags()
  {
    // Arrange
    var dbContext = CreateDbContext();

    var createdById = Guid.NewGuid();
    var defaultRole = Role.Create("RegisteredUser", "Registered User", createdById, isDefault: true);
    var adminRole = Role.Create("Admin", "Administrator", createdById, isDefault: false);
    var moderatorRole = Role.Create("Moderator", "Moderator", createdById, isDefault: false);
    dbContext.Roles.AddRange(defaultRole, adminRole, moderatorRole);

    var identityUser = new IdentityUser
    {
      Id = "test-identity-id",
      UserName = "poweruser",
      Email = "power@example.com"
    };
    dbContext.Users.Add(identityUser);

    var appUser = AppUser.Create();
    appUser.SetIdentityId(identityUser.Id);
    appUser.AddRole(defaultRole);
    appUser.AddRole(adminRole);
    appUser.AddRole(moderatorRole);
    defaultRole.AddUser(appUser);
    adminRole.AddUser(appUser);
    moderatorRole.AddUser(appUser);
    dbContext.AppUsers.Add(appUser);

    await dbContext.SaveChangesAsync();

    var repo = new AppUsersRepository(dbContext);
    var handler = new GetUserQueryHandler(repo);

    var query = new GetUserQuery(appUser.Id);

    // Act
    var result = await handler.Handle(query, default);

    // Assert
    Assert.Equal(Ardalis.Result.ResultStatus.Ok, result.Status);
    Assert.NotNull(result.Value);
    Assert.Equal(3, result.Value.Roles.Count);

    Assert.Single(result.Value.Roles.Where(r => r.IsDefault));
    Assert.Equal(2, result.Value.Roles.Count(r => !r.IsDefault));

    Assert.Contains(result.Value.Roles, r => r.Name == "RegisteredUser" && r.IsDefault);
    Assert.Contains(result.Value.Roles, r => r.Name == "Admin" && !r.IsDefault);
    Assert.Contains(result.Value.Roles, r => r.Name == "Moderator" && !r.IsDefault);
  }
}
