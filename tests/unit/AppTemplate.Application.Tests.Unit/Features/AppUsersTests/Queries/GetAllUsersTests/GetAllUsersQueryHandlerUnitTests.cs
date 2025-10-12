using AppTemplate.Application.Data.Pagination;
using AppTemplate.Application.Features.AppUsers.Queries.GetAllUsers;
using AppTemplate.Application.Repositories;
using AppTemplate.Domain.AppUsers;
using AppTemplate.Domain.Roles;
using AppTemplate.Domain.Roles.ValueObjects;
using Ardalis.Result;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Moq;
using Xunit;

namespace AppTemplate.Application.Tests.Unit.Features.AppUsersTests.Queries.GetAllUsersTests;

[Trait("Category", "Unit")]
public class GetAllUsersQueryHandlerUnitTests
{
  private readonly Mock<IAppUsersRepository> _userRepositoryMock;
  private readonly GetAllUsersQueryHandler _handler;

  public GetAllUsersQueryHandlerUnitTests()
  {
    _userRepositoryMock = new Mock<IAppUsersRepository>();
    _handler = new GetAllUsersQueryHandler(_userRepositoryMock.Object);
  }

  #region Error Cases

  [Fact]
  public async Task Handle_ReturnsError_WhenRepositoryReturnsError()
  {
    // Arrange
    var query = new GetAllUsersQuery(0, 10);

    _userRepositoryMock
        .Setup(r => r.GetAllUsersWithIdentityAndRolesAsync(0, 10, It.IsAny<CancellationToken>()))
        .ReturnsAsync(Result<PaginatedList<AppUser>>.Error("Database error"));

    // Act
    var result = await _handler.Handle(query, default);

    // Assert
    result.IsSuccess.Should().BeFalse();
    result.Errors.Should().ContainSingle()
        .Which.Should().Be("Could not retrieve users.");
  }

  [Fact]
  public async Task Handle_ReturnsError_WhenRepositoryReturnsNull()
  {
    // Arrange
    var query = new GetAllUsersQuery(0, 10);

    _userRepositoryMock
        .Setup(r => r.GetAllUsersWithIdentityAndRolesAsync(0, 10, It.IsAny<CancellationToken>()))
        .ReturnsAsync(Result.Success<PaginatedList<AppUser>>(null!));

    // Act
    var result = await _handler.Handle(query, default);

    // Assert
    result.IsSuccess.Should().BeFalse();
    result.Errors.Should().ContainSingle()
        .Which.Should().Be("Could not retrieve users.");
  }

  [Fact]
  public async Task Handle_ReturnsError_WhenRepositoryReturnsNotFoundStatus()
  {
    // Arrange
    var query = new GetAllUsersQuery(0, 10);

    _userRepositoryMock
        .Setup(r => r.GetAllUsersWithIdentityAndRolesAsync(0, 10, It.IsAny<CancellationToken>()))
        .ReturnsAsync(Result<PaginatedList<AppUser>>.NotFound("Users not found"));

    // Act
    var result = await _handler.Handle(query, default);

    // Assert
    result.IsSuccess.Should().BeFalse();
    result.Errors.Should().ContainSingle()
        .Which.Should().Be("Could not retrieve users.");
  }

  #endregion

  #region Success Cases - Empty Results

  [Fact]
  public async Task Handle_ReturnsEmptyList_WhenNoUsersExist()
  {
    // Arrange
    var query = new GetAllUsersQuery(0, 10);
    var emptyPaginatedList = new PaginatedList<AppUser>([], 0, 0, 10);

    _userRepositoryMock
        .Setup(r => r.GetAllUsersWithIdentityAndRolesAsync(0, 10, It.IsAny<CancellationToken>()))
        .ReturnsAsync(Result.Success(emptyPaginatedList));

    // Act
    var result = await _handler.Handle(query, default);

    // Assert
    result.IsSuccess.Should().BeTrue();
    result.Value.Should().NotBeNull();
    result.Value.Items.Should().BeEmpty();
    result.Value.TotalCount.Should().Be(0);
    result.Value.PageIndex.Should().Be(0);
    result.Value.PageSize.Should().Be(10);
  }

  #endregion

  #region Success Cases - Single User

