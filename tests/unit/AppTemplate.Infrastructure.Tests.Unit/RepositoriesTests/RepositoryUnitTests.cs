using AppTemplate.Application.Data.DynamicQuery;
using AppTemplate.Application.Services.Clock;
using AppTemplate.Domain;
using AppTemplate.Domain.AppUsers;
using AppTemplate.Domain.Roles;
using AppTemplate.Infrastructure;
using AppTemplate.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace AppTemplate.Infrastructure.Tests.Unit.RepositoriesTests;

[Trait("Category", "Unit")]
public class RepositoryUnitTests : IDisposable
{
  private readonly ApplicationDbContext _context;
  private readonly TestRepository _repository;

  public RepositoryUnitTests()
  {
    var options = new DbContextOptionsBuilder<ApplicationDbContext>()
        .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
        .Options;

    var dateTimeProvider = new Mock<IDateTimeProvider>();
    dateTimeProvider.Setup(x => x.UtcNow).Returns(DateTime.UtcNow);

    _context = new ApplicationDbContext(options, dateTimeProvider.Object);
    _repository = new TestRepository(_context);
  }

  #region GetAsync Tests

  [Fact]
  public async Task GetAsync_WithValidPredicate_ShouldReturnEntity()
  {
    // Arrange
    var user = AppUser.Create();
    user.SetIdentityId("test-id");
    _context.AppUsers.Add(user);
    await _context.SaveChangesAsync();

    // Act
    var result = await _repository.GetAsync(u => u.Id == user.Id);

    // Assert
    Assert.NotNull(result);
    Assert.Equal(user.Id, result.Id);
  }

  [Fact]
  public async Task GetAsync_WithNoMatch_ShouldReturnNull()
  {
    // Act
    var result = await _repository.GetAsync(u => u.Id == Guid.NewGuid());

    // Assert
    Assert.Null(result);
  }

  [Fact]
  public async Task GetAsync_WithIncludeSoftDeleted_ShouldReturnDeletedEntity()
  {
    // Arrange
    var user = AppUser.Create();
    user.SetIdentityId("deleted-user");
    user.MarkDeleted();
    _context.AppUsers.Add(user);
    await _context.SaveChangesAsync();

    // Act
    var result = await _repository.GetAsync(u => u.Id == user.Id, includeSoftDeleted: true);

    // Assert
    Assert.NotNull(result);
    Assert.Equal(user.Id, result.Id);
    Assert.NotNull(result.DeletedOnUtc);
  }

  [Fact]
  public async Task GetAsync_WithoutIncludeSoftDeleted_ShouldNotReturnDeletedEntity()
  {
    // Arrange
    var user = AppUser.Create();
    user.SetIdentityId("deleted-user");
    user.MarkDeleted();
    _context.AppUsers.Add(user);
    await _context.SaveChangesAsync();

    // Act
    var result = await _repository.GetAsync(u => u.Id == user.Id, includeSoftDeleted: false);

    // Assert
    Assert.Null(result);
  }

  [Fact]
  public async Task GetAsync_WithInclude_ShouldLoadRelatedEntities()
  {
    // Arrange
    var user = AppUser.Create();
    user.SetIdentityId("user-with-roles");
    user.AddRole(Role.DefaultRole);
    _context.AppUsers.Add(user);
    await _context.SaveChangesAsync();
    _context.ChangeTracker.Clear();

    // Act
    var result = await _repository.GetAsync(
        u => u.Id == user.Id,
        include: query => query.Include(u => u.Roles));

    // Assert
    Assert.NotNull(result);
    Assert.NotEmpty(result.Roles);
  }

  [Fact]
  public async Task GetAsync_WithAsNoTracking_ShouldNotTrackEntity()
  {
    // Arrange
    var user = AppUser.Create();
    user.SetIdentityId("untracked-user");
    _context.AppUsers.Add(user);
    await _context.SaveChangesAsync();

    // Act
    var result = await _repository.GetAsync(u => u.Id == user.Id, asNoTracking: true);

    // Assert
    Assert.NotNull(result);
    Assert.Equal(EntityState.Detached, _context.Entry(result).State);
  }

