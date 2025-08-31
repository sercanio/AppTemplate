using AppTemplate.Application.Services.Authorization;
using AppTemplate.Core.Application.Abstractions.Caching;
using AppTemplate.Core.Application.Abstractions.Clock;
using AppTemplate.Infrastructure.Authorization;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace AppTemplate.Infrastructure.Tests.Unit.AuthorizationTests;

[Trait("Category", "Unit")]
public class AuthorizationServiceTests
{
  [Fact]
  public async Task GetRolesForUserAsync_ReturnsCachedRoles_IfPresent()
  {
    var options = new DbContextOptionsBuilder<ApplicationDbContext>()
        .UseInMemoryDatabase("TestDb")
        .Options;
    var dateTimeProvider = new Mock<IDateTimeProvider>().Object;
    var dbContext = new ApplicationDbContext(options, dateTimeProvider);

    var cacheServiceMock = new Mock<ICacheService>();
    var identityId = "test-id";
    var expectedRoles = new UserRolesResponse { UserId = Guid.NewGuid(), Roles = [] };

    cacheServiceMock.Setup(c => c.GetAsync<UserRolesResponse>($"auth:roles-{identityId}", default))
        .ReturnsAsync(expectedRoles);

    var service = new AuthorizationService(dbContext, cacheServiceMock.Object);

    var result = await service.GetRolesForUserAsync(identityId);

    Assert.Equal(expectedRoles, result);
    cacheServiceMock.Verify(c => c.GetAsync<UserRolesResponse>($"auth:roles-{identityId}", default), Times.Once);
  }
}
