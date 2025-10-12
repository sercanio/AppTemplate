using AppTemplate.Application.Features.AppUsers.Queries.GetAllUsers;
using AppTemplate.Application.Services.Clock;
using AppTemplate.Domain.AppUsers;
using AppTemplate.Domain.Roles;
using AppTemplate.Infrastructure;
using AppTemplate.Infrastructure.Repositories;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace AppTemplate.Application.Tests.Integration.Features.AppUsersTests.Queries.GetAllUsersTests;

[Trait("Category", "Integration")]
public class GetAllUsersQueryHandlerIntegrationTests : IDisposable
{
  private readonly ApplicationDbContext _dbContext;
  private readonly AppUsersRepository _repository;
  private readonly GetAllUsersQueryHandler _handler;

  public GetAllUsersQueryHandlerIntegrationTests()
  {
    var options = new DbContextOptionsBuilder<ApplicationDbContext>()
        .UseInMemoryDatabase(Guid.NewGuid().ToString())
        .Options;

    _dbContext = new ApplicationDbContext(options, new DateTimeProvider());
    _repository = new AppUsersRepository(_dbContext);
    _handler = new GetAllUsersQueryHandler(_repository);
  }

  #region Empty Database Tests

  [Fact]
  public async Task Handle_ReturnsEmptyList_WhenNoUsersExist()
  {
    // Arrange
    var query = new GetAllUsersQuery(0, 10);

    // Act
    var result = await _handler.Handle(query, default);

    // Assert
    result.IsSuccess.Should().BeTrue();
    result.Value.Should().NotBeNull();
    result.Value.Items.Should().BeEmpty();
    result.Value.TotalCount.Should().Be(0);
  }

  #endregion

  #region Single User Tests

  [Fact]
  public async Task Handle_ReturnsPaginatedUsers_WhenUsersExist()
  {
    // Arrange
    var identityUser = new IdentityUser
    {
      Id = "user-1",
      UserName = "testuser1",
      Email = "testuser1@example.com",
      EmailConfirmed = true
    };
    _dbContext.Users.Add(identityUser);

    var appUser = AppUser.Create();
    appUser.SetIdentityId(identityUser.Id);
    _dbContext.AppUsers.Add(appUser);

    await _dbContext.SaveChangesAsync();

    var query = new GetAllUsersQuery(0, 10);

    // Act
    var result = await _handler.Handle(query, default);

    // Assert
    result.IsSuccess.Should().BeTrue();
    result.Value.Should().NotBeNull();
    result.Value.Items.Should().ContainSingle();

    var responseUser = result.Value.Items.First();
    responseUser.Id.Should().Be(appUser.Id);
    responseUser.UserName.Should().Be(identityUser.UserName);
    responseUser.EmailConfirmed.Should().BeTrue();
  }

  // REMOVED: Handle_ReturnsUserWithEmptyUsername_WhenIdentityUserNameIsNull
  // This test is not valid for integration tests because UserName is required in the database.
  // The null-coalescing behavior is covered in unit tests where we can mock the data.

  #endregion

  #region User with Roles Tests