  [Fact]
  public async Task GetAsync_WithoutAsNoTracking_ShouldTrackEntity()
  {
    // Arrange
    var user = AppUser.Create();
    user.SetIdentityId("tracked-user");
    _context.AppUsers.Add(user);
    await _context.SaveChangesAsync();
    _context.ChangeTracker.Clear();

    // Act
    var result = await _repository.GetAsync(u => u.Id == user.Id, asNoTracking: false);

    // Assert
    Assert.NotNull(result);
    Assert.NotEqual(EntityState.Detached, _context.Entry(result).State);
  }

  #endregion

  #region GetAllAsync Tests

  [Fact]
  public async Task GetAllAsync_WithoutPredicate_ShouldReturnAllEntities()
  {
    // Arrange
    var user1 = AppUser.Create();
    user1.SetIdentityId("user1");
    var user2 = AppUser.Create();
    user2.SetIdentityId("user2");
    _context.AppUsers.AddRange(user1, user2);
    await _context.SaveChangesAsync();

    // Act
    var result = await _repository.GetAllAsync();

    // Assert
    Assert.NotNull(result);
    Assert.Equal(2, result.TotalCount);
    Assert.Equal(2, result.Items.Count);
  }

  [Fact]
  public async Task GetAllAsync_WithPredicate_ShouldReturnFilteredEntities()
  {
    // Arrange
    var user1 = AppUser.Create();
    user1.SetIdentityId("user1");
    var user2 = AppUser.Create();
    user2.SetIdentityId("user2");
    _context.AppUsers.AddRange(user1, user2);
    await _context.SaveChangesAsync();

    // Act
    var result = await _repository.GetAllAsync(predicate: u => u.IdentityId == "user1");

    // Assert
    Assert.NotNull(result);
    Assert.Equal(1, result.TotalCount);
    Assert.Single(result.Items);
  }

  [Fact]
  public async Task GetAllAsync_WithPagination_ShouldReturnCorrectPage()
  {
    // Arrange
    for (int i = 0; i < 25; i++)
    {
      var user = AppUser.Create();
      user.SetIdentityId($"user{i}");
      _context.AppUsers.Add(user);
    }
    await _context.SaveChangesAsync();

    // Act
    var result = await _repository.GetAllAsync(pageIndex: 1, pageSize: 10);

    // Assert
    Assert.NotNull(result);
    Assert.Equal(25, result.TotalCount);
    Assert.Equal(10, result.Items.Count);
    Assert.Equal(1, result.PageIndex);
    Assert.Equal(10, result.PageSize);
  }

  [Fact]
  public async Task GetAllAsync_WithIncludeSoftDeleted_ShouldIncludeDeletedEntities()
  {
    // Arrange
    var user1 = AppUser.Create();
    user1.SetIdentityId("active-user");
    var user2 = AppUser.Create();
    user2.SetIdentityId("deleted-user");
    user2.MarkDeleted();
    _context.AppUsers.AddRange(user1, user2);
    await _context.SaveChangesAsync();

    // Act
    var result = await _repository.GetAllAsync(includeSoftDeleted: true);

    // Assert
    Assert.NotNull(result);
    Assert.Equal(2, result.TotalCount);
  }

  [Fact]
  public async Task GetAllAsync_WithoutIncludeSoftDeleted_ShouldExcludeDeletedEntities()
  {
    // Arrange
    var user1 = AppUser.Create();
    user1.SetIdentityId("active-user");
    var user2 = AppUser.Create();
    user2.SetIdentityId("deleted-user");
    user2.MarkDeleted();
    _context.AppUsers.AddRange(user1, user2);
    await _context.SaveChangesAsync();

    // Act
    var result = await _repository.GetAllAsync(includeSoftDeleted: false);

    // Assert
    Assert.NotNull(result);
    Assert.Equal(1, result.TotalCount);
  }

  [Fact]
  public async Task GetAllAsync_WithInclude_ShouldLoadRelatedEntities()
  {
    // Arrange
    var user = AppUser.Create();
    user.SetIdentityId("user-with-roles");
    user.AddRole(Role.DefaultRole);
    _context.AppUsers.Add(user);
    await _context.SaveChangesAsync();
    _context.ChangeTracker.Clear();

    // Act
    var result = await _repository.GetAllAsync(
        predicate: u => u.Id == user.Id,
        include: query => query.Include(u => u.Roles));

    // Assert
    Assert.Single(result.Items);
    Assert.NotEmpty(result.Items[0].Roles);
  }

