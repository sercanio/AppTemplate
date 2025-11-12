using System.Security.Claims;
using AppTemplate.Application.Features.Roles.Commands.Create;
using AppTemplate.Application.Services.Caching;
using AppTemplate.Application.Services.Clock;
using AppTemplate.Domain;
using AppTemplate.Domain.AppUsers;
using AppTemplate.Infrastructure;
using AppTemplate.Infrastructure.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Moq;

namespace AppTemplate.Application.Tests.Integration.Features.RolesTests.Commands.Create;

public class CreateRoleCommandHanderIntegrationTests
{
  private ApplicationDbContext CreateDbContext()
  {
    var options = new DbContextOptionsBuilder<ApplicationDbContext>()
        .UseInMemoryDatabase(Guid.NewGuid().ToString())
        .Options;
    return new ApplicationDbContext(options, new DateTimeProvider());
  }

  [Fact]
  public async Task Handle_CreatesRole_WhenValid()
  {
    // Arrange
    var dbContext = CreateDbContext();

    var identityUser = new IdentityUser
    {
      Id = "user-1",
      UserName = "testuser",
      Email = "testuser@example.com"
    };
    dbContext.Users.Add(identityUser);

    var appUser = AppUser.Create();
    appUser.SetIdentityId(identityUser.Id);
    dbContext.AppUsers.Add(appUser);

    await dbContext.SaveChangesAsync();

    var rolesRepo = new RolesRepository(dbContext);
    var usersRepo = new AppUsersRepository(dbContext);
    IUnitOfWork unitOfWork = dbContext;
    var distributedCacheMock = new Mock<IDistributedCache>();
    var cacheService = new CacheService(distributedCacheMock.Object);

    var httpContext = new DefaultHttpContext();
    httpContext.User = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, identityUser.Id) }, "TestAuth"));
    var httpContextAccessor = new HttpContextAccessor { HttpContext = httpContext };

    var handler = new CreateRoleCommandHander(
        rolesRepo,
        unitOfWork,
        cacheService,
        usersRepo,
        httpContextAccessor);

    var command = new CreateRoleCommand("Admin", "Administrator");

    // Act
    var result = await handler.Handle(command, default);

    // Assert
    Assert.Equal(Ardalis.Result.ResultStatus.Ok, result.Status);
    Assert.NotNull(result.Value);
    Assert.Equal("Admin", result.Value.Name);

    // Verify role is in database
    var createdRole = await dbContext.Roles.FirstOrDefaultAsync(r => r.Name.Value == "Admin");
    Assert.NotNull(createdRole);
    Assert.Equal("Administrator", createdRole.DisplayName.Value);
  }
}
