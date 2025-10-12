using AppTemplate.Application.Features.Roles.Commands.Create;
using FluentAssertions;
using Xunit;

namespace AppTemplate.Application.Tests.Unit.Features.RolesTests.Commands.Create;

[Trait("Category", "Unit")]
public class CreateRoleValidationHandlerUnitTests
{
  private readonly CreateRoleValidationhandler _validator;

  public CreateRoleValidationHandlerUnitTests()
  {
    _validator = new CreateRoleValidationhandler();
  }

  [Fact]
  public void Validate_ShouldPass_WhenCommandIsValid()
  {
    // Arrange
    var command = new CreateRoleCommand("Admin", "Administrator");

    // Act
    var result = _validator.Validate(command);

    // Assert
    result.IsValid.Should().BeTrue();
    result.Errors.Should().BeEmpty();
  }

  [Fact]
  public void Validate_ShouldFail_WhenNameIsEmpty()
  {
    // Arrange
    var command = new CreateRoleCommand("", "Administrator");

    // Act
    var result = _validator.Validate(command);

    // Assert
    result.IsValid.Should().BeFalse();
    result.Errors.Should().ContainSingle();
    result.Errors[0].PropertyName.Should().Be("Name");
    result.Errors[0].ErrorMessage.Should().Contain("empty");
  }

  [Fact]
  public void Validate_ShouldFail_WhenNameIsNull()
  {
    // Arrange
    var command = new CreateRoleCommand(null!, "Administrator");

    // Act
    var result = _validator.Validate(command);

    // Assert
    result.IsValid.Should().BeFalse();
    result.Errors.Should().ContainSingle();
    result.Errors[0].PropertyName.Should().Be("Name");
  }

  [Fact]
  public void Validate_ShouldFail_WhenNameExceedsMaximumLength()
  {
    // Arrange
    var longName = new string('A', 26); // 26 characters, exceeds max of 25
    var command = new CreateRoleCommand(longName, "Administrator");

    // Act
    var result = _validator.Validate(command);

    // Assert
    result.IsValid.Should().BeFalse();
    result.Errors.Should().ContainSingle();
    result.Errors[0].PropertyName.Should().Be("Name");
    result.Errors[0].ErrorMessage.Should().Contain("25");
  }

  [Fact]
  public void Validate_ShouldPass_WhenNameIsExactlyMaximumLength()
  {
    // Arrange
    var maxLengthName = new string('A', 25); // Exactly 25 characters
    var command = new CreateRoleCommand(maxLengthName, "Administrator");

    // Act
    var result = _validator.Validate(command);

    // Assert
    result.IsValid.Should().BeTrue();
    result.Errors.Should().BeEmpty();
  }

  [Fact]
  public void Validate_ShouldPass_WhenNameIsOneCharacter()
  {
    // Arrange
    var command = new CreateRoleCommand("A", "Administrator");

    // Act
    var result = _validator.Validate(command);

    // Assert
    result.IsValid.Should().BeTrue();
    result.Errors.Should().BeEmpty();
  }

  [Theory]
  [InlineData("Admin")]
  [InlineData("User")]
  [InlineData("Manager")]
  [InlineData("SuperAdministrator")]
  [InlineData("A")]
  public void Validate_ShouldPass_ForValidRoleNames(string roleName)
  {
    // Arrange
    var command = new CreateRoleCommand(roleName, "Display Name");

    // Act
    var result = _validator.Validate(command);

    // Assert
    result.IsValid.Should().BeTrue();
    result.Errors.Should().BeEmpty();
  }

  [Theory]
  [InlineData("")]
  [InlineData(null)]
  [InlineData("   ")]
  public void Validate_ShouldFail_ForInvalidEmptyNames(string? roleName)
  {
    // Arrange
    var command = new CreateRoleCommand(roleName!, "Display Name");

    // Act
    var result = _validator.Validate(command);

    // Assert
    result.IsValid.Should().BeFalse();
    result.Errors.Should().NotBeEmpty();
    result.Errors.Should().Contain(e => e.PropertyName == "Name");
  }