  [Fact]
  public async Task GetAllAsync_WithEmptyDatabase_ShouldReturnEmptyList()
  {
    // Act
    var result = await _repository.GetAllAsync();

    // Assert
    Assert.NotNull(result);
    Assert.Equal(0, result.TotalCount);
    Assert.Empty(result.Items);
  }

  #endregion

  #region GetAllDynamicAsync Tests

  [Fact]
  public async Task GetAllDynamicAsync_WithSimpleFilter_ShouldReturnFilteredEntities()
  {
    // Arrange
    var user1 = AppUser.Create();
    user1.SetIdentityId("user1");
    var user2 = AppUser.Create();
    user2.SetIdentityId("user2");
    _context.AppUsers.AddRange(user1, user2);
    await _context.SaveChangesAsync();

    var dynamicQuery = new DynamicQuery
    {
      Filter = new Filter
      {
        Field = "IdentityId",
        Operator = "eq",
        Value = "user1"
      }
    };

    // Act
    var result = await _repository.GetAllDynamicAsync(dynamicQuery);

    // Assert
    Assert.NotNull(result);
    Assert.Equal(1, result.TotalCount);
    Assert.Single(result.Items);
  }

  [Fact]
  public async Task GetAllDynamicAsync_WithSorting_ShouldReturnSortedEntities()
  {
    // Arrange
    var user1 = AppUser.Create();
    user1.SetIdentityId("user-z");
    var user2 = AppUser.Create();
    user2.SetIdentityId("user-a");
    _context.AppUsers.AddRange(user1, user2);
    await _context.SaveChangesAsync();

    var dynamicQuery = new DynamicQuery
    {
      Sort = new List<Sort>
      {
        new Sort { Field = "IdentityId", Dir = "asc" }
      }
    };

    // Act
    var result = await _repository.GetAllDynamicAsync(dynamicQuery);

    // Assert
    Assert.NotNull(result);
    Assert.Equal(2, result.TotalCount);
    Assert.Equal("user-a", result.Items[0].IdentityId);
  }

  [Fact]
  public async Task GetAllDynamicAsync_WithPagination_ShouldReturnCorrectPage()
  {
    // Arrange
    for (int i = 0; i < 15; i++)
    {
      var user = AppUser.Create();
      user.SetIdentityId($"user{i:D2}");
      _context.AppUsers.Add(user);
    }
    await _context.SaveChangesAsync();

    var dynamicQuery = new DynamicQuery();

    // Act
    var result = await _repository.GetAllDynamicAsync(dynamicQuery, pageIndex: 1, pageSize: 5);

    // Assert
    Assert.NotNull(result);
    Assert.Equal(15, result.TotalCount);
    Assert.Equal(5, result.Items.Count);
  }

  [Fact]
  public async Task GetAllDynamicAsync_WithIncludeSoftDeleted_ShouldIncludeDeletedEntities()
  {
    // Arrange
    var user1 = AppUser.Create();
    user1.SetIdentityId("active-user");
    var user2 = AppUser.Create();
    user2.SetIdentityId("deleted-user");
    user2.MarkDeleted();
    _context.AppUsers.AddRange(user1, user2);
    await _context.SaveChangesAsync();

    var dynamicQuery = new DynamicQuery();

    // Act
    var result = await _repository.GetAllDynamicAsync(dynamicQuery, includeSoftDeleted: true);

    // Assert
    Assert.NotNull(result);
    Assert.Equal(2, result.TotalCount);
  }

  [Fact]
  public async Task GetAllDynamicAsync_WithoutIncludeSoftDeleted_ShouldExcludeDeletedEntities()
  {
    // Arrange
    var user1 = AppUser.Create();
    user1.SetIdentityId("active-user");
    var user2 = AppUser.Create();
    user2.SetIdentityId("deleted-user");
    user2.MarkDeleted();
    _context.AppUsers.AddRange(user1, user2);
    await _context.SaveChangesAsync();

    var dynamicQuery = new DynamicQuery();

    // Act
    var result = await _repository.GetAllDynamicAsync(dynamicQuery, includeSoftDeleted: false);

    // Assert
    Assert.NotNull(result);
    Assert.Equal(1, result.TotalCount);
  }

