using AppTemplate.Domain.Mailing;

namespace AppTemplate.Domain.Tests.Unit.Mailing;

[Trait("Category", "Unit")]
public class ToEmailUnitTests
{
  [Fact]
  public void Constructor_Default_ShouldInitializeWithEmptyStrings()
  {
    // Act
    var toEmail = new ToEmail();

    // Assert
    Assert.Equal(string.Empty, toEmail.Email);
    Assert.Equal(string.Empty, toEmail.FullName);
  }

  [Fact]
  public void Constructor_WithParameters_ShouldSetProperties()
  {
    // Arrange
    var email = "test@example.com";
    var fullName = "John Doe";

    // Act
    var toEmail = new ToEmail(email, fullName);

    // Assert
    Assert.Equal(email, toEmail.Email);
    Assert.Equal(fullName, toEmail.FullName);
  }

  [Fact]
  public void Email_CanBeSetAndRetrieved()
  {
    // Arrange
    var toEmail = new ToEmail();
    var email = "user@example.com";

    // Act
    toEmail.Email = email;

    // Assert
    Assert.Equal(email, toEmail.Email);
  }

  [Fact]
  public void FullName_CanBeSetAndRetrieved()
  {
    // Arrange
    var toEmail = new ToEmail();
    var fullName = "Jane Smith";

    // Act
    toEmail.FullName = fullName;

    // Assert
    Assert.Equal(fullName, toEmail.FullName);
  }

  [Fact]
  public void Constructor_WithEmptyStrings_ShouldSetEmptyValues()
  {
    // Arrange
    var email = string.Empty;
    var fullName = string.Empty;

    // Act
    var toEmail = new ToEmail(email, fullName);

    // Assert
    Assert.Equal(string.Empty, toEmail.Email);
    Assert.Equal(string.Empty, toEmail.FullName);
  }

  [Fact]
  public void Constructor_WithNullValues_ShouldHandleGracefully()
  {
    // Arrange
    string? email = null;
    string? fullName = null;

    // Act
    var toEmail = new ToEmail(email!, fullName!);

    // Assert
    Assert.Null(toEmail.Email);
    Assert.Null(toEmail.FullName);
  }

  [Theory]
  [InlineData("test@example.com", "Test User")]
  [InlineData("admin@domain.com", "Admin User")]
  [InlineData("support@company.org", "Support Team")]
  public void Constructor_WithVariousInputs_ShouldSetCorrectly(string email, string fullName)
  {
    // Act
    var toEmail = new ToEmail(email, fullName);

    // Assert
    Assert.Equal(email, toEmail.Email);
    Assert.Equal(fullName, toEmail.FullName);
  }

  [Fact]
  public void Email_WithValidEmailFormat_ShouldSetCorrectly()
  {
    // Arrange
    var toEmail = new ToEmail();
    var email = "user.name+tag@example.co.uk";

    // Act
    toEmail.Email = email;

    // Assert
    Assert.Equal(email, toEmail.Email);
  }

  [Fact]
  public void FullName_WithSpecialCharacters_ShouldPreserveContent()
  {
    // Arrange
    var toEmail = new ToEmail();
    var fullName = "John O'Connor-Smith";

    // Act
    toEmail.FullName = fullName;

    // Assert
    Assert.Equal(fullName, toEmail.FullName);
  }

  [Fact]
  public void FullName_WithUnicodeCharacters_ShouldPreserveContent()
  {
    // Arrange
    var fullName = "田中 太郎";
    var email = "tanaka@example.jp";

    // Act
    var toEmail = new ToEmail(email, fullName);

    // Assert
    Assert.Equal(fullName, toEmail.FullName);
    Assert.Equal(email, toEmail.Email);
  }

