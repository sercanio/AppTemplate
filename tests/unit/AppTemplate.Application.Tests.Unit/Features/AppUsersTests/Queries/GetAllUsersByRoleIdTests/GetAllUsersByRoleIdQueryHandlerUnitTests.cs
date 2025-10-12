using AppTemplate.Application.Data.Pagination;
using AppTemplate.Application.Features.AppUsers.Queries.GetAllUsersByRoleId;
using AppTemplate.Application.Repositories;
using AppTemplate.Domain.AppUsers;
using AppTemplate.Domain.Roles;
using Ardalis.Result;
using Microsoft.AspNetCore.Identity;
using Moq;

namespace AppTemplate.Application.Tests.Unit.Features.AppUsersTests.Queries.GetAllUsersByRoleIdTests;

[Trait("Category", "Unit")]
public class GetAllUsersByRoleIdQueryHandlerUnitTests
{
  private readonly Mock<IAppUsersRepository> _userRepositoryMock;
  private readonly GetAllUsersByRoleIdQueryHandler _handler;

  public GetAllUsersByRoleIdQueryHandlerUnitTests()
  {
    _userRepositoryMock = new Mock<IAppUsersRepository>();
    _handler = new GetAllUsersByRoleIdQueryHandler(_userRepositoryMock.Object);
  }

  [Fact]
  public async Task Handle_ReturnsError_WhenRepositoryReturnsError()
  {
    // Arrange
    var roleId = Guid.NewGuid();
    var query = new GetAllUsersByRoleIdQuery(0, 10, roleId);

    _userRepositoryMock
        .Setup(r => r.GetAllUsersByRoleIdWithIdentityAndRolesAsync(roleId, 0, 10, It.IsAny<CancellationToken>()))
        .ReturnsAsync(Result<PaginatedList<AppUser>>.Error("error"));

    // Act
    var result = await _handler.Handle(query, default);

    // Assert
    Assert.False(result.IsSuccess);
    Assert.Equal(ResultStatus.Error, result.Status);
    Assert.Equal("Could not retrieve users.", result.Errors.First());
  }

  [Fact]
  public async Task Handle_ReturnsError_WhenRepositoryReturnsNull()
  {
    // Arrange
    var roleId = Guid.NewGuid();
    var query = new GetAllUsersByRoleIdQuery(0, 10, roleId);

    _userRepositoryMock
        .Setup(r => r.GetAllUsersByRoleIdWithIdentityAndRolesAsync(roleId, 0, 10, It.IsAny<CancellationToken>()))
        .ReturnsAsync(Result.Success<PaginatedList<AppUser>>(null));

    // Act
    var result = await _handler.Handle(query, default);

    // Assert
    Assert.False(result.IsSuccess);
    Assert.Equal(ResultStatus.Error, result.Status);
    Assert.Equal("Could not retrieve users.", result.Errors.First());
  }

  [Fact]
  public async Task Handle_ReturnsSuccess_WithMappedUsers_WhenUsersFound()
  {
    // Arrange
    var roleId = Guid.NewGuid();
    var query = new GetAllUsersByRoleIdQuery(0, 10, roleId);

    var role = Role.Create("Admin", "Administrator", Guid.NewGuid());

    var identityUser1 = new IdentityUser
    {
      Id = "user-1",
      UserName = "testuser1",
      Email = "test1@example.com"
    };

    var identityUser2 = new IdentityUser
    {
      Id = "user-2",
      UserName = "testuser2",
      Email = "test2@example.com"
    };

    var appUser1 = AppUser.Create();
    appUser1.SetIdentityId(identityUser1.Id);
    appUser1.AddRole(role);
    var identityUserProperty = typeof(AppUser).GetProperty("IdentityUser");
    identityUserProperty?.SetValue(appUser1, identityUser1);

    var appUser2 = AppUser.Create();
    appUser2.SetIdentityId(identityUser2.Id);
    appUser2.AddRole(role);
    identityUserProperty?.SetValue(appUser2, identityUser2);

    var users = new List<AppUser> { appUser1, appUser2 };
    var paginatedList = new PaginatedList<AppUser>(users, 2, 0, 10);

    _userRepositoryMock
        .Setup(r => r.GetAllUsersByRoleIdWithIdentityAndRolesAsync(roleId, 0, 10, It.IsAny<CancellationToken>()))
        .ReturnsAsync(Result.Success(paginatedList));

    // Act
    var result = await _handler.Handle(query, default);

    // Assert
    Assert.True(result.IsSuccess);
    Assert.NotNull(result.Value);
    Assert.Equal(2, result.Value.Items.Count);
    Assert.Equal(2, result.Value.TotalCount);
    Assert.Equal("testuser1", result.Value.Items[0].UserName);
    Assert.Equal("testuser2", result.Value.Items[1].UserName);
    Assert.Single(result.Value.Items[0].Roles);
    Assert.Equal("Admin", result.Value.Items[0].Roles.First().Name);
  }

