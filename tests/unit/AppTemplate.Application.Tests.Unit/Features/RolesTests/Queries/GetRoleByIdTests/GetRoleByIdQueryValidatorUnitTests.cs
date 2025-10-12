using AppTemplate.Application.Features.Roles.Queries.GetRoleById;
using FluentAssertions;

namespace AppTemplate.Application.Tests.Unit.Features.RolesTests.Queries.GetRoleByIdTests;

[Trait("Category", "Unit")]
public class GetRoleByIdQueryValidatorUnitTests
{
  private readonly GetRoleByIdQueryValidator _validator;

  public GetRoleByIdQueryValidatorUnitTests()
  {
    _validator = new GetRoleByIdQueryValidator();
  }

  [Fact]
  public void Validate_ShouldPass_WhenQueryIsValid()
  {
    // Arrange
    var query = new GetRoleByIdQuery(Guid.NewGuid());

    // Act
    var result = _validator.Validate(query);

    // Assert
    result.IsValid.Should().BeTrue();
    result.Errors.Should().BeEmpty();
  }

  [Fact]
  public void Validate_ShouldFail_WhenRoleIdIsEmpty()
  {
    // Arrange
    var query = new GetRoleByIdQuery(Guid.Empty);

    // Act
    var result = _validator.Validate(query);

    // Assert
    result.IsValid.Should().BeFalse();
    result.Errors.Should().ContainSingle();
    result.Errors[0].PropertyName.Should().Be("RoleId");
    result.Errors[0].ErrorMessage.Should().Contain("empty");
  }

  [Fact]
  public void Validate_ShouldPass_WithValidGuid()
  {
    // Arrange
    var roleId = Guid.Parse("12345678-1234-1234-1234-123456789012");
    var query = new GetRoleByIdQuery(roleId);

    // Act
    var result = _validator.Validate(query);

    // Assert
    result.IsValid.Should().BeTrue();
    result.Errors.Should().BeEmpty();
  }

  [Theory]
  [InlineData("11111111-1111-1111-1111-111111111111")]
  [InlineData("aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee")]
  [InlineData("ffffffff-ffff-ffff-ffff-ffffffffffff")]
  [InlineData("12345678-90ab-cdef-1234-567890abcdef")]
  public void Validate_ShouldPass_ForVariousValidGuids(string guidString)
  {
    // Arrange
    var roleId = Guid.Parse(guidString);
    var query = new GetRoleByIdQuery(roleId);

    // Act
    var result = _validator.Validate(query);

    // Assert
    result.IsValid.Should().BeTrue();
    result.Errors.Should().BeEmpty();
  }

  [Fact]
  public void Validate_ShouldHaveOneError_WhenRoleIdIsEmpty()
  {
    // Arrange
    var query = new GetRoleByIdQuery(Guid.Empty);

    // Act
    var result = _validator.Validate(query);

    // Assert
    result.Errors.Should().ContainSingle();
    result.Errors[0].PropertyName.Should().Be("RoleId");
  }

  [Fact]
  public void Validate_ErrorMessage_ShouldIndicateEmptyValue()
  {
    // Arrange
    var query = new GetRoleByIdQuery(Guid.Empty);

    // Act
    var result = _validator.Validate(query);

    // Assert
    result.Errors[0].ErrorMessage.Should().NotBeNullOrWhiteSpace();
    result.Errors[0].ErrorMessage.ToLower().Should().Contain("empty");
  }

  [Fact]
  public void Constructor_ShouldInitializeValidator()
  {
    // Arrange & Act
    var validator = new GetRoleByIdQueryValidator();

    // Assert
    validator.Should().NotBeNull();
  }

  [Fact]
  public void Validator_ShouldBeReusable_ForMultipleValidations()
  {
    // Arrange
    var query1 = new GetRoleByIdQuery(Guid.NewGuid());
    var query2 = new GetRoleByIdQuery(Guid.Empty);
    var query3 = new GetRoleByIdQuery(Guid.NewGuid());

    // Act
    var result1 = _validator.Validate(query1);
    var result2 = _validator.Validate(query2);
    var result3 = _validator.Validate(query3);

    // Assert
    result1.IsValid.Should().BeTrue();
    result2.IsValid.Should().BeFalse();
    result3.IsValid.Should().BeTrue();
  }

  [Fact]
  public void Validate_ShouldOnlyValidateRoleId()
  {
    // Arrange - Create query with empty RoleId
    var query = new GetRoleByIdQuery(Guid.Empty);

    // Act
    var result = _validator.Validate(query);

    // Assert
    result.Errors.Should().ContainSingle(); // Only RoleId validation
    result.Errors.Should().OnlyContain(e => e.PropertyName == "RoleId");
  }

