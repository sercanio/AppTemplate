using AppTemplate.Application.Features.AppUsers.Queries.GetAllUsers;
using AppTemplate.Application.Services.Clock;
using AppTemplate.Domain.AppUsers;
using AppTemplate.Infrastructure;
using AppTemplate.Infrastructure.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace AppTemplate.Application.Tests.Integration.Features.AppUsersTests.Queries.GetAllUsersTests;

[Trait("Category", "Integration")]
public class GetAllUsersQueryHandlerIntegrationTests
{
    private ApplicationDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new ApplicationDbContext(options, new DateTimeProvider());
    }

    [Fact]
    public async Task Handle_ReturnsPaginatedUsers_WhenUsersExist()
    {
        // Arrange
        var dbContext = CreateDbContext();

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
        var handler = new GetAllUsersQueryHandler(repo);

        var query = new GetAllUsersQuery(0, 10);

        // Act
        var result = await handler.Handle(query, default);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Single(result.Value.Items);
        Assert.Equal(appUser.Id, result.Value.Items.First().Id);
        Assert.Equal(identityUser.UserName, result.Value.Items.First().UserName);
    }
}
