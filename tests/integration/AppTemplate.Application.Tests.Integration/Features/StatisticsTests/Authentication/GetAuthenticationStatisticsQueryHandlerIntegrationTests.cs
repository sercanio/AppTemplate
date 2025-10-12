using AppTemplate.Application.Features.Statistics.Authentication.Queries.GetAuthenticationStatistics;
using AppTemplate.Application.Services.Statistics;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Moq;


namespace AppTemplate.Application.Tests.Integration.Features.StatisticsTests.Authentication;

public class GetAuthenticationStatisticsQueryHandlerIntegrationTests
{
  private class FakeSessionService : IActiveSessionService
  {
    private readonly int _activeSessions;
    public FakeSessionService(int activeSessions) => _activeSessions = activeSessions;

    public Task<int> GetActiveSessionsCountAsync() => Task.FromResult(_activeSessions);

    public Task RecordUserActivityAsync(string userId) => Task.CompletedTask;

    public Task RemoveUserSessionAsync(string userId) => Task.CompletedTask;

    public Task<Dictionary<string, DateTime>> GetActiveSessionsAsync() =>
        Task.FromResult(new Dictionary<string, DateTime>());
  }

  private UserManager<IdentityUser> CreateUserManager(DbContextOptions<IdentityDbContext> options)
  {
    var store = new UserStore<IdentityUser>(new IdentityDbContext(options));
    return new UserManager<IdentityUser>(store, null, null, null, null, null, null, null, null);
  }

  [Fact]
  public async Task Handle_ReturnsCorrectStatistics_WhenUsersExist()
  {
    // Arrange
    var options = new DbContextOptionsBuilder<IdentityDbContext>()
        .UseInMemoryDatabase(Guid.NewGuid().ToString())
        .Options;
    var dbContext = new IdentityDbContext(options);

    var user1 = new IdentityUser { UserName = "user1", Email = "user1@example.com", TwoFactorEnabled = true };
    var user2 = new IdentityUser { UserName = "user2", Email = "user2@example.com", TwoFactorEnabled = false };
    var user3 = new IdentityUser { UserName = "user3", Email = "user3@example.com", TwoFactorEnabled = true };
    dbContext.Users.AddRange(user1, user2, user3);
    await dbContext.SaveChangesAsync();

    // Create a mock UserManager instead of a real one
    var userManagerMock = new Mock<UserManager<IdentityUser>>(
        Mock.Of<IUserStore<IdentityUser>>(),
        null, null, null, null, null, null, null, null);

    // Setup the Users property to return our test users
    userManagerMock.Setup(x => x.Users)
        .Returns(dbContext.Users);

    // Setup GetAuthenticatorKeyAsync calls
    userManagerMock.Setup(x => x.GetAuthenticatorKeyAsync(It.Is<IdentityUser>(u => u.UserName == "user1")))
        .ReturnsAsync("key1");
    userManagerMock.Setup(x => x.GetAuthenticatorKeyAsync(It.Is<IdentityUser>(u => u.UserName == "user2")))
        .ReturnsAsync((string)null);
    userManagerMock.Setup(x => x.GetAuthenticatorKeyAsync(It.Is<IdentityUser>(u => u.UserName == "user3")))
        .ReturnsAsync("key3");

    var sessionService = new FakeSessionService(3);

    var handler = new GetAuthenticationStatisticsQueryHandler(sessionService, userManagerMock.Object);

    // Act
    var result = await handler.Handle(new GetAuthenticationStatisticsQuery(), default);

    // Assert
    Assert.True(result.IsSuccess);
    Assert.NotNull(result.Value);
    Assert.Equal(3, result.Value.ActiveSessions);
    Assert.Equal(0, result.Value.SuccessfulLogins);
    Assert.Equal(0, result.Value.FailedLogins);
    Assert.Equal(2, result.Value.TwoFactorEnabled);
    Assert.Equal(2, result.Value.TotalUsersWithAuthenticator);
  }

  [Fact]
  public async Task Handle_ReturnsZero_WhenNoUsersExist()
  {
    // Arrange
    var options = new DbContextOptionsBuilder<IdentityDbContext>()
        .UseInMemoryDatabase(Guid.NewGuid().ToString())
        .Options;
    var dbContext = new IdentityDbContext(options);

    var userManager = new UserManager<IdentityUser>(
        new UserStore<IdentityUser>(dbContext),
        null, null, null, null, null, null, null, null);

    var sessionService = new FakeSessionService(0);

    var handler = new GetAuthenticationStatisticsQueryHandler(sessionService, userManager);

    // Act
    var result = await handler.Handle(new GetAuthenticationStatisticsQuery(), default);

    // Assert
    Assert.True(result.IsSuccess);
    Assert.NotNull(result.Value);
    Assert.Equal(0, result.Value.ActiveSessions);
    Assert.Equal(0, result.Value.SuccessfulLogins);
    Assert.Equal(0, result.Value.FailedLogins);
    Assert.Equal(0, result.Value.TwoFactorEnabled);
    Assert.Equal(0, result.Value.TotalUsersWithAuthenticator);
  }
}
