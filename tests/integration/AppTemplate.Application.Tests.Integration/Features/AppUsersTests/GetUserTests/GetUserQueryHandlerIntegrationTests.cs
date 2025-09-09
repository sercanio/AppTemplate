using AppTemplate.Application.Features.AppUsers.Queries.GetUser;
using AppTemplate.Core.Infrastructure.Clock;
using AppTemplate.Domain.AppUsers;
using AppTemplate.Infrastructure;
using AppTemplate.Infrastructure.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace AppTemplate.Application.Tests.Integration.Features.AppUsersTests.GetUserTests;

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

    var identityUser = new IdentityUser { Id = "test-identity-id", UserName = "testuser", Email = "testuser@example.com" };
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
}
