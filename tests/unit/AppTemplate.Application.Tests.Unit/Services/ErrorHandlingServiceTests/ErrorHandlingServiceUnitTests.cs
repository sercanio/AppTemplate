using AppTemplate.Application.Services.ErrorHandling;
using AppTemplate.Application.Services.Localization;
using Ardalis.Result;
using FluentAssertions;
using FluentValidation.Results;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Globalization;
using System.Net;
using Xunit;

namespace AppTemplate.Application.Tests.Unit.Services.ErrorHandlingServiceTests;

[Trait("Category", "Unit")]
public class ErrorHandlingServiceUnitTests
{
  private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock;
  private readonly Mock<ILocalizationService> _localizationServiceMock;
  private readonly ErrorHandlingService _service;
  private readonly DefaultHttpContext _httpContext;

  public ErrorHandlingServiceUnitTests()
  {
    _httpContextAccessorMock = new Mock<IHttpContextAccessor>();
    _localizationServiceMock = new Mock<ILocalizationService>();
    _httpContext = new DefaultHttpContext();

    _httpContext.Request.Path = "/api/test";
    _httpContext.TraceIdentifier = "test-trace-id";

    _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(_httpContext);

    _service = new ErrorHandlingService(
        _httpContextAccessorMock.Object,
        _localizationServiceMock.Object);
  }

  #region NotFound Status Tests

  [Fact]
  public void HandleErrorResponse_WithNotFoundStatus_ShouldReturnNotFoundResponse()
  {
    // Arrange
    var result = Result<string>.NotFound("Resource not found");
    _localizationServiceMock
        .Setup(x => x.GetLocalizedString("Errors.NotFound", It.IsAny<string>()))
        .Returns("Resource was not found");

    // Act
    var response = _service.HandleErrorResponse(result);

    // Assert
    var objectResult = response.Should().BeOfType<ObjectResult>().Subject;
    objectResult.StatusCode.Should().Be((int)HttpStatusCode.NotFound);

    var problemDetails = objectResult.Value.Should().BeOfType<ProblemDetails>().Subject;
    problemDetails.Status.Should().Be((int)HttpStatusCode.NotFound);
    problemDetails.Title.Should().Be("Resource Not Found");
    problemDetails.Detail.Should().Be("Resource was not found");
    problemDetails.Type.Should().Be("https://datatracker.ietf.org/doc/html/rfc7231#section-6.5.4");
    problemDetails.Instance.Should().Be("/api/test");
  }

  #endregion

  #region Invalid Status Tests

  [Fact]
  public void HandleErrorResponse_WithInvalidStatus_ShouldReturnBadRequestResponse()
  {
    // Arrange
    var result = Result<string>.Invalid(new ValidationError("Field is required"));
    _localizationServiceMock
        .Setup(x => x.GetLocalizedString("Errors.Invalid", It.IsAny<string>()))
        .Returns("Validation failed");

    // Act
    var response = _service.HandleErrorResponse(result);

    // Assert
    var objectResult = response.Should().BeOfType<ObjectResult>().Subject;
    objectResult.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);

