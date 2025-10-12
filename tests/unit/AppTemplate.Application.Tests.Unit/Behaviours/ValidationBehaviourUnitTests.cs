using AppTemplate.Application.Behaviors;
using Ardalis.Result;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Moq;

namespace AppTemplate.Application.Tests.Unit.Behaviours;

public class ValidationBehaviourUnitTests
{
  private readonly CancellationToken _cancellationToken;

  public ValidationBehaviourUnitTests()
  {
    _cancellationToken = CancellationToken.None;
  }

  [Fact]
  public async Task Handle_WhenNoValidatorsExist_ShouldCallNextDelegate()
  {
    // Arrange
    var request = new TestValidationRequest();
    var expectedResult = Result.Success();
    var validators = Array.Empty<IValidator<TestValidationRequest>>();
    var validationBehavior = new ValidationBehavior<TestValidationRequest, Result>(validators);

    // Act
    var result = await validationBehavior.Handle(request, _ => Task.FromResult(expectedResult), _cancellationToken);

    // Assert
    Assert.True(result.IsSuccess);
    Assert.Same(expectedResult, result);
  }

  [Fact]
  public async Task Handle_WhenValidationPasses_ShouldCallNextDelegate()
  {
    // Arrange
    var request = new TestValidationRequest { Name = "Valid Name", Age = 25 };
    var expectedResult = Result.Success();

    var mockValidator = new Mock<IValidator<TestValidationRequest>>();
    mockValidator
        .Setup(v => v.ValidateAsync(It.IsAny<ValidationContext<TestValidationRequest>>(), It.IsAny<CancellationToken>()))
        .ReturnsAsync(new ValidationResult());

    var validators = new[] { mockValidator.Object };
    var validationBehavior = new ValidationBehavior<TestValidationRequest, Result>(validators);

    // Act
    var result = await validationBehavior.Handle(request, _ => Task.FromResult(expectedResult), _cancellationToken);

    // Assert
    Assert.True(result.IsSuccess);
    mockValidator.Verify(v => v.ValidateAsync(
        It.Is<ValidationContext<TestValidationRequest>>(ctx => ctx.InstanceToValidate == request),
        _cancellationToken), Times.Once);
  }

  [Fact]
  public async Task Handle_WhenValidationFails_ShouldReturnInvalidResult()
  {
    // Arrange
    var request = new TestValidationRequest { Name = "", Age = -1 };

    var validationFailures = new List<ValidationFailure>
        {
            new ValidationFailure("Name", "Name is required"),
            new ValidationFailure("Age", "Age must be positive")
        };

    var mockValidator = new Mock<IValidator<TestValidationRequest>>();
    mockValidator
        .Setup(v => v.ValidateAsync(It.IsAny<ValidationContext<TestValidationRequest>>(), It.IsAny<CancellationToken>()))
        .ReturnsAsync(new ValidationResult(validationFailures));

    var validators = new[] { mockValidator.Object };
    var validationBehavior = new ValidationBehavior<TestValidationRequest, Result>(validators);

    // Act
    var result = await validationBehavior.Handle(request, _ => Task.FromResult(Result.Success()), _cancellationToken);

    // Assert
    Assert.False(result.IsSuccess);
    Assert.Equal(ResultStatus.Invalid, result.Status);
    Assert.Equal(2, result.ValidationErrors.ToList().Count);
    Assert.Contains(result.ValidationErrors, e => e.ErrorMessage == "Name is required");
    Assert.Contains(result.ValidationErrors, e => e.ErrorMessage == "Age must be positive");
  }

