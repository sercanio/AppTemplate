using AppTemplate.Application.Services.Localization;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using System.Text;
using Xunit;

namespace AppTemplate.Application.Tests.Unit.Services.LocalizationServiceTests;

public class LocalizationServiceUnitTests : IDisposable
{
  private readonly Mock<ILogger<LocalizationService>> _loggerMock;
  private readonly string _testResourceDirectory;
  private readonly string _originalBaseDirectory;

  public LocalizationServiceUnitTests()
  {
    _loggerMock = new Mock<ILogger<LocalizationService>>();

    // Store original base directory
    _originalBaseDirectory = AppContext.BaseDirectory;

    // Create a temporary test directory for Resources
    _testResourceDirectory = Path.Combine(Path.GetTempPath(), "LocalizationTests", Guid.NewGuid().ToString());
    Directory.CreateDirectory(_testResourceDirectory);
  }

  public void Dispose()
  {
    // Clean up test directory
    if (Directory.Exists(_testResourceDirectory))
    {
      Directory.Delete(_testResourceDirectory, true);
    }
  }

  [Fact]
  public void Constructor_WhenNoResourceDirectory_ShouldNotThrow()
  {
    // Arrange & Act
    var service = new LocalizationService(_loggerMock.Object);

    // Assert
    service.Should().NotBeNull();
  }