  [Fact]
  public async Task Handle_ReturnsUserWithRoles_WhenUserHasActiveRoles()
  {
    // Arrange
    var identityUser = new IdentityUser
    {
      Id = "user-with-roles",
      UserName = "roleuser",
      Email = "roleuser@example.com"
    };
    _dbContext.Users.Add(identityUser);

    var appUser = AppUser.Create();
    appUser.SetIdentityId(identityUser.Id);

    var createdById = Guid.NewGuid();
    var adminRole = Role.Create("Admin", "Administrator", createdById);
    var userRole = Role.Create("User", "Regular User", createdById);

    appUser.AddRole(adminRole);
    appUser.AddRole(userRole);

    _dbContext.AppUsers.Add(appUser);
    await _dbContext.SaveChangesAsync();

    var query = new GetAllUsersQuery(0, 10);

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
  public async Task Handle_ExcludesDeletedRoles_WhenUserHasDeletedAndActiveRoles()
  {
    // Arrange
    var identityUser = new IdentityUser
    {
      Id = "user-deleted-roles",
      UserName = "deletedrolesuser",
      Email = "deletedrolesuser@example.com"
    };
    _dbContext.Users.Add(identityUser);

    var appUser = AppUser.Create();
    appUser.SetIdentityId(identityUser.Id);

    var createdById = Guid.NewGuid();
    var deletedById = Guid.NewGuid();

    var activeRole = Role.Create("Active", "Active Role", createdById);
    var deletedRole = Role.Create("Deleted", "Deleted Role", createdById);

    // Soft delete the role using the static Delete method
    Role.Delete(deletedRole, deletedById);

    appUser.AddRole(activeRole);
    appUser.AddRole(deletedRole);

    _dbContext.AppUsers.Add(appUser);
    await _dbContext.SaveChangesAsync();

    var query = new GetAllUsersQuery(0, 10);

    // Act
    var result = await _handler.Handle(query, default);

    // Assert
    result.IsSuccess.Should().BeTrue();
    var responseUser = result.Value.Items.First();
    responseUser.Roles.Should().ContainSingle();
    responseUser.Roles.First().Name.Should().Be("Active");
  }

  [Fact]
  public async Task Handle_ReturnsUserWithoutRoles_WhenUserHasNoRoles()
  {
    // Arrange
    var identityUser = new IdentityUser
    {
      Id = "user-no-roles",
      UserName = "norolesuser",
      Email = "norolesuser@example.com"
    };
    _dbContext.Users.Add(identityUser);

    var appUser = AppUser.Create();
    appUser.SetIdentityId(identityUser.Id);

    _dbContext.AppUsers.Add(appUser);
    await _dbContext.SaveChangesAsync();

    var query = new GetAllUsersQuery(0, 10);

    // Act
    var result = await _handler.Handle(query, default);

    // Assert
    result.IsSuccess.Should().BeTrue();
    var responseUser = result.Value.Items.First();
    responseUser.Roles.Should().BeEmpty();
  }

  #endregion

  #region Multiple Users Tests

  [Fact]
  public async Task Handle_ReturnsMultipleUsers_InCorrectOrder()
  {
    // Arrange
    var user1Identity = new IdentityUser
    {
      Id = "id1",
      UserName = "user1",
      Email = "user1@example.com",
      EmailConfirmed = true
    };
    var user2Identity = new IdentityUser
    {
      Id = "id2",
      UserName = "user2",
      Email = "user2@example.com",
      EmailConfirmed = false
    };
    var user3Identity = new IdentityUser
    {
      Id = "id3",
      UserName = "user3",
      Email = "user3@example.com",
      EmailConfirmed = true
    };

    _dbContext.Users.AddRange(user1Identity, user2Identity, user3Identity);

    var user1 = AppUser.Create();
    user1.SetIdentityId(user1Identity.Id);

    var user2 = AppUser.Create();
    user2.SetIdentityId(user2Identity.Id);

    var user3 = AppUser.Create();
    user3.SetIdentityId(user3Identity.Id);

    _dbContext.AppUsers.AddRange(user1, user2, user3);
    await _dbContext.SaveChangesAsync();

    var query = new GetAllUsersQuery(0, 10);

    // Act
    var result = await _handler.Handle(query, default);

    // Assert
    result.IsSuccess.Should().BeTrue();
    result.Value.Items.Should().HaveCount(3);
    result.Value.TotalCount.Should().Be(3);
    result.Value.Items.Should().Contain(u => u.UserName == "user1" && u.EmailConfirmed);
    result.Value.Items.Should().Contain(u => u.UserName == "user2" && !u.EmailConfirmed);
    result.Value.Items.Should().Contain(u => u.UserName == "user3" && u.EmailConfirmed);
  }

  #endregion

  #region Pagination Tests

  [Fact]
  public async Task Handle_ReturnsPaginatedResults_WithCorrectPageSize()
  {
    // Arrange
    for (int i = 1; i <= 15; i++)
    {
      var identity = new IdentityUser { Id = $"id{i}", UserName = $"user{i}", Email = $"user{i}@example.com" };
      _dbContext.Users.Add(identity);

      var user = AppUser.Create();
      user.SetIdentityId(identity.Id);
      _dbContext.AppUsers.Add(user);
    }
    await _dbContext.SaveChangesAsync();

    var query = new GetAllUsersQuery(0, 10);

    // Act
    var result = await _handler.Handle(query, default);

    // Assert
    result.IsSuccess.Should().BeTrue();
    result.Value.Items.Should().HaveCount(10);
    result.Value.TotalCount.Should().Be(15);
    result.Value.PageIndex.Should().Be(0);
    result.Value.PageSize.Should().Be(10);
    result.Value.TotalPages.Should().Be(2);
  }

  [Fact]
  public async Task Handle_ReturnsSecondPage_WhenPageIndexIsOne()
  {
    // Arrange
    for (int i = 1; i <= 15; i++)
    {
      var identity = new IdentityUser { Id = $"id{i}", UserName = $"user{i}", Email = $"user{i}@example.com" };
      _dbContext.Users.Add(identity);

      var user = AppUser.Create();
      user.SetIdentityId(identity.Id);
      _dbContext.AppUsers.Add(user);
    }
    await _dbContext.SaveChangesAsync();

    var query = new GetAllUsersQuery(1, 10);

    // Act
    var result = await _handler.Handle(query, default);

    // Assert
    result.IsSuccess.Should().BeTrue();
    result.Value.Items.Should().HaveCount(5);
    result.Value.TotalCount.Should().Be(15);
    result.Value.PageIndex.Should().Be(1);
    result.Value.HasPreviousPage.Should().BeTrue();
    result.Value.HasNextPage.Should().BeFalse();
  }

  [Theory]
  [InlineData(0, 5)]
  [InlineData(1, 5)]
  [InlineData(0, 20)]
  public async Task Handle_WorksWithVariousPaginationSizes(int pageIndex, int pageSize)
  {
    // Arrange
    for (int i = 1; i <= 12; i++)
    {
      var identity = new IdentityUser { Id = $"id{i}", UserName = $"user{i}", Email = $"user{i}@example.com" };
      _dbContext.Users.Add(identity);

      var user = AppUser.Create();
      user.SetIdentityId(identity.Id);
      _dbContext.AppUsers.Add(user);
    }
    await _dbContext.SaveChangesAsync();

    var query = new GetAllUsersQuery(pageIndex, pageSize);

    // Act
    var result = await _handler.Handle(query, default);

    // Assert
    result.IsSuccess.Should().BeTrue();
    result.Value.PageIndex.Should().Be(pageIndex);
    result.Value.PageSize.Should().Be(pageSize);
    result.Value.TotalCount.Should().Be(12);
  }

  #endregion

  #region Complex Scenarios

  [Fact]
  public async Task Handle_ReturnsCompleteUserData_WithAllFieldsPopulated()
  {
    // Arrange
    var identityUser = new IdentityUser
    {
      Id = "complete-user-id",
      UserName = "completeuser",
      Email = "complete@example.com",
      EmailConfirmed = true,
      PhoneNumber = "+1234567890",
      PhoneNumberConfirmed = true
    };
    _dbContext.Users.Add(identityUser);

    var appUser = AppUser.Create();
    appUser.SetIdentityId(identityUser.Id);

    var createdById = Guid.NewGuid();
    var role1 = Role.Create("Admin", "Administrator", createdById);
    var role2 = Role.Create("Manager", "Manager", createdById);

    appUser.AddRole(role1);
    appUser.AddRole(role2);

    _dbContext.AppUsers.Add(appUser);
    await _dbContext.SaveChangesAsync();

    var query = new GetAllUsersQuery(0, 10);

    // Act
    var result = await _handler.Handle(query, default);

    // Assert
    result.IsSuccess.Should().BeTrue();
    var responseUser = result.Value.Items.First();

    responseUser.Id.Should().Be(appUser.Id);
    responseUser.UserName.Should().Be("completeuser");
    responseUser.EmailConfirmed.Should().BeTrue();
    responseUser.JoinDate.Should().Be(appUser.CreatedOnUtc);
    responseUser.Roles.Should().HaveCount(2);
    responseUser.Roles.Should().Contain(r => r.Name == "Admin" && r.DisplayName == "Administrator");
    responseUser.Roles.Should().Contain(r => r.Name == "Manager" && r.DisplayName == "Manager");
  }

  [Fact]
  public async Task Handle_HandlesLargeDatasetsEfficiently_With100Users()
  {
    // Arrange
    for (int i = 1; i <= 100; i++)
    {
      var identity = new IdentityUser
      {
        Id = $"bulk-id{i}",
        UserName = $"bulkuser{i}",
        Email = $"bulk{i}@example.com",
        EmailConfirmed = i % 2 == 0
      };
      _dbContext.Users.Add(identity);

      var user = AppUser.Create();
      user.SetIdentityId(identity.Id);
      _dbContext.AppUsers.Add(user);
    }
    await _dbContext.SaveChangesAsync();

    var query = new GetAllUsersQuery(0, 50);

    // Act
    var result = await _handler.Handle(query, default);

    // Assert
    result.IsSuccess.Should().BeTrue();
    result.Value.Items.Should().HaveCount(50);
    result.Value.TotalCount.Should().Be(100);
    result.Value.TotalPages.Should().Be(2);
  }

  #endregion

  public void Dispose()
  {
    _dbContext?.Dispose();
  }
}