using AppTemplate.Application.Data.DynamicQuery;
using FluentAssertions;

namespace AppTemplate.Application.Tests.Unit.Data.DynamicQueryTests;

public class FilterUnitTests
{
  [Fact]
  public void Constructor_WithNoParameters_ShouldCreateInstanceWithEmptyStrings()
  {
    // Act
    var filter = new Filter();

    // Assert
    filter.Should().NotBeNull();
    filter.Field.Should().BeEmpty();
    filter.Operator.Should().BeEmpty();
    filter.Value.Should().BeNull();
    filter.Logic.Should().BeNull();
    filter.Filters.Should().BeNull();
    filter.IsCaseSensitive.Should().BeTrue();
  }

  [Fact]
  public void Constructor_WithParameters_ShouldSetProperties()
  {
    // Arrange
    string field = "UserName";
    string @operator = "eq";

    // Act
    var filter = new Filter(field, @operator);

    // Assert
    filter.Should().NotBeNull();
    filter.Field.Should().Be(field);
    filter.Operator.Should().Be(@operator);
    filter.IsCaseSensitive.Should().BeTrue();
  }

  [Fact]
  public void Field_Property_CanBeSet()
  {
    // Arrange
    var filter = new Filter();
    string newField = "Email";

    // Act
    filter.Field = newField;

    // Assert
    filter.Field.Should().Be(newField);
  }

  [Fact]
  public void Operator_Property_CanBeSet()
  {
    // Arrange
    var filter = new Filter();
    string newOperator = "contains";

    // Act
    filter.Operator = newOperator;

    // Assert
    filter.Operator.Should().Be(newOperator);
  }

  [Fact]
  public void Value_Property_CanBeSet()
  {
    // Arrange
    var filter = new Filter();
    string value = "test@example.com";

    // Act
    filter.Value = value;

    // Assert
    filter.Value.Should().Be(value);
  }

  [Fact]
  public void Logic_Property_CanBeSet()
  {
    // Arrange
    var filter = new Filter();
    string logic = "and";

    // Act
    filter.Logic = logic;

    // Assert
    filter.Logic.Should().Be(logic);
  }

  [Fact]
  public void IsCaseSensitive_Property_CanBeSet()
  {
    // Arrange
    var filter = new Filter();

    // Act
    filter.IsCaseSensitive = false;

    // Assert
    filter.IsCaseSensitive.Should().BeFalse();
  }

  [Fact]
  public void IsCaseSensitive_DefaultValue_ShouldBeTrue()
  {
    // Arrange & Act
    var filter = new Filter("UserName", "eq");

    // Assert
    filter.IsCaseSensitive.Should().BeTrue();
  }

  [Fact]
  public void Filters_Property_CanBeSet()
  {
    // Arrange
    var filter = new Filter();
    var nestedFilters = new List<Filter>
        {
            new Filter("UserName", "contains") { Value = "admin" },
            new Filter("Id", "gt") { Value = "0" }
        };

    // Act
    filter.Filters = nestedFilters;

    // Assert
    filter.Filters.Should().NotBeNull();
    filter.Filters.Should().HaveCount(2);
  }

  [Fact]
  public void Filter_WithComplexNestedStructure_ShouldWork()
  {
    // Arrange
    var filter = new Filter
    {
      Logic = "and",
      Filters = new List<Filter>
            {
                new Filter("UserName", "contains") { Value = "admin" },
                new Filter
                {
                    Logic = "or",
                    Filters = new List<Filter>
                    {
                        new Filter("Id", "gt") { Value = "0" },
                        new Filter("Id", "lt") { Value = "100" }
                    }
                }
            }
    };

    // Assert
    filter.Logic.Should().Be("and");
    filter.Filters.Should().HaveCount(2);
    filter.Filters!.ElementAt(1).Filters.Should().HaveCount(2);
  }

  [Fact]
  public void Filter_AsRecord_ShouldSupportValueEquality()
  {
    // Arrange
    var filter1 = new Filter("UserName", "eq") { Value = "admin" };
    var filter2 = new Filter("UserName", "eq") { Value = "admin" };

    // Act & Assert
    filter1.Should().Be(filter2);
    (filter1 == filter2).Should().BeTrue();
  }

  [Fact]
  public void Filter_WithDifferentValues_ShouldNotBeEqual()
  {
    // Arrange
    var filter1 = new Filter("UserName", "eq") { Value = "admin" };
    var filter2 = new Filter("UserName", "eq") { Value = "user" };

    // Act & Assert
    filter1.Should().NotBe(filter2);
    (filter1 != filter2).Should().BeTrue();
  }

  [Theory]
  [InlineData("eq", "equals")]
  [InlineData("neq", "not equals")]
  [InlineData("lt", "less than")]
  [InlineData("lte", "less than or equal")]
  [InlineData("gt", "greater than")]
  [InlineData("gte", "greater than or equal")]
  [InlineData("contains", "contains")]
  [InlineData("startswith", "starts with")]
  [InlineData("endswith", "ends with")]
  public void Constructor_WithVariousOperators_ShouldAcceptAllValues(string @operator, string description)
  {
    // Arrange & Act
    var filter = new Filter("UserName", @operator);

    // Assert
    filter.Operator.Should().Be(@operator);
  }