  [Fact]
  public async Task Handle_WhenMultipleValidatorsExist_ShouldRunAllValidators()
  {
    // Arrange
    var request = new TestValidationRequest { Name = "Test", Age = 25 };

    var mockValidator1 = new Mock<IValidator<TestValidationRequest>>();
    mockValidator1
        .Setup(v => v.ValidateAsync(It.IsAny<ValidationContext<TestValidationRequest>>(), It.IsAny<CancellationToken>()))
        .ReturnsAsync(new ValidationResult());

    var mockValidator2 = new Mock<IValidator<TestValidationRequest>>();
    mockValidator2
        .Setup(v => v.ValidateAsync(It.IsAny<ValidationContext<TestValidationRequest>>(), It.IsAny<CancellationToken>()))
        .ReturnsAsync(new ValidationResult());

    var validators = new[] { mockValidator1.Object, mockValidator2.Object };
    var validationBehavior = new ValidationBehavior<TestValidationRequest, Result>(validators);

    // Act
    var result = await validationBehavior.Handle(request, _ => Task.FromResult(Result.Success()), _cancellationToken);

    // Assert
    Assert.True(result.IsSuccess);
    mockValidator1.Verify(v => v.ValidateAsync(
        It.IsAny<ValidationContext<TestValidationRequest>>(),
        _cancellationToken), Times.Once);
    mockValidator2.Verify(v => v.ValidateAsync(
        It.IsAny<ValidationContext<TestValidationRequest>>(),
        _cancellationToken), Times.Once);
  }

  [Fact]
  public async Task Handle_WhenMultipleValidatorsFail_ShouldCombineAllErrors()
  {
    // Arrange
    var request = new TestValidationRequest { Name = "", Age = -1 };

    var validationFailures1 = new List<ValidationFailure>
        {
            new ValidationFailure("Name", "Name is required")
        };

    var validationFailures2 = new List<ValidationFailure>
        {
            new ValidationFailure("Age", "Age must be positive"),
            new ValidationFailure("Age", "Age must be less than 120")
        };

    var mockValidator1 = new Mock<IValidator<TestValidationRequest>>();
    mockValidator1
        .Setup(v => v.ValidateAsync(It.IsAny<ValidationContext<TestValidationRequest>>(), It.IsAny<CancellationToken>()))
        .ReturnsAsync(new ValidationResult(validationFailures1));

    var mockValidator2 = new Mock<IValidator<TestValidationRequest>>();
    mockValidator2
        .Setup(v => v.ValidateAsync(It.IsAny<ValidationContext<TestValidationRequest>>(), It.IsAny<CancellationToken>()))
        .ReturnsAsync(new ValidationResult(validationFailures2));

    var validators = new[] { mockValidator1.Object, mockValidator2.Object };
    var validationBehavior = new ValidationBehavior<TestValidationRequest, Result>(validators);

    // Act
    var result = await validationBehavior.Handle(request, _ => Task.FromResult(Result.Success()), _cancellationToken);

    // Assert
    Assert.False(result.IsSuccess);
    Assert.Equal(ResultStatus.Invalid, result.Status);
    Assert.Equal(3, result.ValidationErrors.ToList().Count);
    Assert.Contains(result.ValidationErrors, e => e.ErrorMessage == "Name is required");
    Assert.Contains(result.ValidationErrors, e => e.ErrorMessage == "Age must be positive");
    Assert.Contains(result.ValidationErrors, e => e.ErrorMessage == "Age must be less than 120");
  }

  [Fact]
  public async Task Handle_WhenValidationFails_ShouldNotCallNextDelegate()
  {
    // Arrange
    var request = new TestValidationRequest { Name = "", Age = -1 };
    var nextDelegateCalled = false;

    var validationFailures = new List<ValidationFailure>
        {
            new ValidationFailure("Name", "Name is required")
        };

    var mockValidator = new Mock<IValidator<TestValidationRequest>>();
    mockValidator
        .Setup(v => v.ValidateAsync(It.IsAny<ValidationContext<TestValidationRequest>>(), It.IsAny<CancellationToken>()))
        .ReturnsAsync(new ValidationResult(validationFailures));

    var validators = new[] { mockValidator.Object };
    var validationBehavior = new ValidationBehavior<TestValidationRequest, Result>(validators);

    // Act
    var result = await validationBehavior.Handle(request, _ =>
    {
      nextDelegateCalled = true;
      return Task.FromResult(Result.Success());
    }, _cancellationToken);

    // Assert
    Assert.False(result.IsSuccess);
    Assert.False(nextDelegateCalled);
  }