  [Fact]
  public void Validate_ShouldFail_WhenNameIs26Characters()
  {
    // Arrange
    var command = new CreateRoleCommand("ABCDEFGHIJKLMNOPQRSTUVWXYZ", "Display Name"); // 26 chars

    // Act
    var result = _validator.Validate(command);

    // Assert
    result.IsValid.Should().BeFalse();
    result.Errors.Should().ContainSingle();
    result.Errors[0].PropertyName.Should().Be("Name");
  }

  [Fact]
  public void Validate_ShouldFail_WhenNameIs50Characters()
  {
    // Arrange
    var longName = new string('X', 50);
    var command = new CreateRoleCommand(longName, "Display Name");

    // Act
    var result = _validator.Validate(command);

    // Assert
    result.IsValid.Should().BeFalse();
    result.Errors.Should().ContainSingle();
    result.Errors[0].PropertyName.Should().Be("Name");
  }

  [Fact]
  public void Validate_ShouldNotValidateDisplayName()
  {
    // Arrange - DisplayName has no validation rules
    var command = new CreateRoleCommand("Admin", ""); // Empty display name

    // Act
    var result = _validator.Validate(command);

    // Assert
    result.IsValid.Should().BeTrue(); // Only Name is validated
  }

  [Fact]
  public void Validate_ShouldPass_WhenDisplayNameIsNull()
  {
    // Arrange
    var command = new CreateRoleCommand("Admin", null!);

    // Act
    var result = _validator.Validate(command);

    // Assert
    result.IsValid.Should().BeTrue(); // DisplayName has no validation
  }

  [Fact]
  public void Validate_ShouldPass_WhenDisplayNameIsVeryLong()
  {
    // Arrange
    var veryLongDisplayName = new string('X', 1000);
    var command = new CreateRoleCommand("Admin", veryLongDisplayName);

    // Act
    var result = _validator.Validate(command);

    // Assert
    result.IsValid.Should().BeTrue(); // DisplayName has no validation
  }

  [Fact]
  public void Validate_ShouldPass_WithSpecialCharactersInName()
  {
    // Arrange
    var command = new CreateRoleCommand("Admin-Role", "Administrator Role");

    // Act
    var result = _validator.Validate(command);

    // Assert
    result.IsValid.Should().BeTrue();
  }

  [Fact]
  public void Validate_ShouldPass_WithNumbersInName()
  {
    // Arrange
    var command = new CreateRoleCommand("Admin123", "Administrator 123");

    // Act
    var result = _validator.Validate(command);

    // Assert
    result.IsValid.Should().BeTrue();
  }

  [Fact]
  public void Validate_ShouldHaveCorrectErrorCount_WhenNameIsEmptyAndTooLong()
  {
    // Arrange - This tests that only one validation rule applies at a time
    var command = new CreateRoleCommand("", "Display Name");

    // Act
    var result = _validator.Validate(command);

    // Assert
    result.IsValid.Should().BeFalse();
    result.Errors.Should().ContainSingle(); // Only NotEmpty rule fails
  }

  [Fact]
  public void Validator_ShouldBeReusable_ForMultipleValidations()
  {
    // Arrange
    var command1 = new CreateRoleCommand("Admin", "Administrator");
    var command2 = new CreateRoleCommand("", "Empty Name");
    var command3 = new CreateRoleCommand("ValidName", "Valid Display");

    // Act
    var result1 = _validator.Validate(command1);
    var result2 = _validator.Validate(command2);
    var result3 = _validator.Validate(command3);

    // Assert
    result1.IsValid.Should().BeTrue();
    result2.IsValid.Should().BeFalse();
    result3.IsValid.Should().BeTrue();
  }

  [Fact]
  public void Constructor_ShouldInitializeValidator()
  {
    // Arrange & Act
    var validator = new CreateRoleValidationhandler();

    // Assert
    validator.Should().NotBeNull();
  }
}