  [Fact]
  public void Validate_ShouldNotValidateCacheKey()
  {
    // Arrange - The query implements ICachedQuery but validator should only validate RoleId
    var query = new GetRoleByIdQuery(Guid.NewGuid());

    // Act
    var result = _validator.Validate(query);

    // Assert
    result.IsValid.Should().BeTrue();
    result.Errors.Should().NotContain(e => e.PropertyName == "CacheKey");
  }

  [Fact]
  public void Validate_ShouldPass_WithNewlyGeneratedGuid()
  {
    // Arrange
    var roleId = Guid.NewGuid();
    var query = new GetRoleByIdQuery(roleId);

    // Act
    var result = _validator.Validate(query);

    // Assert
    result.IsValid.Should().BeTrue();
    result.Errors.Should().BeEmpty();
  }

  [Fact]
  public void Validate_MultipleEmptyGuid_ShouldAlwaysFail()
  {
    // Arrange
    var query1 = new GetRoleByIdQuery(Guid.Empty);
    var query2 = new GetRoleByIdQuery(Guid.Empty);

    // Act
    var result1 = _validator.Validate(query1);
    var result2 = _validator.Validate(query2);

    // Assert
    result1.IsValid.Should().BeFalse();
    result2.IsValid.Should().BeFalse();
    result1.Errors.Should().HaveCount(1);
    result2.Errors.Should().HaveCount(1);
  }

  [Fact]
  public void Validate_ShouldUseSameValidationLogic_ForAllInstances()
  {
    // Arrange
    var validator1 = new GetRoleByIdQueryValidator();
    var validator2 = new GetRoleByIdQueryValidator();
    var query = new GetRoleByIdQuery(Guid.Empty);

    // Act
    var result1 = validator1.Validate(query);
    var result2 = validator2.Validate(query);

    // Assert
    result1.IsValid.Should().Be(result2.IsValid);
    result1.Errors.Should().HaveCount(result2.Errors.Count);
  }

  [Fact]
  public async Task ValidateAsync_ShouldPass_WhenQueryIsValid()
  {
    // Arrange
    var query = new GetRoleByIdQuery(Guid.NewGuid());

    // Act
    var result = await _validator.ValidateAsync(query);

    // Assert
    result.IsValid.Should().BeTrue();
    result.Errors.Should().BeEmpty();
  }

  [Fact]
  public async Task ValidateAsync_ShouldFail_WhenRoleIdIsEmpty()
  {
    // Arrange
    var query = new GetRoleByIdQuery(Guid.Empty);

    // Act
    var result = await _validator.ValidateAsync(query);

    // Assert
    result.IsValid.Should().BeFalse();
    result.Errors.Should().ContainSingle();
    result.Errors[0].PropertyName.Should().Be("RoleId");
  }

  [Fact]
  public void Validate_ShouldBeConsistent_WithMultipleCallsForSameQuery()
  {
    // Arrange
    var query = new GetRoleByIdQuery(Guid.NewGuid());

    // Act
    var result1 = _validator.Validate(query);
    var result2 = _validator.Validate(query);
    var result3 = _validator.Validate(query);

    // Assert
    result1.IsValid.Should().BeTrue();
    result2.IsValid.Should().BeTrue();
    result3.IsValid.Should().BeTrue();
    result1.Errors.Should().BeEmpty();
    result2.Errors.Should().BeEmpty();
    result3.Errors.Should().BeEmpty();
  }

  [Fact]
  public void Validate_ShouldNotThrow_ForAnyValidGuid()
  {
    // Arrange
    var query = new GetRoleByIdQuery(Guid.NewGuid());

    // Act
    Action act = () => _validator.Validate(query);

    // Assert
    act.Should().NotThrow();
  }

  [Fact]
  public void Validate_EmptyGuid_ShouldHaveSpecificPropertyName()
  {
    // Arrange
    var query = new GetRoleByIdQuery(Guid.Empty);

    // Act
    var result = _validator.Validate(query);

    // Assert
    result.Errors.Should().ContainSingle(e => e.PropertyName == "RoleId");
  }

  [Fact]
  public void Validate_ShouldImplementFluentValidation()
  {
    // Arrange & Act
    var validator = new GetRoleByIdQueryValidator();

    // Assert
    validator.Should().BeAssignableTo<FluentValidation.AbstractValidator<GetRoleByIdQuery>>();
  }
}