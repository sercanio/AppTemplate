using System.Security.Claims;
using AppTemplate.Application.Enums;
using AppTemplate.Application.Features.Roles.Commands.Update.UpdatePermissions;
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

namespace AppTemplate.Application.Tests.Integration.Features.RolesTests.Commands.Update;

public class UpdateRolePermissionsCommandHandlerIntegrationTests
{
  private ApplicationDbContext CreateDbContext()
  {
    var options = new DbContextOptionsBuilder<ApplicationDbContext>()
        .UseInMemoryDatabase(Guid.NewGuid().ToString())
        .Options;
    return new ApplicationDbContext(options, new DateTimeProvider());
  }

  [Fact]
  public async Task Handle_AddsPermissionToRole_WhenValid()
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

    var role = Role.Create("Role", "Role", appUser.Id);
    dbContext.Roles.Add(role);

    var permission = new Permission(Guid.NewGuid(), "Perm", "Feature");
    dbContext.Permissions.Add(permission);

    await dbContext.SaveChangesAsync();
    dbContext.ChangeTracker.Clear();

    var rolesRepo = new RolesRepository(dbContext);
    var permissionsRepo = new PermissionsRepository(dbContext);
    var usersRepo = new AppUsersRepository(dbContext);
    IUnitOfWork unitOfWork = dbContext;
    var distributedCacheMock = new Mock<IDistributedCache>();
    var cacheService = new CacheService(distributedCacheMock.Object);

    var httpContext = new DefaultHttpContext();
    httpContext.User = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, identityUser.Id) }, "TestAuth"));
    var httpContextAccessor = new HttpContextAccessor { HttpContext = httpContext };

    var handler = new UpdateRolePermissionsCommandHandler(
        rolesRepo,
        permissionsRepo,
        usersRepo,
        unitOfWork,
        cacheService,
        httpContextAccessor);

    var command = new UpdateRolePermissionsCommand(role.Id, permission.Id, Operation.Add);

    // Act
    var result = await handler.Handle(command, default);

    // Assert
    Assert.Equal(Ardalis.Result.ResultStatus.Ok, result.Status);
    Assert.NotNull(result.Value);
    Assert.Equal(role.Id, result.Value.RoleId);
    Assert.Equal(permission.Id, result.Value.PermissionId);

    // Verify permission is added to role in database
    var updatedRole = await dbContext.Roles.Include(r => r.Permissions).FirstOrDefaultAsync(r => r.Id == role.Id);
    Assert.NotNull(updatedRole);
    Assert.Contains(updatedRole.Permissions, p => p.Id == permission.Id);
  }
}