  [Fact]
  public async Task Handle_ReturnsUserWithoutIdentity_WhenIdentityUserIsNull()
  {
    // Arrange
    var query = new GetAllUsersQuery(0, 10);
    var user = AppUser.Create();
    var users = new List<AppUser> { user };
    var paginatedList = new PaginatedList<AppUser>(users, 1, 0, 10);

    _userRepositoryMock
        .Setup(r => r.GetAllUsersWithIdentityAndRolesAsync(0, 10, It.IsAny<CancellationToken>()))
        .ReturnsAsync(Result.Success(paginatedList));

    // Act
    var result = await _handler.Handle(query, default);

    // Assert
    result.IsSuccess.Should().BeTrue();
    result.Value.Should().NotBeNull();
    result.Value.Items.Should().ContainSingle();
    var responseUser = result.Value.Items.First();
    responseUser.Id.Should().Be(user.Id);
    responseUser.UserName.Should().Be(string.Empty);
    responseUser.EmailConfirmed.Should().BeFalse();
    responseUser.Roles.Should().BeEmpty();
  }

  [Fact]
  public async Task Handle_ReturnsUserWithIdentity_WhenIdentityUserExists()
  {
    // Arrange
    var query = new GetAllUsersQuery(0, 10);
    var identityUser = new IdentityUser
    {
      Id = "identity-123",
      UserName = "testuser",
      Email = "test@example.com",
      EmailConfirmed = true
    };

    var user = AppUser.Create();
    user.SetIdentityId(identityUser.Id);
    // Use reflection to set IdentityUser since there's no public setter
    var identityUserProperty = typeof(AppUser).GetProperty(nameof(AppUser.IdentityUser));
    identityUserProperty?.SetValue(user, identityUser);

    var users = new List<AppUser> { user };
    var paginatedList = new PaginatedList<AppUser>(users, 1, 0, 10);

    _userRepositoryMock
        .Setup(r => r.GetAllUsersWithIdentityAndRolesAsync(0, 10, It.IsAny<CancellationToken>()))
        .ReturnsAsync(Result.Success(paginatedList));

    // Act
    var result = await _handler.Handle(query, default);

    // Assert
    result.IsSuccess.Should().BeTrue();
    result.Value.Should().NotBeNull();
    result.Value.Items.Should().ContainSingle();
    var responseUser = result.Value.Items.First();
    responseUser.Id.Should().Be(user.Id);
    responseUser.UserName.Should().Be("testuser");
    responseUser.EmailConfirmed.Should().BeTrue();
    responseUser.JoinDate.Should().Be(user.CreatedOnUtc);
  }

  [Fact]
  public async Task Handle_ReturnsUserWithoutRoles_WhenRolesAreNull()
  {
    // Arrange
    var query = new GetAllUsersQuery(0, 10);
    var user = AppUser.Create();
    var users = new List<AppUser> { user };
    var paginatedList = new PaginatedList<AppUser>(users, 1, 0, 10);

    _userRepositoryMock
        .Setup(r => r.GetAllUsersWithIdentityAndRolesAsync(0, 10, It.IsAny<CancellationToken>()))
        .ReturnsAsync(Result.Success(paginatedList));

    // Act
    var result = await _handler.Handle(query, default);

    // Assert
    result.IsSuccess.Should().BeTrue();
    result.Value.Items.First().Roles.Should().BeEmpty();
  }

  [Fact]
  public async Task Handle_ReturnsUserWithoutRoles_WhenRolesAreEmpty()
  {
    // Arrange
    var query = new GetAllUsersQuery(0, 10);
    var user = AppUser.Create();
    // Roles collection exists but is empty (no roles added)
    var users = new List<AppUser> { user };
    var paginatedList = new PaginatedList<AppUser>(users, 1, 0, 10);

    _userRepositoryMock
        .Setup(r => r.GetAllUsersWithIdentityAndRolesAsync(0, 10, It.IsAny<CancellationToken>()))
        .ReturnsAsync(Result.Success(paginatedList));

    // Act
    var result = await _handler.Handle(query, default);

    // Assert
    result.IsSuccess.Should().BeTrue();
    result.Value.Items.First().Roles.Should().BeEmpty();
  }

  #endregion

  #region Success Cases - Users with Roles

