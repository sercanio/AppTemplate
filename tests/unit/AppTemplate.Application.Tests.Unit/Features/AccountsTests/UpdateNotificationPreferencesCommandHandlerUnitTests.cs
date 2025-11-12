using AppTemplate.Application.Features.Accounts.UpdateNotificationPreferences;
using AppTemplate.Application.Services.Authentication;
using AppTemplate.Application.Services.Caching;
using AppTemplate.Application.Services.Clock;
using AppTemplate.Domain;
using AppTemplate.Domain.AppUsers;
using AppTemplate.Domain.AppUsers.ValueObjects;
using AppTemplate.Infrastructure;
using AppTemplate.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Moq;

namespace AppTemplate.Application.Tests.Unit.Features.AccountsTests;

public class UpdateNotificationPreferencesCommandHandlerIntegrationTests
{
  private ApplicationDbContext CreateDbContext()
  {
    var options = new DbContextOptionsBuilder<ApplicationDbContext>()
        .UseInMemoryDatabase(Guid.NewGuid().ToString())
        .Options;
    return new ApplicationDbContext(options, new DateTimeProvider());
  }

  [Fact]
  public async Task Handle_UpdatesNotificationPreferences_WhenUserExists()
  {
    // Arrange
    var dbContext = CreateDbContext();

    var appUser = AppUser.Create();
    appUser.SetIdentityId("user-1");
    appUser.NotificationPreference.Update(false, false, false);
    dbContext.AppUsers.Add(appUser);
    await dbContext.SaveChangesAsync();

    var userRepository = new AppUsersRepository(dbContext);
    IUnitOfWork unitOfWork = dbContext;
    var cacheService = new CacheService(new Mock<IDistributedCache>().Object);

    var userContextMock = new Mock<IUserContext>();
    userContextMock.Setup(x => x.IdentityId).Returns("user-1");

    var handler = new UpdateNotificationPreferencesCommandHandler(
        userRepository,
        unitOfWork,
        cacheService,
        userContextMock.Object);

    var command = new UpdateNotificationPreferencesCommand(
        new NotificationPreference(true, true, false));

    // Act
    var result = await handler.Handle(command, default);

    // Assert
    Assert.Equal(Ardalis.Result.ResultStatus.Ok, result.Status);
    Assert.NotNull(result.Value);
    Assert.Equal(appUser.Id, result.Value.UserId);
    Assert.True(result.Value.NotificationPreference.IsInAppNotificationEnabled);
    Assert.True(result.Value.NotificationPreference.IsEmailNotificationEnabled);
    Assert.False(result.Value.NotificationPreference.IsPushNotificationEnabled);
  }

  [Fact]
  public async Task Handle_ReturnsNotFound_WhenUserDoesNotExist()
  {
    // Arrange
    var dbContext = CreateDbContext();
    var userRepository = new AppUsersRepository(dbContext);
    IUnitOfWork unitOfWork = dbContext;
    var cacheService = new CacheService(new Mock<Microsoft.Extensions.Caching.Distributed.IDistributedCache>().Object);

    var userContextMock = new Mock<IUserContext>();
    userContextMock.Setup(x => x.IdentityId).Returns("user-1");

    var handler = new UpdateNotificationPreferencesCommandHandler(
        userRepository,
        unitOfWork,
        cacheService,
        userContextMock.Object);

    var command = new UpdateNotificationPreferencesCommand(
        new NotificationPreference(true, true, false));

    // Act
    var result = await handler.Handle(command, default);

    // Assert
    Assert.Equal(Ardalis.Result.ResultStatus.NotFound, result.Status);
  }
}
