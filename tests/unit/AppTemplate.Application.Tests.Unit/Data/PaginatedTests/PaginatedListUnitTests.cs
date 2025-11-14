using AppTemplate.Application.Data.Pagination;
using FluentAssertions;
using MockQueryable;

namespace AppTemplate.Application.Tests.Unit.Data.PaginatedTests;

public class PaginatedListUnitTests
{
  [Fact]
  public void Constructor_WithValidParameters_ShouldCreateInstance()
  {
    // Arrange
    var items = new List<TestEntity>
        {
            new TestEntity { Id = 1, Name = "Item1" },
            new TestEntity { Id = 2, Name = "Item2" },
            new TestEntity { Id = 3, Name = "Item3" }
        };
    int count = 10;
    int pageIndex = 0;
    int pageSize = 3;

    // Act
    var paginatedList = new PaginatedList<TestEntity>(items, count, pageIndex, pageSize);

    // Assert
    paginatedList.Should().NotBeNull();
    paginatedList.Items.Should().HaveCount(3);
    paginatedList.Items.Should().BeEquivalentTo(items);
    paginatedList.TotalCount.Should().Be(count);
    paginatedList.PageIndex.Should().Be(pageIndex);
    paginatedList.PageSize.Should().Be(pageSize);
    paginatedList.TotalPages.Should().Be(4); // Ceiling(10 / 3) = 4
  }

  [Fact]
  public void Constructor_WithEmptyItems_ShouldCreateEmptyList()
  {
    // Arrange
    var items = new List<TestEntity>();
    int count = 0;
    int pageIndex = 0;
    int pageSize = 10;

    // Act
    var paginatedList = new PaginatedList<TestEntity>(items, count, pageIndex, pageSize);

    // Assert
    paginatedList.Items.Should().BeEmpty();
    paginatedList.TotalCount.Should().Be(0);
    paginatedList.TotalPages.Should().Be(0);
  }

  [Fact]
  public void TotalPages_WithExactDivision_ShouldCalculateCorrectly()
  {
    // Arrange
    var items = new List<TestEntity>
        {
            new TestEntity { Id = 1, Name = "Item1" },
            new TestEntity { Id = 2, Name = "Item2" }
        };
    int count = 20;
    int pageIndex = 0;
    int pageSize = 10;

    // Act
    var paginatedList = new PaginatedList<TestEntity>(items, count, pageIndex, pageSize);

    // Assert
    paginatedList.TotalPages.Should().Be(2); // 20 / 10 = 2
  }

  [Fact]
  public void TotalPages_WithPartialPage_ShouldRoundUp()
  {
    // Arrange
    var items = new List<TestEntity> { new TestEntity { Id = 1, Name = "Item1" } };
    int count = 25;
    int pageIndex = 0;
    int pageSize = 10;

    // Act
    var paginatedList = new PaginatedList<TestEntity>(items, count, pageIndex, pageSize);

    // Assert
    paginatedList.TotalPages.Should().Be(3); // Ceiling(25 / 10) = 3
  }

  [Fact]
  public void HasPreviousPage_WhenOnFirstPage_ShouldReturnFalse()
  {
    // Arrange
    var items = new List<TestEntity> { new TestEntity { Id = 1, Name = "Item1" } };
    int pageIndex = 0;

    // Act
    var paginatedList = new PaginatedList<TestEntity>(items, 10, pageIndex, 5);

    // Assert
    paginatedList.HasPreviousPage.Should().BeFalse();
  }

  [Fact]
  public void HasPreviousPage_WhenNotOnFirstPage_ShouldReturnTrue()
  {
    // Arrange
    var items = new List<TestEntity> { new TestEntity { Id = 1, Name = "Item1" } };
    int pageIndex = 1;

    // Act
    var paginatedList = new PaginatedList<TestEntity>(items, 10, pageIndex, 5);

    // Assert
    paginatedList.HasPreviousPage.Should().BeTrue();
  }

  [Fact]
  public void HasNextPage_WhenOnLastPage_ShouldReturnFalse()
  {
    // Arrange
    var items = new List<TestEntity> { new TestEntity { Id = 1, Name = "Item1" } };
    int count = 10;
    int pageIndex = 1; // Last page (0-indexed)
    int pageSize = 5; // Total pages = 2

    // Act
    var paginatedList = new PaginatedList<TestEntity>(items, count, pageIndex, pageSize);

    // Assert
    paginatedList.HasNextPage.Should().BeFalse();
  }