  [Fact]
  public void Filter_WithNullValue_ShouldAcceptNull()
  {
    // Arrange & Act
    var filter = new Filter("UserName", "eq") { Value = null };

    // Assert
    filter.Value.Should().BeNull();
  }

  [Fact]
  public void Filter_WithEmptyFiltersCollection_ShouldWork()
  {
    // Arrange & Act
    var filter = new Filter
    {
      Logic = "and",
      Filters = new List<Filter>()
    };

    // Assert
    filter.Filters.Should().NotBeNull();
    filter.Filters.Should().BeEmpty();
  }

  [Theory]
  [InlineData("and")]
  [InlineData("or")]
  [InlineData("AND")]
  [InlineData("OR")]
  public void Logic_Property_WithVariousValues_ShouldAcceptAll(string logic)
  {
    // Arrange
    var filter = new Filter();

    // Act
    filter.Logic = logic;

    // Assert
    filter.Logic.Should().Be(logic);
  }

  [Fact]
  public void Filter_WithCaseInsensitiveSearch_ShouldSetPropertyCorrectly()
  {
    // Arrange & Act
    var filter = new Filter("UserName", "contains")
    {
      Value = "admin",
      IsCaseSensitive = false
    };

    // Assert
    filter.IsCaseSensitive.Should().BeFalse();
    filter.Value.Should().Be("admin");
  }

  [Fact]
  public void Filter_WithComplexFieldPath_ShouldWork()
  {
    // Arrange
    string field = "User.Profile.Name";
    string @operator = "contains";

    // Act
    var filter = new Filter(field, @operator);

    // Assert
    filter.Field.Should().Be(field);
  }

  [Fact]
  public void Filter_ToString_ShouldReturnRecordRepresentation()
  {
    // Arrange
    var filter = new Filter("UserName", "eq") { Value = "admin" };

    // Act
    var result = filter.ToString();

    // Assert
    result.Should().Contain("UserName");
    result.Should().Contain("eq");
  }

  [Fact]
  public void Filter_GetHashCode_ShouldBeSameForEqualObjects()
  {
    // Arrange
    var filter1 = new Filter("UserName", "eq") { Value = "admin" };
    var filter2 = new Filter("UserName", "eq") { Value = "admin" };

    // Act & Assert
    filter1.GetHashCode().Should().Be(filter2.GetHashCode());
  }

  [Fact]
  public void Filter_WithMultipleLevelsOfNesting_ShouldWork()
  {
    // Arrange & Act
    var filter = new Filter
    {
      Logic = "and",
      Filters = new List<Filter>
            {
                new Filter("Status", "eq") { Value = "Active" },
                new Filter
                {
                    Logic = "or",
                    Filters = new List<Filter>
                    {
                        new Filter("Role", "eq") { Value = "Admin" },
                        new Filter
                        {
                            Logic = "and",
                            Filters = new List<Filter>
                            {
                                new Filter("Age", "gte") { Value = "18" },
                                new Filter("Age", "lte") { Value = "65" }
                            }
                        }
                    }
                }
            }
    };

    // Assert
    filter.Logic.Should().Be("and");
    filter.Filters.Should().HaveCount(2);
    filter.Filters!.ElementAt(1).Logic.Should().Be("or");
    filter.Filters.ElementAt(1).Filters.Should().HaveCount(2);
  }

  [Fact]
  public void Filter_WithNumericValue_ShouldStoreAsString()
  {
    // Arrange & Act
    var filter = new Filter("Age", "gt") { Value = "18" };

    // Assert
    filter.Value.Should().Be("18");
    filter.Value.Should().BeOfType<string>();
  }

  [Fact]
  public void Filter_WithDateValue_ShouldStoreAsString()
  {
    // Arrange
    string dateValue = "2024-01-01";

    // Act
    var filter = new Filter("CreatedDate", "gte") { Value = dateValue };

    // Assert
    filter.Value.Should().Be(dateValue);
  }

  [Fact]
  public void Filter_ComplexScenario_SimulatingRealUsage()
  {
    // Arrange & Act - Simulating a complex filter like in GetAllUsersDynamic
    var filter = new Filter
    {
      Logic = "and",
      Filters = new List<Filter>
            {
                new Filter("UserName", "contains")
                {
                    Value = "admin",
                    IsCaseSensitive = false
                },
                new Filter
                {
                    Logic = "or",
                    Filters = new List<Filter>
                    {
                        new Filter("Roles.Name", "eq") { Value = "Administrator" },
                        new Filter("Roles.Name", "eq") { Value = "Manager" }
                    }
                },
                new Filter("DeletedOnUtc", "eq") { Value = null }
            }
    };

    // Assert
    filter.Logic.Should().Be("and");
    filter.Filters.Should().HaveCount(3);
    filter.Filters!.First().IsCaseSensitive.Should().BeFalse();
    filter.Filters.ElementAt(1).Filters.Should().HaveCount(2);
    filter.Filters.Last().Value.Should().BeNull();
  }
}