  [Fact]
  public void Properties_CanBeUpdatedMultipleTimes()
  {
    // Arrange
    var toEmail = new ToEmail("initial@example.com", "Initial Name");

    // Act
    toEmail.Email = "updated1@example.com";
    toEmail.FullName = "Updated Name 1";
    toEmail.Email = "updated2@example.com";
    toEmail.FullName = "Updated Name 2";

    // Assert
    Assert.Equal("updated2@example.com", toEmail.Email);
    Assert.Equal("Updated Name 2", toEmail.FullName);
  }

  [Fact]
  public void MultipleInstances_ShouldBeIndependent()
  {
    // Arrange & Act
    var toEmail1 = new ToEmail("user1@example.com", "User One");
    var toEmail2 = new ToEmail("user2@example.com", "User Two");

    // Assert
    Assert.NotSame(toEmail1, toEmail2);
    Assert.NotEqual(toEmail1.Email, toEmail2.Email);
    Assert.NotEqual(toEmail1.FullName, toEmail2.FullName);
  }

  [Fact]
  public void Email_WithLongEmailAddress_ShouldSetCorrectly()
  {
    // Arrange
    var toEmail = new ToEmail();
    var longEmail = "very.long.email.address.with.multiple.dots@subdomain.example.co.uk";

    // Act
    toEmail.Email = longEmail;

    // Assert
    Assert.Equal(longEmail, toEmail.Email);
  }

  [Fact]
  public void FullName_WithLongName_ShouldSetCorrectly()
  {
    // Arrange
    var toEmail = new ToEmail();
    var longName = "Dr. Jonathan Alexander Christopher Wellington-Smythe III, Esq.";

    // Act
    toEmail.FullName = longName;

    // Assert
    Assert.Equal(longName, toEmail.FullName);
  }

  [Fact]
  public void Email_SetToNull_ShouldBeNull()
  {
    // Arrange
    var toEmail = new ToEmail("test@example.com", "Test User");

    // Act
    toEmail.Email = null!;

    // Assert
    Assert.Null(toEmail.Email);
  }

  [Fact]
  public void FullName_SetToNull_ShouldBeNull()
  {
    // Arrange
    var toEmail = new ToEmail("test@example.com", "Test User");

    // Act
    toEmail.FullName = null!;

    // Assert
    Assert.Null(toEmail.FullName);
  }

  [Fact]
  public void Constructor_WithWhitespaceValues_ShouldPreserveWhitespace()
  {
    // Arrange
    var email = "   test@example.com   ";
    var fullName = "   Test User   ";

    // Act
    var toEmail = new ToEmail(email, fullName);

    // Assert
    Assert.Equal(email, toEmail.Email);
    Assert.Equal(fullName, toEmail.FullName);
  }

  [Fact]
  public void Email_WithCaseSensitivity_ShouldPreserveCase()
  {
    // Arrange
    var toEmail = new ToEmail();
    var email = "Test.User@Example.COM";

    // Act
    toEmail.Email = email;

    // Assert
    Assert.Equal(email, toEmail.Email);
  }

  [Fact]
  public void FullName_WithNumbers_ShouldSetCorrectly()
  {
    // Arrange
    var fullName = "User 123";
    var email = "user123@example.com";

    // Act
    var toEmail = new ToEmail(email, fullName);

    // Assert
    Assert.Equal(fullName, toEmail.FullName);
  }

  [Fact]
  public void ToEmail_DefaultConstructor_PropertiesAreNotNull()
  {
    // Act
    var toEmail = new ToEmail();

    // Assert
    Assert.NotNull(toEmail.Email);
    Assert.NotNull(toEmail.FullName);
  }

  [Fact]
  public void ToEmail_ParameterizedConstructor_BothPropertiesSet()
  {
    // Arrange
    var email = "contact@example.com";
    var fullName = "Contact Person";

    // Act
    var toEmail = new ToEmail(email, fullName);

    // Assert
    Assert.NotNull(toEmail.Email);
    Assert.NotNull(toEmail.FullName);
    Assert.Equal(email, toEmail.Email);
    Assert.Equal(fullName, toEmail.FullName);
  }