  [Fact]
  public async Task GetAllDynamicAsync_WithInclude_ShouldLoadRelatedEntities()
  {
    // Arrange
    var user = AppUser.Create();
    user.SetIdentityId("user-with-roles");
    user.AddRole(Role.DefaultRole);
    _context.AppUsers.Add(user);
    await _context.SaveChangesAsync();
    _context.ChangeTracker.Clear();

    var dynamicQuery = new DynamicQuery
    {
      Filter = new Filter
      {
        Field = "IdentityId",
        Operator = "eq",
        Value = "user-with-roles"
      }
    };

    // Act
    var result = await _repository.GetAllDynamicAsync(
        dynamicQuery,
        include: query => query.Include(u => u.Roles));

    // Assert
    Assert.NotEmpty(result.Items[0].Roles);
  }

  [Fact]
  public async Task GetAllDynamicAsync_WithAsNoTracking_ShouldNotTrackEntities()
  {
    // Arrange
    var user = AppUser.Create();
    user.SetIdentityId("untracked-user");
    _context.AppUsers.Add(user);
    await _context.SaveChangesAsync();

    var dynamicQuery = new DynamicQuery();

    // Act
    var result = await _repository.GetAllDynamicAsync(dynamicQuery, asNoTracking: true);

    // Assert
    Assert.NotNull(result);
    Assert.Single(result.Items);
    Assert.Equal(EntityState.Detached, _context.Entry(result.Items[0]).State);
  }

  [Fact]
  public async Task GetAllDynamicAsync_WithComplexFilter_ShouldReturnFilteredEntities()
  {
    // Arrange
    for (int i = 0; i < 10; i++)
    {
      var user = AppUser.Create();
      user.SetIdentityId($"user{i}");
      _context.AppUsers.Add(user);
    }
    await _context.SaveChangesAsync();

    var dynamicQuery = new DynamicQuery
    {
      Filter = new Filter
      {
        Field = "IdentityId",
        Operator = "contains",
        Value = "user"
      }
    };

    // Act
    var result = await _repository.GetAllDynamicAsync(dynamicQuery);

    // Assert
    Assert.NotNull(result);
    Assert.Equal(10, result.TotalCount);
  }

  #endregion

  #region ExistsAsync Tests

  [Fact]
  public async Task ExistsAsync_WithMatchingEntity_ShouldReturnTrue()
  {
    // Arrange
    var user = AppUser.Create();
    user.SetIdentityId("existing-user");
    _context.AppUsers.Add(user);
    await _context.SaveChangesAsync();

    // Act
    var result = await _repository.ExistsAsync(u => u.IdentityId == "existing-user");

    // Assert
    Assert.True(result);
  }

  [Fact]
  public async Task ExistsAsync_WithNoMatch_ShouldReturnFalse()
  {
    // Act
    var result = await _repository.ExistsAsync(u => u.IdentityId == "non-existent");

    // Assert
    Assert.False(result);
  }

  [Fact]
  public async Task ExistsAsync_WithDeletedEntity_ShouldReturnFalseByDefault()
  {
    // Arrange
    var user = AppUser.Create();
    user.SetIdentityId("deleted-user");
    user.MarkDeleted();
    _context.AppUsers.Add(user);
    await _context.SaveChangesAsync();

    // Act
    var result = await _repository.ExistsAsync(u => u.IdentityId == "deleted-user");

    // Assert
    Assert.False(result);
  }

  [Fact]
  public async Task ExistsAsync_WithDeletedEntityAndIncludeSoftDeleted_ShouldReturnTrue()
  {
    // Arrange
    var user = AppUser.Create();
    user.SetIdentityId("deleted-user");
    user.MarkDeleted();
    _context.AppUsers.Add(user);
    await _context.SaveChangesAsync();

    // Act
    var result = await _repository.ExistsAsync(u => u.IdentityId == "deleted-user", includeSoftDeleted: true);

    // Assert
    Assert.True(result);
  }

  #endregion

  #region AddAsync Tests

  [Fact]
  public async Task AddAsync_WithValidEntity_ShouldAddToContext()
  {
    // Arrange
    var user = AppUser.Create();
    user.SetIdentityId("new-user");

    // Act
    await _repository.AddAsync(user);
    await _context.SaveChangesAsync();

    // Assert
    var savedUser = await _context.AppUsers.FirstOrDefaultAsync(u => u.Id == user.Id);
    Assert.NotNull(savedUser);
    Assert.Equal("new-user", savedUser.IdentityId);
  }