  [Fact]
  public async Task Handle_WhenRequestIsNull_ShouldThrowArgumentNullException()
  {
    // Arrange
    var validators = Array.Empty<IValidator<TestValidationRequest>>();
    var validationBehavior = new ValidationBehavior<TestValidationRequest, Result>(validators);

    // Act & Assert
    await Assert.ThrowsAsync<ArgumentNullException>(
        () => validationBehavior.Handle(null!, _ => Task.FromResult(Result.Success()), _cancellationToken));
  }

  [Fact]
  public async Task Handle_WhenNextDelegateIsNull_ShouldThrowArgumentNullException()
  {
    // Arrange
    var request = new TestValidationRequest();
    var validators = Array.Empty<IValidator<TestValidationRequest>>();
    var validationBehavior = new ValidationBehavior<TestValidationRequest, Result>(validators);

    // Act & Assert
    await Assert.ThrowsAsync<ArgumentNullException>(
        () => validationBehavior.Handle(request, null!, _cancellationToken));
  }

  [Fact]
  public void Constructor_WhenValidatorsIsNull_ShouldThrowArgumentNullException()
  {
    // Act & Assert
    Assert.Throws<ArgumentNullException>(() => new ValidationBehavior<TestValidationRequest, Result>(null!));
  }

  [Fact]
  public async Task Handle_WhenCancellationIsRequested_ShouldPropagateCancellation()
  {
    // Arrange
    var request = new TestValidationRequest { Name = "Test", Age = 25 };
    var cts = new CancellationTokenSource();
    cts.Cancel();

    var mockValidator = new Mock<IValidator<TestValidationRequest>>();
    mockValidator
        .Setup(v => v.ValidateAsync(It.IsAny<ValidationContext<TestValidationRequest>>(), It.IsAny<CancellationToken>()))
        .ThrowsAsync(new OperationCanceledException(cts.Token));

    var validators = new[] { mockValidator.Object };
    var validationBehavior = new ValidationBehavior<TestValidationRequest, Result>(validators);

    // Act & Assert
    await Assert.ThrowsAsync<OperationCanceledException>(
        () => validationBehavior.Handle(request, _ => Task.FromResult(Result.Success()), cts.Token));
  }

  [Fact]
  public async Task Handle_WithValidationErrorsIncludingPropertyName_ShouldMapCorrectly()
  {
    // Arrange
    var request = new TestValidationRequest { Name = "A", Age = 200 };

    var validationFailures = new List<ValidationFailure>
        {
            new ValidationFailure("Name", "Name must be at least 3 characters")
            {
                PropertyName = "Name",
                ErrorCode = "MinimumLengthValidator"
            },
            new ValidationFailure("Age", "Age must be less than 120")
            {
                PropertyName = "Age",
                ErrorCode = "LessThanValidator"
            }
        };

    var mockValidator = new Mock<IValidator<TestValidationRequest>>();
    mockValidator
        .Setup(v => v.ValidateAsync(It.IsAny<ValidationContext<TestValidationRequest>>(), It.IsAny<CancellationToken>()))
        .ReturnsAsync(new ValidationResult(validationFailures));

    var validators = new[] { mockValidator.Object };
    var validationBehavior = new ValidationBehavior<TestValidationRequest, Result>(validators);

    // Act
    var result = await validationBehavior.Handle(request, _ => Task.FromResult(Result.Success()), _cancellationToken);

    // Assert
    Assert.False(result.IsSuccess);
    Assert.Equal(2, result.ValidationErrors.ToList().Count);

    var nameError = result.ValidationErrors.FirstOrDefault(e => e.Identifier == "Name");
    Assert.NotNull(nameError);
    Assert.Equal("Name must be at least 3 characters", nameError.ErrorMessage);

    var ageError = result.ValidationErrors.FirstOrDefault(e => e.Identifier == "Age");
    Assert.NotNull(ageError);
    Assert.Equal("Age must be less than 120", ageError.ErrorMessage);
  }