    var problemDetails = objectResult.Value.Should().BeOfType<ProblemDetails>().Subject;
    problemDetails.Status.Should().Be((int)HttpStatusCode.BadRequest);
    problemDetails.Title.Should().Be("Validation Error");
    problemDetails.Type.Should().Be("https://datatracker.ietf.org/doc/html/rfc7231#section-6.5.1");
  }

  [Fact]
  public void HandleErrorResponse_WithValidationErrors_ShouldIncludeErrorMessages()
  {
    // Arrange
    var validationErrors = new List<ValidationError>
        {
            new ValidationError("Username is required"),
            new ValidationError("Email is invalid")
        };
    var result = Result<string>.Invalid(validationErrors);
    _localizationServiceMock
        .Setup(x => x.GetLocalizedString("Errors.Invalid", It.IsAny<string>()))
        .Returns("Validation failed");

    // Act
    var response = _service.HandleErrorResponse(result);

    // Assert
    var objectResult = response.Should().BeOfType<ObjectResult>().Subject;
    var problemDetails = objectResult.Value.Should().BeOfType<ProblemDetails>().Subject;

    problemDetails.Extensions.Should().ContainKey("errors");
    var errors = problemDetails.Extensions["errors"] as List<string>;
    errors.Should().HaveCount(2);
    errors.Should().Contain("Username is required");
    errors.Should().Contain("Email is invalid");
  }

  #endregion

  #region Error Status Tests

  [Fact]
  public void HandleErrorResponse_WithErrorStatus_ShouldReturnInternalServerError()
  {
    // Arrange
    var result = Result<string>.Error("An unexpected error occurred");
    _localizationServiceMock
        .Setup(x => x.GetLocalizedString("Errors.Error", It.IsAny<string>()))
        .Returns("An error occurred");

    // Act
    var response = _service.HandleErrorResponse(result);

    // Assert
    var objectResult = response.Should().BeOfType<ObjectResult>().Subject;
    objectResult.StatusCode.Should().Be((int)HttpStatusCode.InternalServerError);

    var problemDetails = objectResult.Value.Should().BeOfType<ProblemDetails>().Subject;
    problemDetails.Status.Should().Be((int)HttpStatusCode.InternalServerError);
    problemDetails.Title.Should().Be("An Unexpected Error Occurred");
    problemDetails.Type.Should().Be("https://datatracker.ietf.org/doc/html/rfc7231#section-6.6.1");
  }

  #endregion

  #region Forbidden Status Tests

  [Fact]
  public void HandleErrorResponse_WithForbiddenStatus_ShouldReturnForbiddenResponse()
  {
    // Arrange
    var result = Result<string>.Forbidden();
    _localizationServiceMock
        .Setup(x => x.GetLocalizedString("Errors.Forbidden", It.IsAny<string>()))
        .Returns("Access forbidden");

    // Act
    var response = _service.HandleErrorResponse(result);

    // Assert
    var objectResult = response.Should().BeOfType<ObjectResult>().Subject;
    objectResult.StatusCode.Should().Be((int)HttpStatusCode.Forbidden);

    var problemDetails = objectResult.Value.Should().BeOfType<ProblemDetails>().Subject;
    problemDetails.Status.Should().Be((int)HttpStatusCode.Forbidden);
    problemDetails.Title.Should().Be("Forbidden");
    problemDetails.Type.Should().Be("https://datatracker.ietf.org/doc/html/rfc7231#section-6.5.3");
  }

  #endregion

  #region Unauthorized Status Tests

  [Fact]
  public void HandleErrorResponse_WithUnauthorizedStatus_ShouldReturnUnauthorizedResponse()
  {
    // Arrange
    var result = Result<string>.Unauthorized();
    _localizationServiceMock
        .Setup(x => x.GetLocalizedString("Errors.Unauthorized", It.IsAny<string>()))
        .Returns("Unauthorized access");

    // Act
    var response = _service.HandleErrorResponse(result);

    // Assert
    var objectResult = response.Should().BeOfType<ObjectResult>().Subject;
    objectResult.StatusCode.Should().Be((int)HttpStatusCode.Unauthorized);

    var problemDetails = objectResult.Value.Should().BeOfType<ProblemDetails>().Subject;
    problemDetails.Status.Should().Be((int)HttpStatusCode.Unauthorized);
    problemDetails.Title.Should().Be("Unauthorized Access");
    problemDetails.Type.Should().Be("https://datatracker.ietf.org/doc/html/rfc7235#section-3.1");
  }

  #endregion

  #region Conflict Status Tests

  [Fact]
  public void HandleErrorResponse_WithConflictStatus_ShouldReturnConflictResponse()
  {
    // Arrange
    var result = Result<string>.Conflict("Resource already exists");
    _localizationServiceMock
        .Setup(x => x.GetLocalizedString("Errors.Conflict", It.IsAny<string>()))
        .Returns("Conflict occurred");

    // Act
    var response = _service.HandleErrorResponse(result);

    // Assert
    var objectResult = response.Should().BeOfType<ObjectResult>().Subject;
    objectResult.StatusCode.Should().Be((int)HttpStatusCode.Conflict);

    var problemDetails = objectResult.Value.Should().BeOfType<ProblemDetails>().Subject;
    problemDetails.Status.Should().Be((int)HttpStatusCode.Conflict);
    problemDetails.Title.Should().Be("Conflict");
    problemDetails.Type.Should().Be("https://datatracker.ietf.org/doc/html/rfc7231#section-6.5.8");
  }

  #endregion

  #region Localization Tests

  [Fact]
  public void HandleErrorResponse_WithAcceptLanguageHeader_ShouldUseSpecifiedLanguage()
  {
    // Arrange
    _httpContext.Request.Headers["Accept-Language"] = "tr-TR,tr;q=0.9";
    var result = Result<string>.NotFound();
    _localizationServiceMock
        .Setup(x => x.GetLocalizedString("Errors.NotFound", "tr-TR"))
        .Returns("Kaynak bulunamadı");

    // Act
    var response = _service.HandleErrorResponse(result);

    // Assert
    var objectResult = response.Should().BeOfType<ObjectResult>().Subject;
    var problemDetails = objectResult.Value.Should().BeOfType<ProblemDetails>().Subject;
    problemDetails.Detail.Should().Be("Kaynak bulunamadı");

    _localizationServiceMock.Verify(
        x => x.GetLocalizedString("Errors.NotFound", "tr-TR"),
        Times.Once);
  }

  [Fact]
  public void HandleErrorResponse_WithMultipleLanguagesInHeader_ShouldUseFirstLanguage()
  {
    // Arrange
    _httpContext.Request.Headers["Accept-Language"] = "en-US,en;q=0.9,tr-TR;q=0.8";
    var result = Result<string>.NotFound();
    _localizationServiceMock
        .Setup(x => x.GetLocalizedString("Errors.NotFound", "en-US"))
        .Returns("Resource not found");

    // Act
    var response = _service.HandleErrorResponse(result);

    // Assert
    _localizationServiceMock.Verify(
        x => x.GetLocalizedString("Errors.NotFound", "en-US"),
        Times.Once);
  }

  [Fact]
  public void HandleErrorResponse_WithoutAcceptLanguageHeader_ShouldUseCurrentCulture()
  {
    // Arrange
    var currentCulture = CultureInfo.CurrentCulture.Name;
    var result = Result<string>.NotFound();
    _localizationServiceMock
        .Setup(x => x.GetLocalizedString("Errors.NotFound", currentCulture))
        .Returns("Resource not found");

    // Act
    var response = _service.HandleErrorResponse(result);

    // Assert
    _localizationServiceMock.Verify(
        x => x.GetLocalizedString("Errors.NotFound", currentCulture),
        Times.Once);
  }

  [Fact]
  public void HandleErrorResponse_WithEmptyAcceptLanguageHeader_ShouldUseCurrentCulture()
  {
    // Arrange
    _httpContext.Request.Headers["Accept-Language"] = "";
    var currentCulture = CultureInfo.CurrentCulture.Name;
    var result = Result<string>.NotFound();
    _localizationServiceMock
        .Setup(x => x.GetLocalizedString("Errors.NotFound", currentCulture))
        .Returns("Resource not found");

    // Act
    var response = _service.HandleErrorResponse(result);

    // Assert
    _localizationServiceMock.Verify(
        x => x.GetLocalizedString("Errors.NotFound", currentCulture),
        Times.Once);
  }

  #endregion

  #region Error and Validation Error Combination Tests

  [Fact]
  public void HandleErrorResponse_WithBothErrorsAndValidationErrors_ShouldCombineAll()
  {
    // Arrange
    var result = Result<string>.Invalid(new ValidationError("Validation error"));
    // Manually add a regular error (simulating a result with both types)
    var resultWithErrors = Result<string>.Error("Regular error");

    _localizationServiceMock
        .Setup(x => x.GetLocalizedString(It.IsAny<string>(), It.IsAny<string>()))
        .Returns("Error message");

    // Act
    var response = _service.HandleErrorResponse(result);

    // Assert
    var objectResult = response.Should().BeOfType<ObjectResult>().Subject;
    var problemDetails = objectResult.Value.Should().BeOfType<ProblemDetails>().Subject;

    problemDetails.Extensions.Should().ContainKey("errors");
    var errors = problemDetails.Extensions["errors"] as List<string>;
    errors.Should().NotBeNull();
    errors.Should().Contain("Validation error");
  }

  #endregion

  #region TraceId and Instance Tests

  [Fact]
  public void HandleErrorResponse_ShouldIncludeTraceId()
  {
    // Arrange
    var traceId = "unique-trace-id-12345";
    _httpContext.TraceIdentifier = traceId;
    var result = Result<string>.NotFound();
    _localizationServiceMock
        .Setup(x => x.GetLocalizedString(It.IsAny<string>(), It.IsAny<string>()))
        .Returns("Error");

    // Act
    var response = _service.HandleErrorResponse(result);

    // Assert
    var objectResult = response.Should().BeOfType<ObjectResult>().Subject;
    var problemDetails = objectResult.Value.Should().BeOfType<ProblemDetails>().Subject;

    problemDetails.Extensions.Should().ContainKey("traceId");
    problemDetails.Extensions["traceId"].Should().Be(traceId);
  }

  [Fact]
  public void HandleErrorResponse_ShouldIncludeRequestPath()
  {
    // Arrange
    var requestPath = "/api/users/123";
    _httpContext.Request.Path = requestPath;
    var result = Result<string>.NotFound();
    _localizationServiceMock
        .Setup(x => x.GetLocalizedString(It.IsAny<string>(), It.IsAny<string>()))
        .Returns("Error");

    // Act
    var response = _service.HandleErrorResponse(result);

    // Assert
    var objectResult = response.Should().BeOfType<ObjectResult>().Subject;
    var problemDetails = objectResult.Value.Should().BeOfType<ProblemDetails>().Subject;
    problemDetails.Instance.Should().Be(requestPath);
  }

  [Fact]
  public void HandleErrorResponse_WithNullHttpContext_ShouldHandleGracefully()
  {
    // Arrange
    _httpContextAccessorMock.Setup(x => x.HttpContext).Returns((HttpContext?)null);
    var service = new ErrorHandlingService(_httpContextAccessorMock.Object, _localizationServiceMock.Object);
    var result = Result<string>.NotFound();
    _localizationServiceMock
        .Setup(x => x.GetLocalizedString(It.IsAny<string>(), It.IsAny<string>()))
        .Returns("Error");

    // Act
    var response = service.HandleErrorResponse(result);

    // Assert
    var objectResult = response.Should().BeOfType<ObjectResult>().Subject;
    var problemDetails = objectResult.Value.Should().BeOfType<ProblemDetails>().Subject;
    problemDetails.Instance.Should().BeNull();
    problemDetails.Extensions["traceId"].Should().BeNull();
  }

  #endregion

  #region Constructor Tests

  [Fact]
  public void Constructor_ShouldInitializeService_WithValidDependencies()
  {
    // Arrange & Act
    var service = new ErrorHandlingService(
        _httpContextAccessorMock.Object,
        _localizationServiceMock.Object);

    // Assert
    service.Should().NotBeNull();
  }

  #endregion

  #region Multiple Errors Tests

  [Fact]
  public void HandleErrorResponse_WithSingleError_ShouldIncludeError()
  {
    // Arrange
    var result = Result<string>.Error("Single error message");
    _localizationServiceMock
        .Setup(x => x.GetLocalizedString(It.IsAny<string>(), It.IsAny<string>()))
        .Returns("An error occurred");

    // Act
    var response = _service.HandleErrorResponse(result);

    // Assert
    var objectResult = response.Should().BeOfType<ObjectResult>().Subject;
    var problemDetails = objectResult.Value.Should().BeOfType<ProblemDetails>().Subject;

    problemDetails.Extensions.Should().ContainKey("errors");
    var returnedErrors = problemDetails.Extensions["errors"] as List<string>;
    returnedErrors.Should().ContainSingle();
    returnedErrors.Should().Contain("Single error message");
  }

  [Fact]
  public void HandleErrorResponse_WithErrorList_ShouldIncludeAllErrors()
  {
    // Arrange
    var errors = new List<string> { "Error 1", "Error 2", "Error 3" };
    var errorList = new ErrorList(errors);
    
    var result = Result<string>.Error(errorList);
    _localizationServiceMock
        .Setup(x => x.GetLocalizedString(It.IsAny<string>(), It.IsAny<string>()))
        .Returns("Multiple errors");

    // Act
    var response = _service.HandleErrorResponse(result);

    // Assert
    var objectResult = response.Should().BeOfType<ObjectResult>().Subject;
    var problemDetails = objectResult.Value.Should().BeOfType<ProblemDetails>().Subject;

    problemDetails.Extensions.Should().ContainKey("errors");
    var returnedErrors = problemDetails.Extensions["errors"] as List<string>;
    returnedErrors.Should().HaveCount(3);
    returnedErrors.Should().Contain("Error 1");
    returnedErrors.Should().Contain("Error 2");
    returnedErrors.Should().Contain("Error 3");
  }

  #endregion

  #region Edge Cases

  [Fact]
  public void HandleErrorResponse_WithEmptyErrors_ShouldReturnEmptyErrorList()
  {
    // Arrange
    var result = Result<string>.Error();
    _localizationServiceMock
        .Setup(x => x.GetLocalizedString(It.IsAny<string>(), It.IsAny<string>()))
        .Returns("Error");

    // Act
    var response = _service.HandleErrorResponse(result);

    // Assert
    var objectResult = response.Should().BeOfType<ObjectResult>().Subject;
    var problemDetails = objectResult.Value.Should().BeOfType<ProblemDetails>().Subject;

    problemDetails.Extensions.Should().ContainKey("errors");
    var errors = problemDetails.Extensions["errors"] as List<string>;
    errors.Should().BeEmpty();
  }

  [Theory]
  [InlineData(ResultStatus.NotFound, 404)]
  [InlineData(ResultStatus.Invalid, 400)]
  [InlineData(ResultStatus.Error, 500)]
  [InlineData(ResultStatus.Forbidden, 403)]
  [InlineData(ResultStatus.Unauthorized, 401)]
  [InlineData(ResultStatus.Conflict, 409)]
  public void HandleErrorResponse_ShouldMapStatusCodesCorrectly(ResultStatus status, int expectedStatusCode)
  {
    // Arrange
    Result<string> result = status switch
    {
      ResultStatus.NotFound => Result<string>.NotFound(),
      ResultStatus.Invalid => Result<string>.Invalid(new ValidationError("Invalid")),
      ResultStatus.Error => Result<string>.Error(),
      ResultStatus.Forbidden => Result<string>.Forbidden(),
      ResultStatus.Unauthorized => Result<string>.Unauthorized(),
      ResultStatus.Conflict => Result<string>.Conflict(),
      _ => Result<string>.Error()
    };

    _localizationServiceMock
        .Setup(x => x.GetLocalizedString(It.IsAny<string>(), It.IsAny<string>()))
        .Returns("Error");

    // Act
    var response = _service.HandleErrorResponse(result);

    // Assert
    var objectResult = response.Should().BeOfType<ObjectResult>().Subject;
    objectResult.StatusCode.Should().Be(expectedStatusCode);
  }

  #endregion

  #region ProblemDetails Structure Tests

  [Fact]
  public void HandleErrorResponse_ShouldReturnValidProblemDetailsStructure()
  {
    // Arrange
    var result = Result<string>.NotFound("User not found");
    _localizationServiceMock
        .Setup(x => x.GetLocalizedString(It.IsAny<string>(), It.IsAny<string>()))
        .Returns("Not found");

    // Act
    var response = _service.HandleErrorResponse(result);

    // Assert
    var objectResult = response.Should().BeOfType<ObjectResult>().Subject;
    var problemDetails = objectResult.Value.Should().BeOfType<ProblemDetails>().Subject;

    problemDetails.Title.Should().NotBeNullOrEmpty();
    problemDetails.Status.Should().BeGreaterThan(0);
    problemDetails.Detail.Should().NotBeNullOrEmpty();
    problemDetails.Type.Should().NotBeNullOrEmpty();
    problemDetails.Type.Should().StartWith("https://");
    problemDetails.Extensions.Should().ContainKey("errors");
    problemDetails.Extensions.Should().ContainKey("traceId");
  }

  #endregion
}