  [Fact]
  public void HasNextPage_WhenNotOnLastPage_ShouldReturnTrue()
  {
    // Arrange
    var items = new List<TestEntity> { new TestEntity { Id = 1, Name = "Item1" } };
    int count = 15;
    int pageIndex = 0; // First page
    int pageSize = 5; // Total pages = 3

    // Act
    var paginatedList = new PaginatedList<TestEntity>(items, count, pageIndex, pageSize);

    // Assert
    paginatedList.HasNextPage.Should().BeTrue();
  }

  [Fact]
  public void HasNextPage_WhenOnlyOnePage_ShouldReturnFalse()
  {
    // Arrange
    var items = new List<TestEntity>
        {
            new TestEntity { Id = 1, Name = "Item1" },
            new TestEntity { Id = 2, Name = "Item2" }
        };
    int count = 2;
    int pageIndex = 0;
    int pageSize = 10;

    // Act
    var paginatedList = new PaginatedList<TestEntity>(items, count, pageIndex, pageSize);

    // Assert
    paginatedList.HasNextPage.Should().BeFalse();
    paginatedList.HasPreviousPage.Should().BeFalse();
  }

  [Fact]
  public async Task CreateAsync_WithValidQueryable_ShouldReturnPaginatedList()
  {
    // Arrange
    var sourceData = new List<TestEntity>
        {
            new TestEntity { Id = 1, Name = "Item1" },
            new TestEntity { Id = 2, Name = "Item2" },
            new TestEntity { Id = 3, Name = "Item3" },
            new TestEntity { Id = 4, Name = "Item4" },
            new TestEntity { Id = 5, Name = "Item5" },
            new TestEntity { Id = 6, Name = "Item6" },
            new TestEntity { Id = 7, Name = "Item7" },
            new TestEntity { Id = 8, Name = "Item8" },
            new TestEntity { Id = 9, Name = "Item9" },
            new TestEntity { Id = 10, Name = "Item10" }
        };
    var queryable = sourceData.BuildMock();
    int pageIndex = 1;
    int pageSize = 3;

    // Act
    var result = await PaginatedList<TestEntity>.CreateAsync(queryable, pageIndex, pageSize);

    // Assert
    result.Should().NotBeNull();
    result.Items.Should().HaveCount(3);
    result.Items.Select(x => x.Id).Should().BeEquivalentTo(new[] { 4, 5, 6 }); // Second page
    result.TotalCount.Should().Be(10);
    result.PageIndex.Should().Be(1);
    result.PageSize.Should().Be(3);
    result.TotalPages.Should().Be(4);
  }

  [Fact]
  public async Task CreateAsync_WithFirstPage_ShouldReturnCorrectItems()
  {
    // Arrange
    var sourceData = new List<TestEntity>
        {
            new TestEntity { Id = 1, Name = "A" },
            new TestEntity { Id = 2, Name = "B" },
            new TestEntity { Id = 3, Name = "C" },
            new TestEntity { Id = 4, Name = "D" },
            new TestEntity { Id = 5, Name = "E" }
        };
    var queryable = sourceData.BuildMock();
    int pageIndex = 0;
    int pageSize = 2;

    // Act
    var result = await PaginatedList<TestEntity>.CreateAsync(queryable, pageIndex, pageSize);

    // Assert
    result.Items.Should().HaveCount(2);
    result.Items.Select(x => x.Name).Should().BeEquivalentTo(new[] { "A", "B" });
    result.PageIndex.Should().Be(0);
    result.HasPreviousPage.Should().BeFalse();
    result.HasNextPage.Should().BeTrue();
  }

  [Fact]
  public async Task CreateAsync_WithLastPage_ShouldReturnRemainingItems()
  {
    // Arrange
    var sourceData = new List<TestEntity>
        {
            new TestEntity { Id = 1, Name = "Item1" },
            new TestEntity { Id = 2, Name = "Item2" },
            new TestEntity { Id = 3, Name = "Item3" },
            new TestEntity { Id = 4, Name = "Item4" },
            new TestEntity { Id = 5, Name = "Item5" },
            new TestEntity { Id = 6, Name = "Item6" },
            new TestEntity { Id = 7, Name = "Item7" }
        };
    var queryable = sourceData.BuildMock();
    int pageIndex = 2; // Third page
    int pageSize = 3;

    // Act
    var result = await PaginatedList<TestEntity>.CreateAsync(queryable, pageIndex, pageSize);

    // Assert
    result.Items.Should().HaveCount(1); // Only one item on last page
    result.Items[0].Id.Should().Be(7);
    result.HasPreviousPage.Should().BeTrue();
    result.HasNextPage.Should().BeFalse();
  }