  [Fact]
  public async Task AddAsync_MultipleEntities_ShouldAddAllToContext()
  {
    // Arrange
    var user1 = AppUser.Create();
    user1.SetIdentityId("user1");
    var user2 = AppUser.Create();
    user2.SetIdentityId("user2");

    // Act
    await _repository.AddAsync(user1);
    await _repository.AddAsync(user2);
    await _context.SaveChangesAsync();

    // Assert
    var count = await _context.AppUsers.CountAsync();
    Assert.Equal(2, count);
  }

  #endregion

  #region Update Tests

  [Fact]
  public async Task Update_WithExistingEntity_ShouldUpdateEntity()
  {
    // Arrange
    var user = AppUser.Create();
    user.SetIdentityId("original-id");
    _context.AppUsers.Add(user);
    await _context.SaveChangesAsync();
    _context.ChangeTracker.Clear();

    // Act
    var userToUpdate = await _context.AppUsers.FindAsync(user.Id);
    userToUpdate!.SetIdentityId("updated-id");
    _repository.Update(userToUpdate);
    await _context.SaveChangesAsync();

    // Assert
    var updatedUser = await _context.AppUsers.FindAsync(user.Id);
    Assert.NotNull(updatedUser);
    Assert.Equal("updated-id", updatedUser.IdentityId);
  }

  #endregion

  #region Delete Tests

  [Fact]
  public async Task Delete_WithSoftDelete_ShouldMarkAsDeleted()
  {
    // Arrange
    var user = AppUser.Create();
    user.SetIdentityId("user-to-soft-delete");
    _context.AppUsers.Add(user);
    await _context.SaveChangesAsync();
    _context.ChangeTracker.Clear();

    // Act
    var userToDelete = await _context.AppUsers.FindAsync(user.Id);
    _repository.Delete(userToDelete!, isSoftDelete: true);
    await _context.SaveChangesAsync();

    // Assert
    var deletedUser = await _context.AppUsers.IgnoreQueryFilters().FirstOrDefaultAsync(u => u.Id == user.Id);
    Assert.NotNull(deletedUser);
    Assert.NotNull(deletedUser.DeletedOnUtc);
  }

  [Fact]
  public async Task Delete_WithHardDelete_ShouldRemoveFromDatabase()
  {
    // Arrange
    var user = AppUser.Create();
    user.SetIdentityId("user-to-hard-delete");
    _context.AppUsers.Add(user);
    await _context.SaveChangesAsync();
    _context.ChangeTracker.Clear();

    // Act
    var userToDelete = await _context.AppUsers.FindAsync(user.Id);
    _repository.Delete(userToDelete!, isSoftDelete: false);
    await _context.SaveChangesAsync();

    // Assert
    var deletedUser = await _context.AppUsers.IgnoreQueryFilters().FirstOrDefaultAsync(u => u.Id == user.Id);
    Assert.Null(deletedUser);
  }

  [Fact]
  public async Task Delete_WithSoftDeleteOnEntityWithoutSoftDeleteProperty_ShouldHardDelete()
  {
    // Arrange - Using a test entity without soft delete
    var roleRepo = new TestRoleRepository(_context);
    var role = Role.Create("TestRole", "Test Role", Guid.NewGuid());
    _context.Roles.Add(role);
    await _context.SaveChangesAsync();
    _context.ChangeTracker.Clear();

    // Act
    var roleToDelete = await _context.Roles.FindAsync(role.Id);
    roleRepo.Delete(roleToDelete!, isSoftDelete: true);
    await _context.SaveChangesAsync();

    // Assert - Should be soft deleted since Role has DeletedOnUtc property
    var deletedRole = await _context.Roles.IgnoreQueryFilters().FirstOrDefaultAsync(r => r.Id == role.Id);
    Assert.NotNull(deletedRole);
    Assert.NotNull(deletedRole.DeletedOnUtc);
  }

  #endregion

  #region CountAsync Tests

