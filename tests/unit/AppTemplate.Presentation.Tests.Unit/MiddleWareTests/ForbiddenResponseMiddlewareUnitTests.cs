using AppTemplate.Application.Services.Localization;
using AppTemplate.Presentation.Middlewares;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using System.Globalization;
using System.Net;
using System.Text.Json;

namespace AppTemplate.Presentation.Tests.Unit.MiddleWareTests;

[Trait("Category", "Unit")]
public class ForbiddenResponseMiddlewareUnitTests
{
  private readonly Mock<RequestDelegate> _mockNext;
  private readonly Mock<ILogger<ForbiddenResponseMiddleware>> _mockLogger;
  private readonly Mock<ILocalizationService> _mockLocalizationService;
  private readonly ForbiddenResponseMiddleware _middleware;

  public ForbiddenResponseMiddlewareUnitTests()
  {
    _mockNext = new Mock<RequestDelegate>();
    _mockLogger = new Mock<ILogger<ForbiddenResponseMiddleware>>();
    _mockLocalizationService = new Mock<ILocalizationService>();
    _middleware = new ForbiddenResponseMiddleware(
        _mockNext.Object,
        _mockLogger.Object,
        _mockLocalizationService.Object);
  }

  [Fact]
  public async Task InvokeAsync_WhenStatusCodeIsNotForbidden_ShouldCallNextDelegateOnly()
  {
    // Arrange
    var context = CreateHttpContext();
    context.Response.StatusCode = 200;
    _mockNext.Setup(x => x(It.IsAny<HttpContext>())).Returns(Task.CompletedTask);

    // Act
    await _middleware.InvokeAsync(context);

    // Assert
    _mockNext.Verify(x => x(context), Times.Once);
    Assert.Equal(200, context.Response.StatusCode);
    _mockLogger.Verify(x => x.Log(
        It.IsAny<LogLevel>(),
        It.IsAny<EventId>(),
        It.IsAny<It.IsAnyType>(),
        It.IsAny<Exception>(),
        It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Never);
  }

  [Fact]
  public async Task InvokeAsync_WhenStatusCodeIsForbidden_ShouldReturnProblemDetailsResponse()
  {
    // Arrange
    var context = CreateHttpContext();
    context.Response.StatusCode = (int)HttpStatusCode.Forbidden;
    context.Request.Path = "/api/test";
    var localizedMessage = "Access is forbidden";

    _mockNext.Setup(x => x(It.IsAny<HttpContext>())).Returns(Task.CompletedTask);
    _mockLocalizationService.Setup(x => x.GetLocalizedString("Errors.Forbidden", It.IsAny<string>()))
        .Returns(localizedMessage);

    // Act
    await _middleware.InvokeAsync(context);

    // Assert
    Assert.Equal((int)HttpStatusCode.Forbidden, context.Response.StatusCode);
    Assert.Equal("application/json; charset=utf-8", context.Response.ContentType);

    // Verify the response content
    context.Response.Body.Seek(0, SeekOrigin.Begin);
    var responseContent = await new StreamReader(context.Response.Body).ReadToEndAsync();
    var problemDetails = JsonSerializer.Deserialize<ProblemDetails>(responseContent, new JsonSerializerOptions
    {
      PropertyNameCaseInsensitive = true
    });

    Assert.NotNull(problemDetails);
    Assert.Equal(403, problemDetails.Status);
    Assert.Equal("https://datatracker.ietf.org/doc/html/rfc7231#section-6.5.3", problemDetails.Type);
    Assert.Equal("Forbidden", problemDetails.Title);
    Assert.Equal(localizedMessage, problemDetails.Detail);
    Assert.Equal("/api/test", problemDetails.Instance);
  }

  [Fact]
  public async Task InvokeAsync_WhenStatusCodeIsForbidden_ShouldLogWarningMessage()
  {
    // Arrange
    var context = CreateHttpContext();
    context.Response.StatusCode = (int)HttpStatusCode.Forbidden;
    context.Request.Path = "/api/secure";

    _mockNext.Setup(x => x(It.IsAny<HttpContext>())).Returns(Task.CompletedTask);
    _mockLocalizationService.Setup(x => x.GetLocalizedString("Errors.Forbidden", It.IsAny<string>()))
        .Returns("Forbidden");

    // Act
    await _middleware.InvokeAsync(context);

    // Assert
    _mockLogger.Verify(x => x.Log(
        LogLevel.Warning,
        It.IsAny<EventId>(),
        It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Forbidden request: /api/secure")),
        It.IsAny<Exception>(),
        It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Once);
  }

  [Fact]
  public async Task InvokeAsync_WithAcceptLanguageHeader_ShouldUseCorrectLanguage()
  {
    // Arrange
    var context = CreateHttpContext();
    context.Response.StatusCode = (int)HttpStatusCode.Forbidden;
    context.Request.Headers["Accept-Language"] = "fr-FR,en-US;q=0.8";
    var expectedLanguage = "fr-FR";

    _mockNext.Setup(x => x(It.IsAny<HttpContext>())).Returns(Task.CompletedTask);
    _mockLocalizationService.Setup(x => x.GetLocalizedString("Errors.Forbidden", expectedLanguage))
        .Returns("Accès interdit");

    // Act
    await _middleware.InvokeAsync(context);

    // Assert
    _mockLocalizationService.Verify(x => x.GetLocalizedString("Errors.Forbidden", expectedLanguage), Times.Once);
  }

  [Fact]
  public async Task InvokeAsync_WithoutAcceptLanguageHeader_ShouldUseCurrentCulture()
  {
    // Arrange
    var context = CreateHttpContext();
    context.Response.StatusCode = (int)HttpStatusCode.Forbidden;
    var currentCulture = CultureInfo.CurrentCulture.Name;

    _mockNext.Setup(x => x(It.IsAny<HttpContext>())).Returns(Task.CompletedTask);
    _mockLocalizationService.Setup(x => x.GetLocalizedString("Errors.Forbidden", currentCulture))
        .Returns("Forbidden");

    // Act
    await _middleware.InvokeAsync(context);

    // Assert
    _mockLocalizationService.Verify(x => x.GetLocalizedString("Errors.Forbidden", currentCulture), Times.Once);
  }

  [Fact]
  public async Task InvokeAsync_WithEmptyAcceptLanguageHeader_ShouldUseCurrentCulture()
  {
    // Arrange
    var context = CreateHttpContext();
    context.Response.StatusCode = (int)HttpStatusCode.Forbidden;
    context.Request.Headers["Accept-Language"] = string.Empty;
    var currentCulture = CultureInfo.CurrentCulture.Name;

    _mockNext.Setup(x => x(It.IsAny<HttpContext>())).Returns(Task.CompletedTask);
    _mockLocalizationService.Setup(x => x.GetLocalizedString("Errors.Forbidden", currentCulture))
        .Returns("Forbidden");

    // Act
    await _middleware.InvokeAsync(context);

    // Assert
    _mockLocalizationService.Verify(x => x.GetLocalizedString("Errors.Forbidden", currentCulture), Times.Once);
  }

  [Fact]
  public async Task InvokeAsync_WithMultipleLanguagesInHeader_ShouldUseFirstLanguage()
  {
    // Arrange
    var context = CreateHttpContext();
    context.Response.StatusCode = (int)HttpStatusCode.Forbidden;
    context.Request.Headers["Accept-Language"] = "de-DE,fr-FR;q=0.9,en-US;q=0.8";
    var expectedLanguage = "de-DE";

    _mockNext.Setup(x => x(It.IsAny<HttpContext>())).Returns(Task.CompletedTask);
    _mockLocalizationService.Setup(x => x.GetLocalizedString("Errors.Forbidden", expectedLanguage))
        .Returns("Zugriff verweigert");

    // Act
    await _middleware.InvokeAsync(context);

    // Assert
    _mockLocalizationService.Verify(x => x.GetLocalizedString("Errors.Forbidden", expectedLanguage), Times.Once);
  }

  [Theory]
  [InlineData(200)]
  [InlineData(400)]
  [InlineData(401)]
  [InlineData(404)]
  [InlineData(500)]
  public async Task InvokeAsync_WithNonForbiddenStatusCodes_ShouldNotModifyResponse(int statusCode)
  {
    // Arrange
    var context = CreateHttpContext();
    context.Response.StatusCode = statusCode;

    _mockNext.Setup(x => x(It.IsAny<HttpContext>())).Returns(Task.CompletedTask);

    // Act
    await _middleware.InvokeAsync(context);

    // Assert
    Assert.Equal(statusCode, context.Response.StatusCode);
    _mockLocalizationService.Verify(x => x.GetLocalizedString(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    _mockLogger.Verify(x => x.Log(
        It.IsAny<LogLevel>(),
        It.IsAny<EventId>(),
        It.IsAny<It.IsAnyType>(),
        It.IsAny<Exception>(),
        It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Never);
  }

  [Fact]
  public async Task InvokeAsync_ShouldWriteCorrectJsonFormat()
  {
    // Arrange
    var context = CreateHttpContext();
    context.Response.StatusCode = (int)HttpStatusCode.Forbidden;
    context.Request.Path = "/api/test";

    _mockNext.Setup(x => x(It.IsAny<HttpContext>())).Returns(Task.CompletedTask);
    _mockLocalizationService.Setup(x => x.GetLocalizedString("Errors.Forbidden", It.IsAny<string>()))
        .Returns("Test forbidden message");

    // Act
    await _middleware.InvokeAsync(context);

    // Assert
    context.Response.Body.Seek(0, SeekOrigin.Begin);
    var responseContent = await new StreamReader(context.Response.Body).ReadToEndAsync();

    // Verify JSON structure
    var jsonDocument = JsonDocument.Parse(responseContent);
    var root = jsonDocument.RootElement;

    Assert.True(root.TryGetProperty("status", out var statusElement));
    Assert.Equal(403, statusElement.GetInt32());

    Assert.True(root.TryGetProperty("type", out var typeElement));
    Assert.Equal("https://datatracker.ietf.org/doc/html/rfc7231#section-6.5.3", typeElement.GetString());

    Assert.True(root.TryGetProperty("title", out var titleElement));
    Assert.Equal("Forbidden", titleElement.GetString());

    Assert.True(root.TryGetProperty("detail", out var detailElement));
    Assert.Equal("Test forbidden message", detailElement.GetString());

    Assert.True(root.TryGetProperty("instance", out var instanceElement));
    Assert.Equal("/api/test", instanceElement.GetString());
  }

  [Fact]
  public async Task InvokeAsync_ShouldSetCorrectContentTypeAndEncoding()
  {
    // Arrange
    var context = CreateHttpContext();
    context.Response.StatusCode = (int)HttpStatusCode.Forbidden;

    _mockNext.Setup(x => x(It.IsAny<HttpContext>())).Returns(Task.CompletedTask);
    _mockLocalizationService.Setup(x => x.GetLocalizedString("Errors.Forbidden", It.IsAny<string>()))
        .Returns("Forbidden");

    // Act
    await _middleware.InvokeAsync(context);

    // Assert
    Assert.Equal("application/json; charset=utf-8", context.Response.ContentType);
  }

  [Fact]
  public async Task InvokeAsync_WithComplexPath_ShouldIncludeCorrectInstanceInResponse()
  {
    // Arrange
    var context = CreateHttpContext();
    context.Response.StatusCode = (int)HttpStatusCode.Forbidden;
    context.Request.Path = "/api/v1/users/123/admin";

    _mockNext.Setup(x => x(It.IsAny<HttpContext>())).Returns(Task.CompletedTask);
    _mockLocalizationService.Setup(x => x.GetLocalizedString("Errors.Forbidden", It.IsAny<string>()))
        .Returns("Access denied");

    // Act
    await _middleware.InvokeAsync(context);

    // Assert
    context.Response.Body.Seek(0, SeekOrigin.Begin);
    var responseContent = await new StreamReader(context.Response.Body).ReadToEndAsync();
    var problemDetails = JsonSerializer.Deserialize<ProblemDetails>(responseContent, new JsonSerializerOptions
    {
      PropertyNameCaseInsensitive = true
    });

    Assert.NotNull(problemDetails);
    Assert.Equal("/api/v1/users/123/admin", problemDetails.Instance);
  }

  [Fact]
  public void Constructor_ShouldInitializeFieldsCorrectly()
  {
    // Arrange & Act
    var middleware = new ForbiddenResponseMiddleware(
        _mockNext.Object,
        _mockLogger.Object,
        _mockLocalizationService.Object);

    // Assert - If constructor doesn't throw, fields are initialized correctly
    Assert.NotNull(middleware);
  }

  [Fact]
  public async Task InvokeAsync_ShouldCallNextDelegateFirst()
  {
    // Arrange
    var context = CreateHttpContext();
    var nextCalled = false;

    _mockNext.Setup(x => x(It.IsAny<HttpContext>()))
        .Callback(() =>
        {
          nextCalled = true;
          context.Response.StatusCode = (int)HttpStatusCode.Forbidden;
        })
        .Returns(Task.CompletedTask);

    _mockLocalizationService.Setup(x => x.GetLocalizedString("Errors.Forbidden", It.IsAny<string>()))
        .Returns("Forbidden");

    // Act
    await _middleware.InvokeAsync(context);

    // Assert
    Assert.True(nextCalled);
    _mockNext.Verify(x => x(context), Times.Once);
  }

  private static DefaultHttpContext CreateHttpContext()
  {
    var context = new DefaultHttpContext();
    context.Response.Body = new MemoryStream();
    return context;
  }
}