  [Fact]
  public async Task CreateAsync_WithEmptyQueryable_ShouldReturnEmptyList()
  {
    // Arrange
    var sourceData = new List<TestEntity>();
    var queryable = sourceData.BuildMock();
    int pageIndex = 0;
    int pageSize = 10;

    // Act
    var result = await PaginatedList<TestEntity>.CreateAsync(queryable, pageIndex, pageSize);

    // Assert
    result.Items.Should().BeEmpty();
    result.TotalCount.Should().Be(0);
    result.TotalPages.Should().Be(0);
    result.HasPreviousPage.Should().BeFalse();
    result.HasNextPage.Should().BeFalse();
  }

  [Fact]
  public async Task CreateAsync_WithPageIndexBeyondData_ShouldReturnEmptyItems()
  {
    // Arrange
    var sourceData = new List<TestEntity>
        {
            new TestEntity { Id = 1, Name = "Item1" },
            new TestEntity { Id = 2, Name = "Item2" },
            new TestEntity { Id = 3, Name = "Item3" }
        };
    var queryable = sourceData.BuildMock();
    int pageIndex = 10; // Way beyond available data
    int pageSize = 5;

    // Act
    var result = await PaginatedList<TestEntity>.CreateAsync(queryable, pageIndex, pageSize);

    // Assert
    result.Items.Should().BeEmpty();
    result.TotalCount.Should().Be(3);
    result.PageIndex.Should().Be(10);
  }

  [Fact]
  public async Task CreateAsync_WithLargePageSize_ShouldReturnAllItems()
  {
    // Arrange
    var sourceData = new List<TestEntity>
        {
            new TestEntity { Id = 1, Name = "A" },
            new TestEntity { Id = 2, Name = "B" },
            new TestEntity { Id = 3, Name = "C" },
            new TestEntity { Id = 4, Name = "D" },
            new TestEntity { Id = 5, Name = "E" }
        };
    var queryable = sourceData.BuildMock();
    int pageIndex = 0;
    int pageSize = 100; // Larger than data

    // Act
    var result = await PaginatedList<TestEntity>.CreateAsync(queryable, pageIndex, pageSize);

    // Assert
    result.Items.Should().HaveCount(5);
    result.Items.Should().BeEquivalentTo(sourceData);
    result.TotalPages.Should().Be(1);
    result.HasNextPage.Should().BeFalse();
  }

  [Fact]
  public void Constructor_WithComplexObject_ShouldWork()
  {
    // Arrange
    var items = new List<TestUser>
        {
            new TestUser { Id = 1, Name = "User1" },
            new TestUser { Id = 2, Name = "User2" }
        };
    int count = 20;
    int pageIndex = 0;
    int pageSize = 2;

    // Act
    var paginatedList = new PaginatedList<TestUser>(items, count, pageIndex, pageSize);

    // Assert
    paginatedList.Items.Should().HaveCount(2);
    paginatedList.Items[0].Name.Should().Be("User1");
    paginatedList.TotalPages.Should().Be(10);
  }

  [Fact]
  public async Task CreateAsync_WithComplexObject_ShouldReturnPaginatedList()
  {
    // Arrange
    var sourceData = new List<TestUser>
        {
            new TestUser { Id = 1, Name = "User1" },
            new TestUser { Id = 2, Name = "User2" },
            new TestUser { Id = 3, Name = "User3" },
            new TestUser { Id = 4, Name = "User4" }
        };
    var queryable = sourceData.BuildMock();
    int pageIndex = 1;
    int pageSize = 2;

    // Act
    var result = await PaginatedList<TestUser>.CreateAsync(queryable, pageIndex, pageSize);

    // Assert
    result.Items.Should().HaveCount(2);
    result.Items[0].Id.Should().Be(3);
    result.Items[1].Id.Should().Be(4);
    result.TotalCount.Should().Be(4);
  }

  [Fact]
  public void Constructor_WithSingleItem_ShouldCalculateCorrectly()
  {
    // Arrange
    var items = new List<TestEntity> { new TestEntity { Id = 42, Name = "Single" } };
    int count = 1;
    int pageIndex = 0;
    int pageSize = 10;

    // Act
    var paginatedList = new PaginatedList<TestEntity>(items, count, pageIndex, pageSize);

    // Assert
    paginatedList.Items.Should().ContainSingle();
    paginatedList.TotalPages.Should().Be(1);
    paginatedList.HasPreviousPage.Should().BeFalse();
    paginatedList.HasNextPage.Should().BeFalse();
  }

  // Helper classes for testing
  private class TestEntity
  {
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
  }

  private class TestUser
  {
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
  }
}