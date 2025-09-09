using AppTemplate.Application.Features.AppUsers.Queries.GetAllUsersByRoleId;
using AppTemplate.Core.Infrastructure.Clock;
using AppTemplate.Domain.AppUsers;
using AppTemplate.Domain.Roles;
using AppTemplate.Infrastructure;
using AppTemplate.Infrastructure.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Testcontainers.PostgreSql;

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
}