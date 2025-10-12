using AppTemplate.Application.Features.AppUsers.Queries.GetUser;
using AppTemplate.Application.Repositories;
using AppTemplate.Domain.AppUsers;
using AppTemplate.Domain.Roles;
using Ardalis.Result;
using Microsoft.AspNetCore.Identity;
using Moq;

namespace AppTemplate.Application.Tests.Unit.Features.AppUsersTests.Queries.GetUserTests;

[Trait("Category", "Unit")]
public class GetUserQueryHandlerUnitTests
{
    private readonly Mock<IAppUsersRepository> _userRepositoryMock;
    private readonly GetUserQueryHandler _handler;

    public GetUserQueryHandlerUnitTests()
    {
        _userRepositoryMock = new Mock<IAppUsersRepository>();
        _handler = new GetUserQueryHandler(_userRepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_ReturnsNotFound_WhenUserNotFound()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _userRepositoryMock
            .Setup(r => r.GetUserByIdWithIdentityAndRrolesAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<AppUser>.NotFound("User not found"));

        var query = new GetUserQuery(userId);

        // Act
        var result = await _handler.Handle(query, default);

        // Assert
        Assert.Equal(ResultStatus.NotFound, result.Status);
        Assert.Null(result.Value);
    }

    [Fact]
    public async Task Handle_ReturnsNotFound_WhenUserIsNull()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _userRepositoryMock
            .Setup(r => r.GetUserByIdWithIdentityAndRrolesAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success<AppUser>(null));

        var query = new GetUserQuery(userId);

        // Act
        var result = await _handler.Handle(query, default);

        // Assert
        Assert.Equal(ResultStatus.NotFound, result.Status);
        Assert.Null(result.Value);
    }

    [Fact]
    public async Task Handle_ReturnsSuccess_WithUserData_WhenUserFound()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var identityUser = new IdentityUser
        {
            Id = "identity-123",
            UserName = "testuser",
            Email = "test@example.com"
        };

        var role1 = Role.Create("Admin", "Administrator", Guid.NewGuid());
        var role2 = Role.Create("User", "User", Guid.NewGuid());

        var appUser = AppUser.Create();
        appUser.SetIdentityId(identityUser.Id);
        appUser.AddRole(role1);
        appUser.AddRole(role2);

        // Set IdentityUser using reflection
        var identityUserProperty = typeof(AppUser).GetProperty("IdentityUser");
        identityUserProperty?.SetValue(appUser, identityUser);

        _userRepositoryMock
            .Setup(r => r.GetUserByIdWithIdentityAndRrolesAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(appUser));

        var query = new GetUserQuery(userId);

        // Act
        var result = await _handler.Handle(query, default);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(appUser.Id, result.Value.Id);
        Assert.Equal(identityUser.UserName, result.Value.UserName);
        Assert.Equal(2, result.Value.Roles.Count);
        Assert.Contains(result.Value.Roles, r => r.Name == "Admin");
        Assert.Contains(result.Value.Roles, r => r.Name == "User");
    }

    [Fact]
    public async Task Handle_ReturnsSuccess_WithEmptyRoles_WhenUserHasNoRoles()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var identityUser = new IdentityUser
        {
            Id = "identity-123",
            UserName = "testuser",
            Email = "test@example.com"
        };

        var appUser = AppUser.Create();
        appUser.SetIdentityId(identityUser.Id);

        var identityUserProperty = typeof(AppUser).GetProperty("IdentityUser");
        identityUserProperty?.SetValue(appUser, identityUser);

        _userRepositoryMock
            .Setup(r => r.GetUserByIdWithIdentityAndRrolesAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(appUser));

        var query = new GetUserQuery(userId);

        // Act
        var result = await _handler.Handle(query, default);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(appUser.Id, result.Value.Id);
        Assert.Equal(identityUser.UserName, result.Value.UserName);
        Assert.Empty(result.Value.Roles);
    }

    [Fact]
    public async Task Handle_MapsRoleProperties_Correctly()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var identityUser = new IdentityUser
        {
            Id = "identity-123",
            UserName = "testuser",
            Email = "test@example.com"
        };

        var role = Role.Create("Admin", "Administrator", Guid.NewGuid(), isDefault: true);

        var appUser = AppUser.Create();
        appUser.SetIdentityId(identityUser.Id);
        appUser.AddRole(role);

        var identityUserProperty = typeof(AppUser).GetProperty("IdentityUser");
        identityUserProperty?.SetValue(appUser, identityUser);

        _userRepositoryMock
            .Setup(r => r.GetUserByIdWithIdentityAndRrolesAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(appUser));

        var query = new GetUserQuery(userId);

        // Act
        var result = await _handler.Handle(query, default);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Single(result.Value.Roles);

        var mappedRole = result.Value.Roles.First();
        Assert.Equal(role.Id, mappedRole.Id);
        Assert.Equal("Admin", mappedRole.Name);
        Assert.Equal("Administrator", mappedRole.DisplayName);
        Assert.True(mappedRole.IsDefault);
    }

    [Fact]
    public async Task Handle_UsesCancellationToken()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var cancellationTokenSource = new CancellationTokenSource();
        var cancellationToken = cancellationTokenSource.Token;

        _userRepositoryMock
            .Setup(r => r.GetUserByIdWithIdentityAndRrolesAsync(userId, cancellationToken))
            .ReturnsAsync(Result<AppUser>.NotFound("User not found"));

        var query = new GetUserQuery(userId);

        // Act
        var result = await _handler.Handle(query, cancellationToken);

        // Assert
        Assert.False(result.IsSuccess);
        _userRepositoryMock.Verify(
            r => r.GetUserByIdWithIdentityAndRrolesAsync(userId, cancellationToken),
            Times.Once);
    }

    [Fact]
    public async Task Handle_MapsMultipleRoles_WithDifferentProperties()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var identityUser = new IdentityUser
        {
            Id = "identity-123",
            UserName = "multiRoleUser",
            Email = "multi@example.com"
        };

        var defaultRole = Role.Create("User", "Standard User", Guid.NewGuid(), isDefault: true);
        var adminRole = Role.Create("Admin", "Administrator", Guid.NewGuid(), isDefault: false);
        var managerRole = Role.Create("Manager", "Manager Role", Guid.NewGuid(), isDefault: false);

        var appUser = AppUser.Create();
        appUser.SetIdentityId(identityUser.Id);
        appUser.AddRole(defaultRole);
        appUser.AddRole(adminRole);
        appUser.AddRole(managerRole);

        var identityUserProperty = typeof(AppUser).GetProperty("IdentityUser");
        identityUserProperty?.SetValue(appUser, identityUser);

        _userRepositoryMock
            .Setup(r => r.GetUserByIdWithIdentityAndRrolesAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(appUser));

        var query = new GetUserQuery(userId);

        // Act
        var result = await _handler.Handle(query, default);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(3, result.Value.Roles.Count);

        var defaultRoleResult = result.Value.Roles.First(r => r.Name == "User");
        Assert.True(defaultRoleResult.IsDefault);

        var adminRoleResult = result.Value.Roles.First(r => r.Name == "Admin");
        Assert.False(adminRoleResult.IsDefault);

        var managerRoleResult = result.Value.Roles.First(r => r.Name == "Manager");
        Assert.False(managerRoleResult.IsDefault);
    }
}