  [Fact]
  public async Task Handle_ReturnsEmptyList_WhenNoUsersFound()
  {
    // Arrange
    var roleId = Guid.NewGuid();
    var query = new GetAllUsersByRoleIdQuery(0, 10, roleId);

    var emptyList = new PaginatedList<AppUser>(new List<AppUser>(), 0, 0, 10);

    _userRepositoryMock
        .Setup(r => r.GetAllUsersByRoleIdWithIdentityAndRolesAsync(roleId, 0, 10, It.IsAny<CancellationToken>()))
        .ReturnsAsync(Result.Success(emptyList));

    // Act
    var result = await _handler.Handle(query, default);

    // Assert
    Assert.True(result.IsSuccess);
    Assert.NotNull(result.Value);
    Assert.Empty(result.Value.Items);
    Assert.Equal(0, result.Value.TotalCount);
  }

  [Fact]
  public async Task Handle_FiltersOutDeletedRoles_InResponse()
  {
    // Arrange
    var roleId = Guid.NewGuid();
    var query = new GetAllUsersByRoleIdQuery(0, 10, roleId);

    var activeRole = Role.Create("ActiveRole", "Active Role", Guid.NewGuid());
    var deletedRole = Role.Create("DeletedRole", "Deleted Role", Guid.NewGuid());
    Role.Delete(deletedRole, Guid.NewGuid());

    var identityUser = new IdentityUser
    {
      Id = "user-1",
      UserName = "testuser",
      Email = "test@example.com"
    };

    var appUser = AppUser.Create();
    appUser.SetIdentityId(identityUser.Id);
    appUser.AddRole(activeRole);
    appUser.AddRole(deletedRole);

    var identityUserProperty = typeof(AppUser).GetProperty("IdentityUser");
    identityUserProperty?.SetValue(appUser, identityUser);

    var users = new List<AppUser> { appUser };
    var paginatedList = new PaginatedList<AppUser>(users, 1, 0, 10);

    _userRepositoryMock
        .Setup(r => r.GetAllUsersByRoleIdWithIdentityAndRolesAsync(roleId, 0, 10, It.IsAny<CancellationToken>()))
        .ReturnsAsync(Result.Success(paginatedList));

    // Act
    var result = await _handler.Handle(query, default);

    // Assert
    Assert.True(result.IsSuccess);
    Assert.NotNull(result.Value);
    Assert.Single(result.Value.Items);
    Assert.Single(result.Value.Items[0].Roles);
    Assert.Equal("ActiveRole", result.Value.Items[0].Roles.First().Name);
    Assert.DoesNotContain(result.Value.Items[0].Roles, r => r.Name == "DeletedRole");
  }

  [Fact]
  public async Task Handle_HandlesUserWithNullIdentityUser()
  {
    // Arrange
    var roleId = Guid.NewGuid();
    var query = new GetAllUsersByRoleIdQuery(0, 10, roleId);

    var role = Role.Create("Admin", "Administrator", Guid.NewGuid());

    var appUser = AppUser.Create();
    appUser.SetIdentityId("user-1");
    appUser.AddRole(role);
    // IdentityUser is intentionally left null

    var users = new List<AppUser> { appUser };
    var paginatedList = new PaginatedList<AppUser>(users, 1, 0, 10);

    _userRepositoryMock
        .Setup(r => r.GetAllUsersByRoleIdWithIdentityAndRolesAsync(roleId, 0, 10, It.IsAny<CancellationToken>()))
        .ReturnsAsync(Result.Success(paginatedList));

    // Act
    var result = await _handler.Handle(query, default);

    // Assert
    Assert.True(result.IsSuccess);
    Assert.NotNull(result.Value);
    Assert.Single(result.Value.Items);
    Assert.Equal(string.Empty, result.Value.Items[0].UserName);
  }

