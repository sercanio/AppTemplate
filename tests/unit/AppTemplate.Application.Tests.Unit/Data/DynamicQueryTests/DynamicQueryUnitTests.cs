using AppTemplate.Application.Data.DynamicQuery;
using FluentAssertions;

namespace AppTemplate.Application.Tests.Unit.Data.DynamicQueryTests;

public class DynamicQueryUnitTests
{
  [Fact]
  public void Constructor_WithNoParameters_ShouldCreateInstance()
  {
    // Act
    var dynamicQuery = new DynamicQuery();

    // Assert
    dynamicQuery.Should().NotBeNull();
    dynamicQuery.Sort.Should().BeNull();
    dynamicQuery.Filter.Should().BeNull();
  }

  [Fact]
  public void Constructor_WithParameters_ShouldSetProperties()
  {
    // Arrange
    var sorts = new List<Sort>
        {
            new Sort { Field = "UserName", Dir = "asc" }
        };
    var filter = new Filter
    {
      Field = "Id",
      Operator = "eq",
      Value = "123"
    };

    // Act
    var dynamicQuery = new DynamicQuery(sorts, filter);

    // Assert
    dynamicQuery.Should().NotBeNull();
    dynamicQuery.Sort.Should().NotBeNull();
    dynamicQuery.Sort!.Should().HaveCount(1);
    dynamicQuery.Sort.Should().BeEquivalentTo(sorts);
    dynamicQuery.Filter.Should().NotBeNull();
    dynamicQuery.Filter.Should().Be(filter);
  }

  [Fact]
  public void Constructor_WithNullParameters_ShouldAcceptNullValues()
  {
    // Act
    var dynamicQuery = new DynamicQuery(null, null);

    // Assert
    dynamicQuery.Should().NotBeNull();
    dynamicQuery.Sort.Should().BeNull();
    dynamicQuery.Filter.Should().BeNull();
  }

  [Fact]
  public void Sort_Property_CanBeSet()
  {
    // Arrange
    var dynamicQuery = new DynamicQuery();
    var sorts = new List<Sort>
        {
            new Sort { Field = "CreatedDate", Dir = "desc" }
        };

    // Act
    dynamicQuery.Sort = sorts;

    // Assert
    dynamicQuery.Sort.Should().NotBeNull();
    dynamicQuery.Sort!.Should().HaveCount(1);
    dynamicQuery.Sort.Should().BeEquivalentTo(sorts);
  }

  [Fact]
  public void Filter_Property_CanBeSet()
  {
    // Arrange
    var dynamicQuery = new DynamicQuery();
    var filter = new Filter
    {
      Field = "UserName",
      Operator = "contains",
      Value = "admin"
    };

    // Act
    dynamicQuery.Filter = filter;

    // Assert
    dynamicQuery.Filter.Should().NotBeNull();
    dynamicQuery.Filter.Should().Be(filter);
  }

  [Fact]
  public void DynamicQuery_WithMultipleSorts_ShouldStoreAllSorts()
  {
    // Arrange
    var sorts = new List<Sort>
        {
            new Sort { Field = "UserName", Dir = "asc" },
            new Sort { Field = "CreatedDate", Dir = "desc" },
            new Sort { Field = "Id", Dir = "asc" }
        };

    // Act
    var dynamicQuery = new DynamicQuery(sorts, null);

    // Assert
    dynamicQuery.Sort.Should().HaveCount(3);
    dynamicQuery.Sort.Should().BeEquivalentTo(sorts);
  }

  [Fact]
  public void DynamicQuery_WithComplexFilter_ShouldStoreFilter()
  {
    // Arrange
    var filter = new Filter
    {
      Logic = "and",
      Filters = new List<Filter>
            {
                new Filter { Field = "UserName", Operator = "contains", Value = "admin" },
                new Filter { Field = "Id", Operator = "gt", Value = "0" }
            }
    };

    // Act
    var dynamicQuery = new DynamicQuery(null, filter);

    // Assert
    dynamicQuery.Filter.Should().NotBeNull();
    dynamicQuery.Filter!.Logic.Should().Be("and");
    dynamicQuery.Filter.Filters.Should().HaveCount(2);
  }

  [Fact]
  public void DynamicQuery_AsRecord_ShouldSupportValueEquality()
  {
    // Arrange
    var sorts = new List<Sort> { new Sort { Field = "UserName", Dir = "asc" } };
    var filter = new Filter { Field = "Id", Operator = "eq", Value = "123" };

    var query1 = new DynamicQuery(sorts, filter);
    var query2 = new DynamicQuery(sorts, filter);

    // Act & Assert
    query1.Should().Be(query2);
    (query1 == query2).Should().BeTrue();
  }

  [Fact]
  public void DynamicQuery_WithDifferentValues_ShouldNotBeEqual()
  {
    // Arrange
    var sorts1 = new List<Sort> { new Sort { Field = "UserName", Dir = "asc" } };
    var sorts2 = new List<Sort> { new Sort { Field = "Id", Dir = "desc" } };

    var query1 = new DynamicQuery(sorts1, null);
    var query2 = new DynamicQuery(sorts2, null);

    // Act & Assert
    query1.Should().NotBe(query2);
  }

  [Fact]
  public void DynamicQuery_EmptyCollections_ShouldBeHandledCorrectly()
  {
    // Arrange
    var emptySorts = new List<Sort>();

    // Act
    var dynamicQuery = new DynamicQuery(emptySorts, null);

    // Assert
    dynamicQuery.Sort.Should().NotBeNull();
    dynamicQuery.Sort!.Should().BeEmpty();
  }
}