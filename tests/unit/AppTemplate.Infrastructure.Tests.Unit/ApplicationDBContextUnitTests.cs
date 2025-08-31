using AppTemplate.Core.Application.Abstractions.Clock;
using AppTemplate.Domain.AppUsers;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace AppTemplate.Infrastructure.Tests.Unit;

[Trait("Category", "Unit")]
public class ApplicationDbContextTests
{
  [Fact]
  public void ClearChangeTracker_ShouldClearTrackedEntities()
  {
    var options = new DbContextOptionsBuilder<ApplicationDbContext>()
        .UseInMemoryDatabase(Guid.NewGuid().ToString())
        .Options;
    var dateTimeProvider = new Mock<IDateTimeProvider>().Object;
    var dbContext = new ApplicationDbContext(options, dateTimeProvider);

    dbContext.AppUsers.Add(AppUser.Create());

    Assert.True(dbContext.ChangeTracker.Entries().Any());

    dbContext.ClearChangeTracker();

    Assert.False(dbContext.ChangeTracker.Entries().Any());
  }
}
