using AppTemplate.Application.Features.Roles.Commands.Update.UpdateRoleName;
using AppTemplate.Application.Services.Caching;
using AppTemplate.Application.Services.Clock;
using AppTemplate.Domain;
using AppTemplate.Domain.AppUsers;
using AppTemplate.Domain.Roles;
using AppTemplate.Infrastructure;
using AppTemplate.Infrastructure.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Moq;
using System.Security.Claims;

namespace AppTemplate.Application.Tests.Integration.Features.RolesTests.Commands.Update;

public class UpdateRoleNameCommandHandlerIntegrationTests
{
  private ApplicationDbContext CreateDbContext()
  {
    var options = new DbContextOptionsBuilder<ApplicationDbContext>()
        .UseInMemoryDatabase(Guid.NewGuid().ToString())
        .Options;
    return new ApplicationDbContext(options, new DateTimeProvider());
  }

  [Fact]
  public async Task Handle_UpdatesRoleName_WhenValid()
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

    var role = Role.Create("OldName", "OldDisplayName", appUser.Id);
    dbContext.Roles.Add(role);

    await dbContext.SaveChangesAsync();
    dbContext.ChangeTracker.Clear();

    var rolesRepo = new RolesRepository(dbContext);
    var usersRepo = new AppUsersRepository(dbContext);
    IUnitOfWork unitOfWork = dbContext;
    var distributedCacheMock = new Mock<IDistributedCache>();
    var cacheService = new CacheService(distributedCacheMock.Object);

    var httpContext = new DefaultHttpContext();
    httpContext.User = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, identityUser.Id) }, "TestAuth"));
    var httpContextAccessor = new HttpContextAccessor { HttpContext = httpContext };

    var handler = new UpdateRoleNameCommandHandler(
        rolesRepo,
        usersRepo,
        httpContextAccessor,
        unitOfWork,
        cacheService);

    var command = new UpdateRoleNameCommand(role.Id, "NewName", "NewDisplayName");

    // Act
    var result = await handler.Handle(command, default);

    // Assert
    Assert.Equal(Ardalis.Result.ResultStatus.Ok, result.Status);
    Assert.NotNull(result.Value);
    Assert.Equal("NewName", result.Value.Name);

    // Verify role is updated in database
    var updatedRole = await dbContext.Roles.FirstOrDefaultAsync(r => r.Id == role.Id);
    Assert.NotNull(updatedRole);
    Assert.Equal("NewName", updatedRole.Name.Value);
    Assert.Equal("NewDisplayName", updatedRole.DisplayName.Value);
  }
}