  [Theory]
  [InlineData("", "")]
  [InlineData("test@test.com", "")]
  [InlineData("", "Test User")]
  public void Constructor_WithPartialData_ShouldSetCorrectly(string email, string fullName)
  {
    // Act
    var toEmail = new ToEmail(email, fullName);

    // Assert
    Assert.Equal(email, toEmail.Email);
    Assert.Equal(fullName, toEmail.FullName);
  }

  [Fact]
  public void Email_WithSpecialEmailCharacters_ShouldSetCorrectly()
  {
    // Arrange
    var toEmail = new ToEmail();
    var email = "user+filter@sub-domain.example-site.com";

    // Act
    toEmail.Email = email;

    // Assert
    Assert.Equal(email, toEmail.Email);
  }

  [Fact]
  public void FullName_WithMultipleSpaces_ShouldPreserveSpaces()
  {
    // Arrange
    var fullName = "John    Doe";
    var email = "john.doe@example.com";

    // Act
    var toEmail = new ToEmail(email, fullName);

    // Assert
    Assert.Equal(fullName, toEmail.FullName);
    Assert.Contains("    ", toEmail.FullName);
  }

  [Fact]
  public void ToEmail_AfterConstruction_PropertiesAreAccessible()
  {
    // Arrange
    var email = "access@example.com";
    var fullName = "Access Test";
    var toEmail = new ToEmail(email, fullName);

    // Act
    var retrievedEmail = toEmail.Email;
    var retrievedFullName = toEmail.FullName;

    // Assert
    Assert.Equal(email, retrievedEmail);
    Assert.Equal(fullName, retrievedFullName);
  }

  [Fact]
  public void Email_SetToEmptyString_ShouldBeEmpty()
  {
    // Arrange
    var toEmail = new ToEmail("test@example.com", "Test User");

    // Act
    toEmail.Email = string.Empty;

    // Assert
    Assert.Equal(string.Empty, toEmail.Email);
  }

  [Fact]
  public void FullName_SetToEmptyString_ShouldBeEmpty()
  {
    // Arrange
    var toEmail = new ToEmail("test@example.com", "Test User");

    // Act
    toEmail.FullName = string.Empty;

    // Assert
    Assert.Equal(string.Empty, toEmail.FullName);
  }

  [Fact]
  public void Constructor_CreatesNewInstance()
  {
    // Act
    var toEmail1 = new ToEmail();
    var toEmail2 = new ToEmail();

    // Assert
    Assert.NotSame(toEmail1, toEmail2);
  }

  [Fact]
  public void FullName_WithAccentedCharacters_ShouldPreserveContent()
  {
    // Arrange
    var fullName = "José García-Pérez";
    var email = "jose@example.es";

    // Act
    var toEmail = new ToEmail(email, fullName);

    // Assert
    Assert.Equal(fullName, toEmail.FullName);
  }

  [Fact]
  public void ToEmail_WithCompleteData_AllPropertiesSet()
  {
    // Arrange
    var email = "complete@example.com";
    var fullName = "Complete User";

    // Act
    var toEmail = new ToEmail(email, fullName)
    {
      Email = "updated@example.com",
      FullName = "Updated User"
    };

    // Assert
    Assert.Equal("updated@example.com", toEmail.Email);
    Assert.Equal("Updated User", toEmail.FullName);
  }

  [Fact]
  public void Email_WithNumericDomain_ShouldSetCorrectly()
  {
    // Arrange
    var toEmail = new ToEmail();
    var email = "user@123.456.789.012";

    // Act
    toEmail.Email = email;

    // Assert
    Assert.Equal(email, toEmail.Email);
  }

  [Fact]
  public void FullName_WithTitleAndSuffix_ShouldSetCorrectly()
  {
    // Arrange
    var fullName = "Dr. Jane Smith, PhD";
    var email = "dr.smith@university.edu";

    // Act
    var toEmail = new ToEmail(email, fullName);

    // Assert
    Assert.Equal(fullName, toEmail.FullName);
  }
}