  [Fact]
  public void GetLocalizedString_WhenKeyExistsInRequestedLanguage_ShouldReturnLocalizedString()
  {
    // Arrange
    var resourceDir = CreateResourceDirectory();
    CreateYamlFile(resourceDir, "en-US.yaml", @"
greeting: Hello
welcome: Welcome to AppTemplate
");

    var service = CreateServiceWithResourceDirectory(resourceDir);

    // Act
    var result = service.GetLocalizedString("greeting", "en-US");

    // Assert
    result.Should().Be("Hello");
  }

  [Fact]
  public void GetLocalizedString_WhenKeyExistsInNestedStructure_ShouldReturnLocalizedString()
  {
    // Arrange
    var resourceDir = CreateResourceDirectory();
    CreateYamlFile(resourceDir, "en-US.yaml", @"
errors:
  validation:
    required: This field is required
    email: Invalid email format
");

    var service = CreateServiceWithResourceDirectory(resourceDir);

    // Act
    var result = service.GetLocalizedString("errors.validation.required", "en-US");

    // Assert
    result.Should().Be("This field is required");
  }

  [Fact]
  public void GetLocalizedString_WhenKeyNotFoundInSpecificCulture_ShouldFallbackToBaseLanguage()
  {
    // Arrange
    var resourceDir = CreateResourceDirectory();
    CreateYamlFile(resourceDir, "en.yaml", @"
greeting: Hello
");
    CreateYamlFile(resourceDir, "en-GB.yaml", @"
welcome: Welcome
");

    var service = CreateServiceWithResourceDirectory(resourceDir);

    // Act
    var result = service.GetLocalizedString("greeting", "en-GB");

    // Assert
    result.Should().Be("Hello");
  }

  [Fact]
  public void GetLocalizedString_WhenKeyNotFoundInBaseLanguage_ShouldFallbackToEnglish()
  {
    // Arrange
    var resourceDir = CreateResourceDirectory();
    CreateYamlFile(resourceDir, "en-US.yaml", @"
greeting: Hello
");
    CreateYamlFile(resourceDir, "tr-TR.yaml", @"
welcome: Hoşgeldiniz
");

    var service = CreateServiceWithResourceDirectory(resourceDir);

    // Act
    var result = service.GetLocalizedString("greeting", "tr-TR");

    // Assert
    result.Should().Be("Hello");
  }

  [Fact]
  public void GetLocalizedString_WhenKeyNotFoundAnywhere_ShouldReturnKey()
  {
    // Arrange
    var resourceDir = CreateResourceDirectory();
    CreateYamlFile(resourceDir, "en-US.yaml", @"
greeting: Hello
");

    var service = CreateServiceWithResourceDirectory(resourceDir);

    // Act
    var result = service.GetLocalizedString("nonexistent.key", "en-US");

    // Assert
    result.Should().Be("nonexistent.key");
  }

  [Fact]
  public void GetLocalizedString_WithMultipleLanguages_ShouldReturnCorrectLanguage()
  {
    // Arrange
    var resourceDir = CreateResourceDirectory();
    CreateYamlFile(resourceDir, "en-US.yaml", @"
greeting: Hello
");
    CreateYamlFile(resourceDir, "tr-TR.yaml", @"
greeting: Merhaba
");
    CreateYamlFile(resourceDir, "es-ES.yaml", @"
greeting: Hola
");

    var service = CreateServiceWithResourceDirectory(resourceDir);

    // Act
    var resultEn = service.GetLocalizedString("greeting", "en-US");
    var resultTr = service.GetLocalizedString("greeting", "tr-TR");
    var resultEs = service.GetLocalizedString("greeting", "es-ES");

    // Assert
    resultEn.Should().Be("Hello");
    resultTr.Should().Be("Merhaba");
    resultEs.Should().Be("Hola");
  }

  [Fact]
  public void GetLocalizedString_WithComplexNestedStructure_ShouldFlattenCorrectly()
  {
    // Arrange
    var resourceDir = CreateResourceDirectory();
    CreateYamlFile(resourceDir, "en-US.yaml", @"
authentication:
  login:
    title: Login
    fields:
      username: Username
      password: Password
    errors:
      invalid: Invalid credentials
      locked: Account locked
");

    var service = CreateServiceWithResourceDirectory(resourceDir);

    // Act
    var title = service.GetLocalizedString("authentication.login.title", "en-US");
    var username = service.GetLocalizedString("authentication.login.fields.username", "en-US");
    var error = service.GetLocalizedString("authentication.login.errors.invalid", "en-US");

    // Assert
    title.Should().Be("Login");
    username.Should().Be("Username");
    error.Should().Be("Invalid credentials");
  }

  [Fact]
  public void LoadLocalizedMessages_WhenYamlFileIsInvalid_ShouldLogErrorAndContinue()
  {
    // Arrange
    var resourceDir = CreateResourceDirectory();
    CreateInvalidYamlFile(resourceDir, "invalid.yaml");
    CreateYamlFile(resourceDir, "en-US.yaml", @"
greeting: Hello
");

    // Act
    var service = CreateServiceWithResourceDirectory(resourceDir);
    var result = service.GetLocalizedString("greeting", "en-US");

    // Assert
    result.Should().Be("Hello");
    _loggerMock.Verify(
        x => x.Log(
            LogLevel.Error,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => true),
            It.IsAny<Exception>(),
            It.Is<Func<It.IsAnyType, Exception?, string>>((v, t) => true)),
        Times.AtLeastOnce);
  }

  [Fact]
  public void LoadLocalizedMessages_WhenResourceDirectoryDoesNotExist_ShouldReturnEmptyDictionary()
  {
    // Arrange
    // Don't create the directory at all - test with completely non-existent path
    var nonExistentDir = Path.Combine(_testResourceDirectory, "NonExistent", "Resources");

    // Act
    var service = CreateServiceWithNonExistentDirectory(nonExistentDir);
    var result = service.GetLocalizedString("any.key", "en-US");

    // Assert
    result.Should().Be("any.key");
  }

  [Fact]
  public void GetLocalizedString_WithNumericValue_ShouldConvertToString()
  {
    // Arrange
    var resourceDir = CreateResourceDirectory();
    CreateYamlFile(resourceDir, "en-US.yaml", @"
config:
  maxAttempts: 5
  timeout: 30
");

    var service = CreateServiceWithResourceDirectory(resourceDir);

    // Act
    var maxAttempts = service.GetLocalizedString("config.maxAttempts", "en-US");
    var timeout = service.GetLocalizedString("config.timeout", "en-US");

    // Assert
    maxAttempts.Should().Be("5");
    timeout.Should().Be("30");
  }

  [Fact]
  public void GetLocalizedString_WithBooleanValue_ShouldConvertToString()
  {
    // Arrange
    var resourceDir = CreateResourceDirectory();
    CreateYamlFile(resourceDir, "en-US.yaml", @"
features:
  darkMode: true
  notifications: false
");

    var service = CreateServiceWithResourceDirectory(resourceDir);

    // Act
    var darkMode = service.GetLocalizedString("features.darkMode", "en-US");
    var notifications = service.GetLocalizedString("features.notifications", "en-US");

    // Assert
    // YAML deserializes booleans as lowercase "true"/"false"
    darkMode.Should().Be("true");
    notifications.Should().Be("false");
  }

  [Fact]
  public void GetLocalizedString_WithSpecialCharacters_ShouldHandleCorrectly()
  {
    // Arrange
    var resourceDir = CreateResourceDirectory();
    CreateYamlFile(resourceDir, "tr-TR.yaml", @"
messages:
  special: 'Türkçe karakterler: ç, ğ, ı, ö, ş, ü'
  symbols: 'Symbols: @#$%^&*()'
");

    var service = CreateServiceWithResourceDirectory(resourceDir);

    // Act
    var special = service.GetLocalizedString("messages.special", "tr-TR");
    var symbols = service.GetLocalizedString("messages.symbols", "tr-TR");

    // Assert
    special.Should().Contain("Türkçe");
    symbols.Should().Contain("@#$%");
  }

  [Fact]
  public void GetLocalizedString_WithEmptyString_ShouldReturnEmptyString()
  {
    // Arrange
    var resourceDir = CreateResourceDirectory();
    CreateYamlFile(resourceDir, "en-US.yaml", @"
empty: ''
");

    var service = CreateServiceWithResourceDirectory(resourceDir);

    // Act
    var result = service.GetLocalizedString("empty", "en-US");

    // Assert
    result.Should().BeEmpty();
  }

  [Theory]
  [InlineData("en-US")]
  [InlineData("en-GB")]
  [InlineData("en-CA")]
  public void GetLocalizedString_WithEnglishVariants_ShouldFallbackCorrectly(string culture)
  {
    // Arrange
    var resourceDir = CreateResourceDirectory();
    CreateYamlFile(resourceDir, "en.yaml", @"
greeting: Hello
");

    var service = CreateServiceWithResourceDirectory(resourceDir);

    // Act
    var result = service.GetLocalizedString("greeting", culture);

    // Assert
    result.Should().Be("Hello");
  }

  [Fact]
  public void LoadLocalizedMessages_ShouldLoadMultipleFiles()
  {
    // Arrange
    var resourceDir = CreateResourceDirectory();
    CreateYamlFile(resourceDir, "en-US.yaml", @"
greeting: Hello
");
    CreateYamlFile(resourceDir, "tr-TR.yaml", @"
greeting: Merhaba
");
    CreateYamlFile(resourceDir, "es-ES.yaml", @"
greeting: Hola
");

    // Act
    var service = CreateServiceWithResourceDirectory(resourceDir);

    // Assert
    var enResult = service.GetLocalizedString("greeting", "en-US");
    var trResult = service.GetLocalizedString("greeting", "tr-TR");
    var esResult = service.GetLocalizedString("greeting", "es-ES");

    enResult.Should().Be("Hello");
    trResult.Should().Be("Merhaba");
    esResult.Should().Be("Hola");

    _loggerMock.Verify(
        x => x.Log(
            LogLevel.Information,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Successfully loaded")),
            It.IsAny<Exception>(),
            It.Is<Func<It.IsAnyType, Exception?, string>>((v, t) => true)),
        Times.AtLeast(3));
  }

  [Fact]
  public void GetLocalizedString_WithMixedCase_ShouldBeCaseSensitive()
  {
    // Arrange
    var resourceDir = CreateResourceDirectory();
    CreateYamlFile(resourceDir, "en-US.yaml", @"
myKey: Value1
MyKey: Value2
");

    var service = CreateServiceWithResourceDirectory(resourceDir);

    // Act
    var result1 = service.GetLocalizedString("myKey", "en-US");
    var result2 = service.GetLocalizedString("MyKey", "en-US");

    // Assert
    result1.Should().Be("Value1");
    result2.Should().Be("Value2");
  }

  [Fact]
  public void GetLocalizedString_WithLongNestedPath_ShouldWork()
  {
    // Arrange
    var resourceDir = CreateResourceDirectory();
    CreateYamlFile(resourceDir, "en-US.yaml", @"
level1:
  level2:
    level3:
      level4:
        level5:
          deepValue: Found it!
");

    var service = CreateServiceWithResourceDirectory(resourceDir);

    // Act
    var result = service.GetLocalizedString("level1.level2.level3.level4.level5.deepValue", "en-US");

    // Assert
    result.Should().Be("Found it!");
  }

  [Fact]
  public void GetLocalizedString_WithNullValue_ShouldReturnEmptyString()
  {
    // Arrange
    var resourceDir = CreateResourceDirectory();
    CreateYamlFile(resourceDir, "en-US.yaml", @"
nullValue: null
");

    var service = CreateServiceWithResourceDirectory(resourceDir);

    // Act
    var result = service.GetLocalizedString("nullValue", "en-US");

    // Assert
    result.Should().BeEmpty();
  }

  // Helper methods
  private string CreateResourceDirectory()
  {
    var resourcePath = Path.Combine(_testResourceDirectory, "Resources");
    Directory.CreateDirectory(resourcePath);
    return resourcePath;
  }

  private void CreateYamlFile(string directory, string fileName, string content)
  {
    var filePath = Path.Combine(directory, fileName);
    File.WriteAllText(filePath, content, Encoding.UTF8);
  }

  private void CreateInvalidYamlFile(string directory, string fileName)
  {
    var filePath = Path.Combine(directory, fileName);
    File.WriteAllText(filePath, "{ invalid yaml content [[[", Encoding.UTF8);
  }

  private LocalizationService CreateServiceWithResourceDirectory(string resourceDir)
  {
    // Create a custom AppContext base directory setup
    var testBaseDir = Path.GetDirectoryName(resourceDir)!;

    // We need to temporarily change AppContext.BaseDirectory
    // Since we can't directly change it, we'll copy files to the actual base directory
    var actualResourceDir = Path.Combine(AppContext.BaseDirectory, "Resources");

    if (Directory.Exists(actualResourceDir))
    {
      Directory.Delete(actualResourceDir, true);
    }

    CopyDirectory(resourceDir, actualResourceDir);

    return new LocalizationService(_loggerMock.Object);
  }

  private LocalizationService CreateServiceWithNonExistentDirectory(string nonExistentDir)
  {
    // Clean up any existing Resources directory to ensure it doesn't exist
    var actualResourceDir = Path.Combine(AppContext.BaseDirectory, "Resources");

    if (Directory.Exists(actualResourceDir))
    {
      Directory.Delete(actualResourceDir, true);
    }

    // Create the service without copying any files
    return new LocalizationService(_loggerMock.Object);
  }

  private void CopyDirectory(string sourceDir, string targetDir)
  {
    // Check if source directory exists before attempting to copy
    if (!Directory.Exists(sourceDir))
    {
      return;
    }

    Directory.CreateDirectory(targetDir);

    foreach (var file in Directory.GetFiles(sourceDir))
    {
      var fileName = Path.GetFileName(file);
      var destFile = Path.Combine(targetDir, fileName);
      File.Copy(file, destFile, true);
    }
  }
}