  [Fact]
  public async Task Handle_ReturnsUserWithActiveRoles_WhenUserHasRoles()
  {
    // Arrange
    var query = new GetAllUsersQuery(0, 10);
    var user = AppUser.Create();
    var createdById = Guid.NewGuid();
    
    var adminRole = Role.Create("Admin", "Administrator", createdById);
    var userRole = Role.Create("User", "Regular User", createdById);
    
    user.AddRole(adminRole);
    user.AddRole(userRole);

    var users = new List<AppUser> { user };
    var paginatedList = new PaginatedList<AppUser>(users, 1, 0, 10);

    _userRepositoryMock
        .Setup(r => r.GetAllUsersWithIdentityAndRolesAsync(0, 10, It.IsAny<CancellationToken>()))
        .ReturnsAsync(Result.Success(paginatedList));

    // Act
    var result = await _handler.Handle(query, default);

    // Assert
    result.IsSuccess.Should().BeTrue();
    var responseUser = result.Value.Items.First();
    responseUser.Roles.Should().HaveCount(2);
    responseUser.Roles.Should().Contain(r => r.Name == "Admin" && r.DisplayName == "Administrator");
    responseUser.Roles.Should().Contain(r => r.Name == "User" && r.DisplayName == "Regular User");
  }

  [Fact]
  public async Task Handle_ExcludesDeletedRoles_WhenUserHasDeletedRoles()
  {
    // Arrange
    var query = new GetAllUsersQuery(0, 10);
    var user = AppUser.Create();
    var createdById = Guid.NewGuid();
    var deletedById = Guid.NewGuid();
    
    var activeRole = Role.Create("Admin", "Administrator", createdById);
    var deletedRole = Role.Create("OldRole", "Old Role", createdById);
    
    // Mark role as deleted using the static Delete method
    Role.Delete(deletedRole, deletedById);
    
    user.AddRole(activeRole);
    user.AddRole(deletedRole);

    var users = new List<AppUser> { user };
    var paginatedList = new PaginatedList<AppUser>(users, 1, 0, 10);

    _userRepositoryMock
        .Setup(r => r.GetAllUsersWithIdentityAndRolesAsync(0, 10, It.IsAny<CancellationToken>()))
        .ReturnsAsync(Result.Success(paginatedList));

    // Act
    var result = await _handler.Handle(query, default);

    // Assert
    result.IsSuccess.Should().BeTrue();
    var responseUser = result.Value.Items.First();
    responseUser.Roles.Should().ContainSingle();
    responseUser.Roles.First().Name.Should().Be("Admin");
  }

  #endregion

  #region Success Cases - Multiple Users

  [Fact]
  public async Task Handle_ReturnsMultipleUsers_WhenMultipleUsersExist()
  {
    // Arrange
    var query = new GetAllUsersQuery(0, 10);
    
    var user1 = AppUser.Create();
    var identityUser1 = new IdentityUser { Id = "id1", UserName = "user1", EmailConfirmed = true };
    user1.SetIdentityId(identityUser1.Id);
    var identityUserProperty = typeof(AppUser).GetProperty(nameof(AppUser.IdentityUser));
    identityUserProperty?.SetValue(user1, identityUser1);

    var user2 = AppUser.Create();
    var identityUser2 = new IdentityUser { Id = "id2", UserName = "user2", EmailConfirmed = false };
    user2.SetIdentityId(identityUser2.Id);
    identityUserProperty?.SetValue(user2, identityUser2);

    var user3 = AppUser.Create();
    // user3 has no identity

    var users = new List<AppUser> { user1, user2, user3 };
    var paginatedList = new PaginatedList<AppUser>(users, 3, 0, 10);

    _userRepositoryMock
        .Setup(r => r.GetAllUsersWithIdentityAndRolesAsync(0, 10, It.IsAny<CancellationToken>()))
        .ReturnsAsync(Result.Success(paginatedList));

    // Act
    var result = await _handler.Handle(query, default);

    // Assert
    result.IsSuccess.Should().BeTrue();
    result.Value.Items.Should().HaveCount(3);
    result.Value.TotalCount.Should().Be(3);
    result.Value.Items.Should().Contain(u => u.UserName == "user1" && u.EmailConfirmed);
    result.Value.Items.Should().Contain(u => u.UserName == "user2" && !u.EmailConfirmed);
    result.Value.Items.Should().Contain(u => u.UserName == string.Empty);
  }

  #endregion

  #region Pagination Tests

  [Fact]
  public async Task Handle_RespectsPaginationParameters_WithCustomPageSize()
  {
    // Arrange
    var query = new GetAllUsersQuery(1, 5);
    var users = new List<AppUser> { AppUser.Create(), AppUser.Create() };
    var paginatedList = new PaginatedList<AppUser>(users, 10, 1, 5);

    _userRepositoryMock
        .Setup(r => r.GetAllUsersWithIdentityAndRolesAsync(1, 5, It.IsAny<CancellationToken>()))
        .ReturnsAsync(Result.Success(paginatedList));

    // Act
    var result = await _handler.Handle(query, default);

    // Assert
    result.IsSuccess.Should().BeTrue();
    result.Value.PageIndex.Should().Be(1);
    result.Value.PageSize.Should().Be(5);
    result.Value.TotalCount.Should().Be(10);
    result.Value.Items.Should().HaveCount(2);
  }

