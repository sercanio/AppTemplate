using AppTemplate.Application.Data.DynamicQuery;
using FluentAssertions;

namespace AppTemplate.Application.Tests.Unit.Data.DynamicQueryTests;

public class SortUnitTests
{
  [Fact]
  public void Constructor_WithNoParameters_ShouldCreateInstanceWithEmptyStrings()
  {
    // Act
    var sort = new Sort();

    // Assert
    sort.Should().NotBeNull();
    sort.Field.Should().BeEmpty();
    sort.Dir.Should().BeEmpty();
  }

  [Fact]
  public void Constructor_WithParameters_ShouldSetProperties()
  {
    // Arrange
    string field = "UserName";
    string dir = "asc";

    // Act
    var sort = new Sort(field, dir);

    // Assert
    sort.Should().NotBeNull();
    sort.Field.Should().Be(field);
    sort.Dir.Should().Be(dir);
  }

  [Fact]
  public void Constructor_WithDescendingDirection_ShouldSetProperties()
  {
    // Arrange
    string field = "CreatedDate";
    string dir = "desc";

    // Act
    var sort = new Sort(field, dir);

    // Assert
    sort.Field.Should().Be(field);
    sort.Dir.Should().Be(dir);
  }

  [Fact]
  public void Field_Property_CanBeSet()
  {
    // Arrange
    var sort = new Sort();
    string newField = "Email";

    // Act
    sort.Field = newField;

    // Assert
    sort.Field.Should().Be(newField);
  }

  [Fact]
  public void Dir_Property_CanBeSet()
  {
    // Arrange
    var sort = new Sort();
    string newDir = "desc";

    // Act
    sort.Dir = newDir;

    // Assert
    sort.Dir.Should().Be(newDir);
  }

  [Fact]
  public void Sort_AsRecord_ShouldSupportValueEquality()
  {
    // Arrange
    var sort1 = new Sort("UserName", "asc");
    var sort2 = new Sort("UserName", "asc");

    // Act & Assert
    sort1.Should().Be(sort2);
    (sort1 == sort2).Should().BeTrue();
  }

  [Fact]
  public void Sort_WithDifferentValues_ShouldNotBeEqual()
  {
    // Arrange
    var sort1 = new Sort("UserName", "asc");
    var sort2 = new Sort("UserName", "desc");

    // Act & Assert
    sort1.Should().NotBe(sort2);
    (sort1 != sort2).Should().BeTrue();
  }

  [Fact]
  public void Sort_WithDifferentFields_ShouldNotBeEqual()
  {
    // Arrange
    var sort1 = new Sort("UserName", "asc");
    var sort2 = new Sort("Email", "asc");

    // Act & Assert
    sort1.Should().NotBe(sort2);
  }

  [Fact]
  public void Constructor_WithEmptyStrings_ShouldAcceptEmptyValues()
  {
    // Arrange & Act
    var sort = new Sort(string.Empty, string.Empty);

    // Assert
    sort.Field.Should().BeEmpty();
    sort.Dir.Should().BeEmpty();
  }

  [Fact]
  public void Sort_WithComplexFieldPath_ShouldWork()
  {
    // Arrange
    string field = "User.Profile.Name";
    string dir = "asc";

    // Act
    var sort = new Sort(field, dir);

    // Assert
    sort.Field.Should().Be(field);
    sort.Dir.Should().Be(dir);
  }

  [Theory]
  [InlineData("asc")]
  [InlineData("desc")]
  [InlineData("ASC")]
  [InlineData("DESC")]
  public void Constructor_WithVariousDirections_ShouldAcceptAllValues(string direction)
  {
    // Arrange
    string field = "UserName";

    // Act
    var sort = new Sort(field, direction);

    // Assert
    sort.Dir.Should().Be(direction);
  }

  [Fact]
  public void Sort_ToString_ShouldReturnRecordRepresentation()
  {
    // Arrange
    var sort = new Sort("UserName", "asc");

    // Act
    var result = sort.ToString();

    // Assert
    result.Should().Contain("UserName");
    result.Should().Contain("asc");
  }

  [Fact]
  public void Sort_GetHashCode_ShouldBeSameForEqualObjects()
  {
    // Arrange
    var sort1 = new Sort("UserName", "asc");
    var sort2 = new Sort("UserName", "asc");

    // Act & Assert
    sort1.GetHashCode().Should().Be(sort2.GetHashCode());
  }

  [Fact]
  public void Sort_GetHashCode_ShouldBeDifferentForDifferentObjects()
  {
    // Arrange
    var sort1 = new Sort("UserName", "asc");
    var sort2 = new Sort("UserName", "desc");

    // Act & Assert
    sort1.GetHashCode().Should().NotBe(sort2.GetHashCode());
  }
}
