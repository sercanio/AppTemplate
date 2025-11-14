using System.Linq.Expressions;
using AppTemplate.Application.Data.Pagination;
using AppTemplate.Application.Repositories;
using AppTemplate.Application.Services.AppUsers;
using AppTemplate.Application.Services.Roles;
using AppTemplate.Domain.AppUsers;
using AppTemplate.Domain.Roles;
using Ardalis.Result;
using Moq;

namespace AppTemplate.Application.Tests.Unit.Services.AppUsersServiceTests;

[Trait("Category", "Unit")]
public class AppUsersServiceUnitTests
{
  private readonly Mock<IAppUsersRepository> _userRepositoryMock;
  private readonly Mock<IRolesService> _rolesServiceMock;
  private readonly AppUsersService _service;

  public AppUsersServiceUnitTests()
  {
    _userRepositoryMock = new Mock<IAppUsersRepository>();
    _rolesServiceMock = new Mock<IRolesService>();
    _service = new AppUsersService(_userRepositoryMock.Object, _rolesServiceMock.Object);
  }

  [Fact]
  public async Task AddAsync_CallsRepositoryAddAsync()
  {
    // Arrange
    var user = AppUser.Create();
    var defaultRole = Role.DefaultRole; // Use the static default role

    _rolesServiceMock
      .Setup(r => r.GetDefaultRole(It.IsAny<CancellationToken>()))
      .ReturnsAsync(Result.Success(defaultRole));

    // Act
    await _service.AddAsync(user);

    // Assert
    _userRepositoryMock.Verify(r => r.AddAsync(user, It.IsAny<CancellationToken>()), Times.Once);
  }

  [Fact]
  public void Delete_CallsRepositoryDelete()
  {
    var user = AppUser.Create();
    _service.Delete(user);
    _userRepositoryMock.Verify(r => r.Delete(user, true, It.IsAny<CancellationToken>()), Times.Once);
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

  [Fact]
  public async Task GetUserByIdAsync_ReturnsUser()
  {
    // Arrange
    var user = AppUser.Create();
    var userId = user.Id;

    _userRepositoryMock
      .Setup(r => r.GetAsync(
          It.IsAny<Expression<Func<AppUser, bool>>>(),
          It.IsAny<bool>(),
          It.IsAny<Func<IQueryable<AppUser>, IQueryable<AppUser>>>(),
          It.IsAny<bool>(),
          It.IsAny<CancellationToken>()))
      .ReturnsAsync(user);

    // Act
    var result = await _service.GetUserByIdAsync(userId);

    // Assert
    Assert.Equal(user, result);
    _userRepositoryMock.Verify(r => r.GetAsync(
        It.Is<Expression<Func<AppUser, bool>>>(expr => true),
        It.IsAny<bool>(),
        It.IsAny<Func<IQueryable<AppUser>, IQueryable<AppUser>>>(),
        It.IsAny<bool>(),
        It.IsAny<CancellationToken>()), Times.Once);
  }

  [Fact]
  public async Task GetByIdentityIdAsync_WithValidIdentityId_ReturnsSuccessResult()
  {
    // Arrange
    var user = AppUser.Create();
    var identityId = "test-identity-id";

    _userRepositoryMock
      .Setup(r => r.GetAsync(
          It.IsAny<Expression<Func<AppUser, bool>>>(),
          It.IsAny<bool>(),
          It.IsAny<Func<IQueryable<AppUser>, IQueryable<AppUser>>>(),
          It.IsAny<bool>(),
          It.IsAny<CancellationToken>()))
      .ReturnsAsync(user);

    // Act
    var result = await _service.GetByIdentityIdAsync(identityId);

    // Assert
    Assert.True(result.IsSuccess);
    Assert.Equal(user, result.Value);
  }

  [Fact]
  public async Task GetByIdentityIdAsync_WithEmptyIdentityId_ReturnsErrorResult()
  {
    // Act
    var result = await _service.GetByIdentityIdAsync(string.Empty);

    // Assert
    Assert.True(!result.IsSuccess);
  }

  [Fact]
  public async Task GetByIdentityIdAsync_WithNullIdentityId_ReturnsErrorResult()
  {
    // Act
    var result = await _service.GetByIdentityIdAsync(null!);

    // Assert
    Assert.True(!result.IsSuccess);
  }

  [Fact]
  public async Task GetUsersCountAsync_ReturnsCount()
  {
    // Arrange
    var expectedCount = 5;
    _userRepositoryMock
      .Setup(r => r.GetUsersCountAsync(It.IsAny<bool>(), It.IsAny<CancellationToken>()))
      .ReturnsAsync(expectedCount);

    // Act
    var result = await _service.GetUsersCountAsync();

    // Assert
    Assert.Equal(expectedCount, result);
    _userRepositoryMock.Verify(r => r.GetUsersCountAsync(false, It.IsAny<CancellationToken>()), Times.Once);
  }

  [Fact]
  public void Update_CallsRepositoryUpdate()
  {
    // Arrange
    var user = AppUser.Create();

    // Act
    _service.Update(user);

    // Assert
    _userRepositoryMock.Verify(r => r.Update(user, It.IsAny<CancellationToken>()), Times.Once);
  }
}