  [Fact]
  public async Task CountAsync_WithoutPredicate_ShouldReturnTotalCount()
  {
    // Arrange
    for (int i = 0; i < 5; i++)
    {
      var user = AppUser.Create();
      user.SetIdentityId($"user{i}");
      _context.AppUsers.Add(user);
    }
    await _context.SaveChangesAsync();

    // Act
    var count = await _repository.CountAsync();

    // Assert
    Assert.Equal(5, count);
  }

  [Fact]
  public async Task CountAsync_WithPredicate_ShouldReturnFilteredCount()
  {
    // Arrange
    for (int i = 0; i < 5; i++)
    {
      var user = AppUser.Create();
      user.SetIdentityId($"user{i}");
      _context.AppUsers.Add(user);
    }
    await _context.SaveChangesAsync();

    // Act
    var count = await _repository.CountAsync(u => u.IdentityId.Contains("1"));

    // Assert
    Assert.Equal(1, count);
  }

  [Fact]
  public async Task CountAsync_WithIncludeSoftDeleted_ShouldIncludeDeletedEntities()
  {
    // Arrange
    var user1 = AppUser.Create();
    user1.SetIdentityId("active-user");
    var user2 = AppUser.Create();
    user2.SetIdentityId("deleted-user");
    user2.MarkDeleted();
    _context.AppUsers.AddRange(user1, user2);
    await _context.SaveChangesAsync();

    // Act
    var count = await _repository.CountAsync(includeSoftDeleted: true);

    // Assert
    Assert.Equal(2, count);
  }

  [Fact]
  public async Task CountAsync_WithoutIncludeSoftDeleted_ShouldExcludeDeletedEntities()
  {
    // Arrange
    var user1 = AppUser.Create();
    user1.SetIdentityId("active-user");
    var user2 = AppUser.Create();
    user2.SetIdentityId("deleted-user");
    user2.MarkDeleted();
    _context.AppUsers.AddRange(user1, user2);
    await _context.SaveChangesAsync();

    // Act
    var count = await _repository.CountAsync(includeSoftDeleted: false);

    // Assert
    Assert.Equal(1, count);
  }

  [Fact]
  public async Task CountAsync_WithEmptyDatabase_ShouldReturnZero()
  {
    // Act
    var count = await _repository.CountAsync();

    // Assert
    Assert.Equal(0, count);
  }

  [Fact]
  public async Task CountAsync_WithPredicateAndIncludeSoftDeleted_ShouldReturnCorrectCount()
  {
    // Arrange
    var user1 = AppUser.Create();
    user1.SetIdentityId("user1");
    var user2 = AppUser.Create();
    user2.SetIdentityId("user2");
    user2.MarkDeleted();
    var user3 = AppUser.Create();
    user3.SetIdentityId("user3");
    _context.AppUsers.AddRange(user1, user2, user3);
    await _context.SaveChangesAsync();

    // Act
    var count = await _repository.CountAsync(
        u => u.IdentityId.StartsWith("user"),
        includeSoftDeleted: true);

    // Assert
    Assert.Equal(3, count);
  }

  #endregion

  #region Soft Delete Tests

  [Fact]
  public void IsSoftDeleted_ShouldReturnTrue_IfDeletedOnUtcHasValue()
  {
    // Arrange
    var entity = new SoftDeleteEntity { DeletedOnUtc = DateTime.UtcNow };

    // Act
    var isSoftDeleted = entity.DeletedOnUtc != null;

    // Assert
    Assert.True(isSoftDeleted);
  }

  [Fact]
  public void IsSoftDeleted_ShouldReturnFalse_IfDeletedOnUtcIsNull()
  {
    // Arrange
    var entity = new SoftDeleteEntity { DeletedOnUtc = null };

    // Act
    var isSoftDeleted = entity.DeletedOnUtc != null;

    // Assert
    Assert.False(isSoftDeleted);
  }

  #endregion

  public void Dispose()
  {
    _context?.Dispose();
  }
}

// Test repository implementation
public class TestRepository : Repository<AppUser, Guid>
{
  public TestRepository(ApplicationDbContext dbContext) : base(dbContext) { }
}

// Test role repository implementation
public class TestRoleRepository : Repository<Role, Guid>
{
  public TestRoleRepository(ApplicationDbContext dbContext) : base(dbContext) { }
}

// Test entity for soft delete tests
public class SoftDeleteEntity
{
  public DateTime? DeletedOnUtc { get; set; }
}