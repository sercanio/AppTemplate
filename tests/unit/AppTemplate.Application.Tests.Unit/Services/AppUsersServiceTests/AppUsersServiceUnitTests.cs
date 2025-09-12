using AppTemplate.Application.Data.Pagination;
using AppTemplate.Application.Repositories;
using AppTemplate.Application.Services.AppUsers;
using AppTemplate.Domain.AppUsers;
using Moq;
using System.Linq.Expressions;

namespace AppTemplate.Application.Tests.Unit.Services.AppUsersServiceTests;

[Trait("Category", "Unit")]
public class AppUsersServiceUnitTests
{
  private readonly Mock<IAppUsersRepository> _userRepositoryMock;
  private readonly AppUsersService _service;

  public AppUsersServiceUnitTests()
  {
    _userRepositoryMock = new Mock<IAppUsersRepository>();
    _service = new AppUsersService(_userRepositoryMock.Object);
  }

  [Fact]
  public async Task AddAsync_CallsRepositoryAddAsync()
  {
    var user = AppUser.Create();
    await _service.AddAsync(user);
    _userRepositoryMock.Verify(r => r.AddAsync(user), Times.Once);
  }

  [Fact]
  public void Delete_CallsRepositoryDelete()
  {
    var user = AppUser.Create();
    _service.Delete(user);
    _userRepositoryMock.Verify(r => r.Delete(user, true), Times.Once);
  }

  [Fact]
  public async Task GetAllAsync_ReturnsPaginatedList()
  {
    var paginatedList = new PaginatedList<AppUser>(new List<AppUser>(), 0, 0, 10);
    _userRepositoryMock
      .Setup(r => r.GetAllAsync(
          It.IsAny<int>(),
          It.IsAny<int>(),
          It.IsAny<Expression<Func<AppUser, bool>>>(),
          It.IsAny<bool>(),
          It.IsAny<Func<IQueryable<AppUser>, IQueryable<AppUser>>>(),
          It.IsAny<bool>(),
          It.IsAny<CancellationToken>()))
      .ReturnsAsync(paginatedList);

    var result = await _service.GetAllAsync();

    Assert.NotNull(result);
    Assert.Equal(0, result.TotalCount);
  }

  [Fact]
  public async Task GetAsync_ReturnsUser()
  {
    var user = AppUser.Create();
    var someId = user.Id;
    _userRepositoryMock
          .Setup(r => r.GetAsync(
              It.IsAny<Expression<Func<AppUser, bool>>>(),
              false,
              null,
              true,
              It.IsAny<CancellationToken>()))
          .ReturnsAsync(user);

    // Predicate only uses property access, not method calls with optional args
    Expression<Func<AppUser, bool>> predicate = u => u.Id == someId;

    var result = await _service.GetAsync(
        predicate: predicate,
        includeSoftDeleted: false,
        include: null,
        asNoTracking: true,
        cancellationToken: CancellationToken.None);

    Assert.Equal(user, result);
  }

}
