using System.Security.Claims;
using AppTemplate.Application.Enums;
using AppTemplate.Application.Features.AppUsers.Commands.Update.UpdateUserRoles;
using AppTemplate.Application.Services.Caching;
using AppTemplate.Application.Services.Clock;
using AppTemplate.Application.Services.Roles;
using AppTemplate.Domain;
using AppTemplate.Domain.AppUsers;
using AppTemplate.Domain.Roles;
using AppTemplate.Infrastructure;
using AppTemplate.Infrastructure.Repositories;
using Ardalis.Result;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Moq;

namespace AppTemplate.Application.Tests.Integration.Features.AppUsersTests.Commands.UpdateUserRolesTests;

public class UpdateUserRolesCommandHandlerIntegrationTests
{
  private ApplicationDbContext CreateDbContext()
  {
    var options = new DbContextOptionsBuilder<ApplicationDbContext>()
        .UseInMemoryDatabase(Guid.NewGuid().ToString())
        .Options;
    return new ApplicationDbContext(options, new DateTimeProvider());
  }

  [Fact]
  public async Task Handle_AddsRoleToUser_WhenValid()
  {
    // Arrange
    var dbContext = CreateDbContext();

    // Create role and user
    var role = Role.Create("Admin", "Administrator", Guid.NewGuid());
    dbContext.Roles.Add(role);

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

    // Setup HttpContextAccessor with claims
    var claims = new[] { new Claim(ClaimTypes.NameIdentifier, identityUser.Id) };
    var httpContext = new DefaultHttpContext();
    httpContext.User = new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuth"));
    var httpContextAccessor = new HttpContextAccessor { HttpContext = httpContext };

    var repo = new AppUsersRepository(dbContext);
    var rolesRepository = new RolesRepository(dbContext);
    var rolesService = new RolesService(rolesRepository);
    IUnitOfWork unitOfWork = dbContext;
    var distributedCacheMock = new Mock<IDistributedCache>();
    var cacheService = new CacheService(distributedCacheMock.Object);

    var handler = new UpdateUserRolesCommandHandler(
        repo,
        rolesService,
        unitOfWork,
        cacheService,
        httpContextAccessor);

    var command = new UpdateUserRolesCommand(appUser.Id, Operation.Add, role.Id);

    // Act
    var result = await handler.Handle(command, default);

    // Assert
    Assert.True(result.IsSuccess);
    var updatedUser = await dbContext.AppUsers.Include(u => u.Roles).FirstAsync(u => u.Id == appUser.Id);
    Assert.Contains(updatedUser.Roles, r => r.Id == role.Id);
  }

  [Fact]
  public async Task Handle_RemovesRoleFromUser_WhenValid()
  {
    // Arrange
    var dbContext = CreateDbContext();

    var role = Role.Create("Admin", "Administrator", Guid.NewGuid());
    dbContext.Roles.Add(role);

    var identityUser = new IdentityUser
    {
      Id = "user-1",
      UserName = "testuser",
      Email = "testuser@example.com"
    };
    dbContext.Users.Add(identityUser);

    var appUser = AppUser.Create();
    appUser.SetIdentityId(identityUser.Id);
    appUser.AddRole(role);
    dbContext.AppUsers.Add(appUser);

    await dbContext.SaveChangesAsync();

    var claims = new[] { new Claim(ClaimTypes.NameIdentifier, identityUser.Id) };
    var httpContext = new DefaultHttpContext();
    httpContext.User = new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuth"));
    var httpContextAccessor = new HttpContextAccessor { HttpContext = httpContext };

    var repo = new AppUsersRepository(dbContext);
    var rolesRepository = new RolesRepository(dbContext);
    var rolesService = new RolesService(rolesRepository);
    IUnitOfWork unitOfWork = dbContext;
    var distributedCacheMock = new Mock<IDistributedCache>();
    var cacheService = new CacheService(distributedCacheMock.Object);

    var handler = new UpdateUserRolesCommandHandler(
        repo,
        rolesService,
        unitOfWork,
        cacheService,
        httpContextAccessor);

    var command = new UpdateUserRolesCommand(appUser.Id, Operation.Remove, role.Id);

    // Act
    var result = await handler.Handle(command, default);

    // Assert
    Assert.True(result.IsSuccess);
    var updatedUser = await dbContext.AppUsers.Include(u => u.Roles).FirstAsync(u => u.Id == appUser.Id);
    Assert.DoesNotContain(updatedUser.Roles, r => r.Id == role.Id);
  }