  [Fact]
  public async Task Handle_WhenValidatorThrowsException_ShouldPropagateException()
  {
    // Arrange
    var request = new TestValidationRequest { Name = "Test", Age = 25 };
    var expectedException = new InvalidOperationException("Validator error");

    var mockValidator = new Mock<IValidator<TestValidationRequest>>();
    mockValidator
        .Setup(v => v.ValidateAsync(It.IsAny<ValidationContext<TestValidationRequest>>(), It.IsAny<CancellationToken>()))
        .ThrowsAsync(expectedException);

    var validators = new[] { mockValidator.Object };
    var validationBehavior = new ValidationBehavior<TestValidationRequest, Result>(validators);

    // Act & Assert
    var thrownException = await Assert.ThrowsAsync<InvalidOperationException>(
        () => validationBehavior.Handle(request, _ => Task.FromResult(Result.Success()), _cancellationToken));

    Assert.Same(expectedException, thrownException);
  }

  [Fact]
  public async Task Handle_WithRealFluentValidationValidator_ShouldWorkCorrectly()
  {
    // Arrange
    var request = new TestValidationRequest { Name = "", Age = -5 };
    var validator = new TestValidationRequestValidator();
    var validators = new IValidator<TestValidationRequest>[] { validator };
    var validationBehavior = new ValidationBehavior<TestValidationRequest, Result>(validators);

    // Act
    var result = await validationBehavior.Handle(request, _ => Task.FromResult(Result.Success()), _cancellationToken);

    // Assert
    Assert.False(result.IsSuccess);
    Assert.Equal(ResultStatus.Invalid, result.Status);
    Assert.True(result.ValidationErrors.ToList().Count > 0);
  }

  [Fact]
  public async Task Handle_WithSuccessfulValidation_ShouldReturnNextDelegateResult()
  {
    // Arrange
    var request = new TestValidationStringRequest { Name = "ValidName", Age = 30 };
    var expectedResult = Result<string>.Success("Success Value");

    var mockValidator = new Mock<IValidator<TestValidationStringRequest>>();
    mockValidator
        .Setup(v => v.ValidateAsync(It.IsAny<ValidationContext<TestValidationStringRequest>>(), It.IsAny<CancellationToken>()))
        .ReturnsAsync(new ValidationResult());

    var validators = new[] { mockValidator.Object };
    var validationBehavior = new ValidationBehavior<TestValidationStringRequest, Result<string>>(validators);

    // Act
    var result = await validationBehavior.Handle(request, _ => Task.FromResult(expectedResult), _cancellationToken);

    // Assert
    Assert.True(result.IsSuccess);
    Assert.Equal("Success Value", result.Value);
  }

  // NEW TESTS TO COVER MISSING LINES

