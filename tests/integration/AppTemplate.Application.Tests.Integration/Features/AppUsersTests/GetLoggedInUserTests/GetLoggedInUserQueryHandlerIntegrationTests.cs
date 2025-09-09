using AppTemplate.Application.Features.AppUsers.Queries.GetLoggedInUser;
using AppTemplate.Core.Infrastructure.Clock;
using AppTemplate.Domain.AppUsers;
using AppTemplate.Infrastructure;
using AppTemplate.Infrastructure.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace AppTemplate.Application.Tests.Integration.Features.AppUsersTests.GetLoggedInUserTests;

public class GetLoggedInUserQueryHandlerIntegrationTests
{
  private ApplicationDbContext CreateDbContext()
  {
    var options = new DbContextOptionsBuilder<ApplicationDbContext>()
        .UseInMemoryDatabase(Guid.NewGuid().ToString())
        .Options;
    return new ApplicationDbContext(options, new DateTimeProvider());
  }

  [Fact]
  public async Task Handle_ReturnsUserResponse_WhenUserFound()
  {
    // Arrange
    var dbContext = CreateDbContext();

    var identityUser = new IdentityUser
    {
      Id = "user-123",
      UserName = "testuser",
      Email = "testuser@example.com",
      EmailConfirmed = true
    };
    dbContext.Users.Add(identityUser);

    var appUser = AppUser.Create();
    appUser.SetIdentityId(identityUser.Id);
    dbContext.AppUsers.Add(appUser);

    await dbContext.SaveChangesAsync();

    var repo = new AppUsersRepository(dbContext);

    // Setup HttpContextAccessor with claims
    var claims = new[] { new Claim(ClaimTypes.NameIdentifier, identityUser.Id) };
    var httpContext = new DefaultHttpContext();
    httpContext.User = new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuth"));
    var httpContextAccessor = new HttpContextAccessor { HttpContext = httpContext };

    var handler = new GetLoggedInUserQueryHandler(repo, httpContextAccessor);

    // Act
    var result = await handler.Handle(new GetLoggedInUserQuery(), default);

    // Assert
    Assert.Equal(Ardalis.Result.ResultStatus.Ok, result.Status);
    Assert.NotNull(result.Value);
    Assert.Equal(identityUser.Email, result.Value.Email);
    Assert.Equal(identityUser.UserName, result.Value.UserName);
    Assert.Equal(identityUser.EmailConfirmed, result.Value.EmailConfirmed);
  }
}