  [Fact]
  public async Task Handle_HandlesUserWithNullRoles()
  {
    // Arrange
    var roleId = Guid.NewGuid();
    var query = new GetAllUsersByRoleIdQuery(0, 10, roleId);

    var identityUser = new IdentityUser
    {
      Id = "user-1",
      UserName = "testuser",
      Email = "test@example.com"
    };

    var appUser = AppUser.Create();
    appUser.SetIdentityId(identityUser.Id);

    var identityUserProperty = typeof(AppUser).GetProperty("IdentityUser");
    identityUserProperty?.SetValue(appUser, identityUser);
    // User has no roles assigned

    var users = new List<AppUser> { appUser };
    var paginatedList = new PaginatedList<AppUser>(users, 1, 0, 10);

    _userRepositoryMock
        .Setup(r => r.GetAllUsersByRoleIdWithIdentityAndRolesAsync(roleId, 0, 10, It.IsAny<CancellationToken>()))
        .ReturnsAsync(Result.Success(paginatedList));

    // Act
    var result = await _handler.Handle(query, default);

    // Assert
    Assert.True(result.IsSuccess);
    Assert.NotNull(result.Value);
    Assert.Single(result.Value.Items);
    Assert.Equal("testuser", result.Value.Items[0].UserName);
    Assert.Empty(result.Value.Items[0].Roles);
  }

  [Fact]
  public async Task Handle_PreservesPaginationInfo()
  {
    // Arrange
    var roleId = Guid.NewGuid();
    var pageIndex = 2;
    var pageSize = 5;
    var query = new GetAllUsersByRoleIdQuery(pageIndex, pageSize, roleId);

    var users = new List<AppUser>();
    var paginatedList = new PaginatedList<AppUser>(users, 15, pageIndex, pageSize);

    _userRepositoryMock
        .Setup(r => r.GetAllUsersByRoleIdWithIdentityAndRolesAsync(roleId, pageIndex, pageSize, It.IsAny<CancellationToken>()))
        .ReturnsAsync(Result.Success(paginatedList));

    // Act
    var result = await _handler.Handle(query, default);

    // Assert
    Assert.True(result.IsSuccess);
    Assert.NotNull(result.Value);
    Assert.Equal(15, result.Value.TotalCount);
    Assert.Equal(pageIndex, result.Value.PageIndex);
    Assert.Equal(pageSize, result.Value.PageSize);
  }

  [Fact]
  public async Task Handle_UsesCorrectCancellationToken()
  {
    // Arrange
    var roleId = Guid.NewGuid();
    var query = new GetAllUsersByRoleIdQuery(0, 10, roleId);
    var cancellationTokenSource = new CancellationTokenSource();
    var cancellationToken = cancellationTokenSource.Token;

    var emptyList = new PaginatedList<AppUser>(new List<AppUser>(), 0, 0, 10);

    _userRepositoryMock
        .Setup(r => r.GetAllUsersByRoleIdWithIdentityAndRolesAsync(roleId, 0, 10, cancellationToken))
        .ReturnsAsync(Result.Success(emptyList));

    // Act
    var result = await _handler.Handle(query, cancellationToken);

    // Assert
    Assert.True(result.IsSuccess);
    _userRepositoryMock.Verify(
        r => r.GetAllUsersByRoleIdWithIdentityAndRolesAsync(roleId, 0, 10, cancellationToken),
        Times.Once);
  }

  [Fact]
  public async Task Handle_MapsMultipleRolesPerUser()
  {
    // Arrange
    var roleId = Guid.NewGuid();
    var query = new GetAllUsersByRoleIdQuery(0, 10, roleId);

    var role1 = Role.Create("Admin", "Administrator", Guid.NewGuid());
    var role2 = Role.Create("Manager", "Manager Role", Guid.NewGuid());
    var role3 = Role.Create("User", "User Role", Guid.NewGuid());

    var identityUser = new IdentityUser
    {
      Id = "user-1",
      UserName = "multipleRolesUser",
      Email = "multi@example.com"
    };

    var appUser = AppUser.Create();
    appUser.SetIdentityId(identityUser.Id);
    appUser.AddRole(role1);
    appUser.AddRole(role2);
    appUser.AddRole(role3);

    var identityUserProperty = typeof(AppUser).GetProperty("IdentityUser");
    identityUserProperty?.SetValue(appUser, identityUser);

    var users = new List<AppUser> { appUser };
    var paginatedList = new PaginatedList<AppUser>(users, 1, 0, 10);

    _userRepositoryMock
        .Setup(r => r.GetAllUsersByRoleIdWithIdentityAndRolesAsync(roleId, 0, 10, It.IsAny<CancellationToken>()))
        .ReturnsAsync(Result.Success(paginatedList));

    // Act
    var result = await _handler.Handle(query, default);

    // Assert
    Assert.True(result.IsSuccess);
    Assert.NotNull(result.Value);
    Assert.Single(result.Value.Items);
    Assert.Equal(3, result.Value.Items[0].Roles.Count);
    Assert.Contains(result.Value.Items[0].Roles, r => r.Name == "Admin");
    Assert.Contains(result.Value.Items[0].Roles, r => r.Name == "Manager");
    Assert.Contains(result.Value.Items[0].Roles, r => r.Name == "User");
  }
}