  [Fact]
  public async Task Handle_WithGenericResultValidationFailure_ShouldReturnInvalidGenericResult()
  {
    // Arrange - This test covers lines 52-59
    var request = new TestValidationStringRequest { Name = "", Age = -1 };

    var validationFailures = new List<ValidationFailure>
        {
            new ValidationFailure("Name", "Name is required"),
            new ValidationFailure("Age", "Age must be positive")
        };

    var mockValidator = new Mock<IValidator<TestValidationStringRequest>>();
    mockValidator
        .Setup(v => v.ValidateAsync(It.IsAny<ValidationContext<TestValidationStringRequest>>(), It.IsAny<CancellationToken>()))
        .ReturnsAsync(new ValidationResult(validationFailures));

    var validators = new[] { mockValidator.Object };
    var validationBehavior = new ValidationBehavior<TestValidationStringRequest, Result<string>>(validators);

    // Act
    var result = await validationBehavior.Handle(request, _ => Task.FromResult(Result<string>.Success("Should not reach here")), _cancellationToken);

    // Assert
    Assert.False(result.IsSuccess);
    Assert.Equal(ResultStatus.Invalid, result.Status);
    Assert.Equal(2, result.ValidationErrors.ToList().Count);
    Assert.Contains(result.ValidationErrors, e => e.ErrorMessage == "Name is required");
    Assert.Contains(result.ValidationErrors, e => e.ErrorMessage == "Age must be positive");
  }

  [Fact]
  public async Task Handle_WithNonResultTypeValidationFailure_ShouldThrowValidationException()
  {
    // Arrange - This test covers line 68
    var request = new TestNonResultRequest { Name = "" };

    var validationFailures = new List<ValidationFailure>
        {
            new ValidationFailure("Name", "Name is required")
        };

    var mockValidator = new Mock<IValidator<TestNonResultRequest>>();
    mockValidator
        .Setup(v => v.ValidateAsync(It.IsAny<ValidationContext<TestNonResultRequest>>(), It.IsAny<CancellationToken>()))
        .ReturnsAsync(new ValidationResult(validationFailures));

    var validators = new[] { mockValidator.Object };
    var validationBehavior = new ValidationBehavior<TestNonResultRequest, string>(validators);

    // Act & Assert
    var exception = await Assert.ThrowsAsync<FluentValidation.ValidationException>(
        () => validationBehavior.Handle(request, _ => Task.FromResult("Success"), _cancellationToken));

    Assert.Single(exception.Errors);
    Assert.Equal("Name", exception.Errors.First().PropertyName);
    Assert.Equal("Name is required", exception.Errors.First().ErrorMessage);
  }

  [Fact]
  public async Task Handle_WithNonResultTypeValidationSuccess_ShouldReturnResponse()
  {
    // Arrange
    var request = new TestNonResultRequest { Name = "Valid Name" };
    var expectedResponse = "Success Response";

    var mockValidator = new Mock<IValidator<TestNonResultRequest>>();
    mockValidator
        .Setup(v => v.ValidateAsync(It.IsAny<ValidationContext<TestNonResultRequest>>(), It.IsAny<CancellationToken>()))
        .ReturnsAsync(new ValidationResult());

    var validators = new[] { mockValidator.Object };
    var validationBehavior = new ValidationBehavior<TestNonResultRequest, string>(validators);

    // Act
    var result = await validationBehavior.Handle(request, _ => Task.FromResult(expectedResponse), _cancellationToken);

    // Assert
    Assert.Equal(expectedResponse, result);
  }
}

// Test request class - implements IRequest<Result> instead of IBaseRequest
public class TestValidationRequest : IRequest<Result>
{
  public string Name { get; set; } = string.Empty;
  public int Age { get; set; }
}

// Test request class for Result<string> - implements IRequest<Result<string>>
public class TestValidationStringRequest : IRequest<Result<string>>
{
  public string Name { get; set; } = string.Empty;
  public int Age { get; set; }
}

// Test request class for non-Result responses
public class TestNonResultRequest : IRequest<string>
{
  public string Name { get; set; } = string.Empty;
}

// Real FluentValidation validator for testing
public class TestValidationRequestValidator : AbstractValidator<TestValidationRequest>
{
  public TestValidationRequestValidator()
  {
    RuleFor(x => x.Name)
        .NotEmpty().WithMessage("Name is required")
        .MinimumLength(3).WithMessage("Name must be at least 3 characters");

    RuleFor(x => x.Age)
        .GreaterThan(0).WithMessage("Age must be positive")
        .LessThan(120).WithMessage("Age must be less than 120");
  }
}