  [Theory]
  [InlineData(0, 10)]
  [InlineData(1, 20)]
  [InlineData(5, 50)]
  public async Task Handle_WorksWithVariousPaginationValues(int pageIndex, int pageSize)
  {
    // Arrange
    var query = new GetAllUsersQuery(pageIndex, pageSize);
    var users = new List<AppUser> { AppUser.Create() };
    var paginatedList = new PaginatedList<AppUser>(users, 100, pageIndex, pageSize);

    _userRepositoryMock
        .Setup(r => r.GetAllUsersWithIdentityAndRolesAsync(pageIndex, pageSize, It.IsAny<CancellationToken>()))
        .ReturnsAsync(Result.Success(paginatedList));

    // Act
    var result = await _handler.Handle(query, default);

    // Assert
    result.IsSuccess.Should().BeTrue();
    result.Value.PageIndex.Should().Be(pageIndex);
    result.Value.PageSize.Should().Be(pageSize);
    _userRepositoryMock.Verify(r => r.GetAllUsersWithIdentityAndRolesAsync(pageIndex, pageSize, It.IsAny<CancellationToken>()), Times.Once);
  }

  #endregion

  #region Cancellation Token Tests

  [Fact]
  public async Task Handle_PassesCancellationToken_ToRepository()
  {
    // Arrange
    var query = new GetAllUsersQuery(0, 10);
    var cancellationToken = new CancellationToken();
    var paginatedList = new PaginatedList<AppUser>([], 0, 0, 10);

    _userRepositoryMock
        .Setup(r => r.GetAllUsersWithIdentityAndRolesAsync(0, 10, cancellationToken))
        .ReturnsAsync(Result.Success(paginatedList));

    // Act
    await _handler.Handle(query, cancellationToken);

    // Assert
    _userRepositoryMock.Verify(r => r.GetAllUsersWithIdentityAndRolesAsync(0, 10, cancellationToken), Times.Once);
  }

  #endregion

  #region Complex Scenarios

  [Fact]
  public async Task Handle_ReturnsCompleteUserInformation_WithAllFieldsPopulated()
  {
    // Arrange
    var query = new GetAllUsersQuery(0, 10);
    var createdById = Guid.NewGuid();

    var user = AppUser.Create();
    var identityUser = new IdentityUser
    {
      Id = "complete-user",
      UserName = "completeuser",
      Email = "complete@example.com",
      EmailConfirmed = true,
      PhoneNumber = "+1234567890",
      PhoneNumberConfirmed = true
    };

    user.SetIdentityId(identityUser.Id);
    var identityUserProperty = typeof(AppUser).GetProperty(nameof(AppUser.IdentityUser));
    identityUserProperty?.SetValue(user, identityUser);

    var role1 = Role.Create("Admin", "Administrator", createdById);
    var role2 = Role.Create("Manager", "Manager", createdById);
    user.AddRole(role1);
    user.AddRole(role2);

    // Don't try to set CreatedOnUtc - it's already set when the user was created
    var expectedCreatedDate = user.CreatedOnUtc;

    var users = new List<AppUser> { user };
    var paginatedList = new PaginatedList<AppUser>(users, 1, 0, 10);

    _userRepositoryMock
        .Setup(r => r.GetAllUsersWithIdentityAndRolesAsync(0, 10, It.IsAny<CancellationToken>()))
        .ReturnsAsync(Result.Success(paginatedList));

    // Act
    var result = await _handler.Handle(query, default);

    // Assert
    result.IsSuccess.Should().BeTrue();
    var responseUser = result.Value.Items.First();
    responseUser.Id.Should().Be(user.Id);
    responseUser.UserName.Should().Be("completeuser");
    responseUser.EmailConfirmed.Should().BeTrue();
    responseUser.JoinDate.Should().Be(expectedCreatedDate);
    responseUser.Roles.Should().HaveCount(2);
    responseUser.Roles.Should().Contain(r => r.Name == "Admin" && r.DisplayName == "Administrator");
    responseUser.Roles.Should().Contain(r => r.Name == "Manager" && r.DisplayName == "Manager");
  }

  #endregion
}