  [Fact]
  public async Task Handle_ReturnsError_WhenUserNotAuthenticated()
  {
    // Arrange
    var dbContext = CreateDbContext();

    // Create role and user
    var role = Role.Create("Admin", "Administrator", Guid.NewGuid());
    dbContext.Roles.Add(role);

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

    // No HttpContextAccessor (simulate unauthenticated)
    var httpContextAccessor = new HttpContextAccessor { HttpContext = null };

    var repo = new AppUsersRepository(dbContext);
    var rolesRepository = new RolesRepository(dbContext);
    var rolesService = new RolesService(rolesRepository);
    IUnitOfWork unitOfWork = dbContext;
    var distributedCacheMock = new Mock<IDistributedCache>();
    var cacheService = new CacheService(distributedCacheMock.Object);

    var handler = new UpdateUserRolesCommandHandler(
        repo,
        rolesService,
        unitOfWork,
        cacheService,
        httpContextAccessor);

    var command = new UpdateUserRolesCommand(appUser.Id, Operation.Add, role.Id);

    // Act
    var result = await handler.Handle(command, default);

    // Assert
    Assert.Equal(Ardalis.Result.ResultStatus.Error, result.Status);
    Assert.Contains("Current user not authenticated.", result.Errors);
  }

  [Fact]
  public async Task Handle_ReturnsNotFound_WhenUserDoesNotExist()
  {
    // Arrange
    var dbContext = CreateDbContext();

    var role = Role.Create("Admin", "Administrator", Guid.NewGuid());
    dbContext.Roles.Add(role);

    var identityUser = new IdentityUser
    {
      Id = "user-1",
      UserName = "testuser",
      Email = "testuser@example.com"
    };
    dbContext.Users.Add(identityUser);

    await dbContext.SaveChangesAsync();

    var claims = new[] { new Claim(ClaimTypes.NameIdentifier, identityUser.Id) };
    var httpContext = new DefaultHttpContext();
    httpContext.User = new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuth"));
    var httpContextAccessor = new HttpContextAccessor { HttpContext = httpContext };

    var repo = new AppUsersRepository(dbContext);
    var rolesRepository = new RolesRepository(dbContext);
    var rolesService = new RolesService(rolesRepository);
    IUnitOfWork unitOfWork = dbContext;
    var distributedCacheMock = new Mock<IDistributedCache>();
    var cacheService = new CacheService(distributedCacheMock.Object);

    var handler = new UpdateUserRolesCommandHandler(
        repo,
        rolesService,
        unitOfWork,
        cacheService,
        httpContextAccessor);

    var command = new UpdateUserRolesCommand(Guid.NewGuid(), Operation.Add, role.Id);

    // Act
    var result = await handler.Handle(command, default);

    // Assert
    Assert.Equal(ResultStatus.NotFound, result.Status);
  }

  [Fact]
  public async Task Handle_ReturnsNotFound_WhenRoleDoesNotExist()
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

    var claims = new[] { new Claim(ClaimTypes.NameIdentifier, identityUser.Id) };
    var httpContext = new DefaultHttpContext();
    httpContext.User = new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuth"));
    var httpContextAccessor = new HttpContextAccessor { HttpContext = httpContext };

    var repo = new AppUsersRepository(dbContext);
    var rolesRepository = new RolesRepository(dbContext);
    var rolesService = new RolesService(rolesRepository);
    IUnitOfWork unitOfWork = dbContext;
    var distributedCacheMock = new Mock<IDistributedCache>();
    var cacheService = new CacheService(distributedCacheMock.Object);

    var handler = new UpdateUserRolesCommandHandler(
        repo,
        rolesService,
        unitOfWork,
        cacheService,
        httpContextAccessor);

    var command = new UpdateUserRolesCommand(appUser.Id, Operation.Add, Guid.NewGuid());

    // Act
    var result = await handler.Handle(command, default);

    // Assert
    Assert.Equal(ResultStatus.NotFound, result.Status);
  }
}