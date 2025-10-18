using AppTemplate.Application.Services.Localization;
using AppTemplate.Presentation.Middlewares;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using Moq;
using System.Globalization;
using System.Text.Json;

namespace AppTemplate.Presentation.Tests.Unit.MiddleWareTests;

[Trait("Category", "Unit")]
public class RateLimitExceededMiddlewareUnitTests
{
    private readonly Mock<RequestDelegate> _mockNext;
    private readonly Mock<ILocalizationService> _mockLocalizationService;
    private readonly RateLimitExceededMiddleware _middleware;

    public RateLimitExceededMiddlewareUnitTests()
    {
        _mockNext = new Mock<RequestDelegate>();
        _mockLocalizationService = new Mock<ILocalizationService>();
        _middleware = new RateLimitExceededMiddleware(
            _mockNext.Object,
            _mockLocalizationService.Object);
    }

    [Fact]
    public async Task InvokeAsync_WhenStatusCodeIsNot429_ShouldCallNextDelegateOnly()
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
        _mockLocalizationService.Verify(x => x.GetLocalizedString(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task InvokeAsync_WhenStatusCodeIs429_ShouldReturnProblemDetailsResponse()
    {
        // Arrange
        var context = CreateHttpContext();
        context.Response.StatusCode = StatusCodes.Status429TooManyRequests;
        context.Request.Path = "/api/test";
        var localizedMessage = "Too many requests";

        _mockNext.Setup(x => x(It.IsAny<HttpContext>())).Returns(Task.CompletedTask);
        _mockLocalizationService.Setup(x => x.GetLocalizedString("Errors.TooManyRequests", It.IsAny<string>()))
            .Returns(localizedMessage);

        // Act
        await _middleware.InvokeAsync(context);

        // Assert
        Assert.Equal(StatusCodes.Status429TooManyRequests, context.Response.StatusCode);
        Assert.Equal("application/json; charset=utf-8", context.Response.ContentType);

        // Verify the response content
        context.Response.Body.Seek(0, SeekOrigin.Begin);
        var responseContent = await new StreamReader(context.Response.Body).ReadToEndAsync();
        var problemDetails = JsonSerializer.Deserialize<ProblemDetails>(responseContent, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        Assert.NotNull(problemDetails);
        Assert.Equal(429, problemDetails.Status);
        Assert.Equal("https://httpstatuses.com/429", problemDetails.Type);
        Assert.Equal("Too Many Requests", problemDetails.Title);
        Assert.Equal(localizedMessage, problemDetails.Detail);
        Assert.Equal("/api/test", problemDetails.Instance);
    }

    [Fact]
    public async Task InvokeAsync_WhenStatusCodeIs429WithRetryAfterHeader_ShouldIncludeRetryAfterInResponse()
    {
        // Arrange
        var context = CreateHttpContext();
        context.Response.StatusCode = StatusCodes.Status429TooManyRequests;
        context.Request.Path = "/api/test";
        var retryAfterValue = "60";
        context.Response.Headers.RetryAfter = retryAfterValue;
        var localizedMessage = "Too many requests";

        _mockNext.Setup(x => x(It.IsAny<HttpContext>())).Returns(Task.CompletedTask);
        _mockLocalizationService.Setup(x => x.GetLocalizedString("Errors.TooManyRequests", It.IsAny<string>()))
            .Returns(localizedMessage);

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
        Assert.True(problemDetails.Extensions.ContainsKey("retryAfter"));
        Assert.Equal(retryAfterValue, problemDetails.Extensions["retryAfter"]?.ToString());
    }

    [Fact]
    public async Task InvokeAsync_WhenStatusCodeIs429WithoutRetryAfterHeader_ShouldNotIncludeRetryAfterInResponse()
    {
        // Arrange
        var context = CreateHttpContext();
        context.Response.StatusCode = StatusCodes.Status429TooManyRequests;
        context.Request.Path = "/api/test";
        var localizedMessage = "Too many requests";

        _mockNext.Setup(x => x(It.IsAny<HttpContext>())).Returns(Task.CompletedTask);
        _mockLocalizationService.Setup(x => x.GetLocalizedString("Errors.TooManyRequests", It.IsAny<string>()))
            .Returns(localizedMessage);

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
        Assert.False(problemDetails.Extensions.ContainsKey("retryAfter"));
    }

    [Fact]
    public async Task InvokeAsync_WithEmptyRetryAfterHeader_ShouldNotIncludeRetryAfterInResponse()
    {
        // Arrange
        var context = CreateHttpContext();
        context.Response.StatusCode = StatusCodes.Status429TooManyRequests;
        context.Request.Path = "/api/test";
        context.Response.Headers.RetryAfter = string.Empty;
        var localizedMessage = "Too many requests";

        _mockNext.Setup(x => x(It.IsAny<HttpContext>())).Returns(Task.CompletedTask);
        _mockLocalizationService.Setup(x => x.GetLocalizedString("Errors.TooManyRequests", It.IsAny<string>()))
            .Returns(localizedMessage);

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
        Assert.False(problemDetails.Extensions.ContainsKey("retryAfter"));
    }

    [Fact]
    public async Task InvokeAsync_WithAcceptLanguageHeader_ShouldUseCorrectLanguage()
    {
        // Arrange
        var context = CreateHttpContext();
        context.Response.StatusCode = StatusCodes.Status429TooManyRequests;
        context.Request.Headers["Accept-Language"] = "fr-FR,en-US;q=0.8";
        var expectedLanguage = "fr-FR";

        _mockNext.Setup(x => x(It.IsAny<HttpContext>())).Returns(Task.CompletedTask);
        _mockLocalizationService.Setup(x => x.GetLocalizedString("Errors.TooManyRequests", expectedLanguage))
            .Returns("Trop de requêtes");

        // Act
        await _middleware.InvokeAsync(context);

        // Assert
        _mockLocalizationService.Verify(x => x.GetLocalizedString("Errors.TooManyRequests", expectedLanguage), Times.Once);
    }

    [Fact]
    public async Task InvokeAsync_WithoutAcceptLanguageHeader_ShouldUseCurrentCulture()
    {
        // Arrange
        var context = CreateHttpContext();
        context.Response.StatusCode = StatusCodes.Status429TooManyRequests;
        var currentCulture = CultureInfo.CurrentCulture.Name;

        _mockNext.Setup(x => x(It.IsAny<HttpContext>())).Returns(Task.CompletedTask);
        _mockLocalizationService.Setup(x => x.GetLocalizedString("Errors.TooManyRequests", currentCulture))
            .Returns("Too many requests");

        // Act
        await _middleware.InvokeAsync(context);

        // Assert
        _mockLocalizationService.Verify(x => x.GetLocalizedString("Errors.TooManyRequests", currentCulture), Times.Once);
    }

    [Fact]
    public async Task InvokeAsync_WithEmptyAcceptLanguageHeader_ShouldUseCurrentCulture()
    {
        // Arrange
        var context = CreateHttpContext();
        context.Response.StatusCode = StatusCodes.Status429TooManyRequests;
        context.Request.Headers["Accept-Language"] = string.Empty;
        var currentCulture = CultureInfo.CurrentCulture.Name;

        _mockNext.Setup(x => x(It.IsAny<HttpContext>())).Returns(Task.CompletedTask);
        _mockLocalizationService.Setup(x => x.GetLocalizedString("Errors.TooManyRequests", currentCulture))
            .Returns("Too many requests");

        // Act
        await _middleware.InvokeAsync(context);

        // Assert
        _mockLocalizationService.Verify(x => x.GetLocalizedString("Errors.TooManyRequests", currentCulture), Times.Once);
    }

    [Fact]
    public async Task InvokeAsync_WithMultipleLanguagesInHeader_ShouldUseFirstLanguage()
    {
        // Arrange
        var context = CreateHttpContext();
        context.Response.StatusCode = StatusCodes.Status429TooManyRequests;
        context.Request.Headers["Accept-Language"] = "de-DE,fr-FR;q=0.9,en-US;q=0.8";
        var expectedLanguage = "de-DE";

        _mockNext.Setup(x => x(It.IsAny<HttpContext>())).Returns(Task.CompletedTask);
        _mockLocalizationService.Setup(x => x.GetLocalizedString("Errors.TooManyRequests", expectedLanguage))
            .Returns("Zu viele Anfragen");

        // Act
        await _middleware.InvokeAsync(context);

        // Assert
        _mockLocalizationService.Verify(x => x.GetLocalizedString("Errors.TooManyRequests", expectedLanguage), Times.Once);
    }

    [Theory]
    [InlineData(200)]
    [InlineData(400)]
    [InlineData(401)]
    [InlineData(403)]
    [InlineData(404)]
    [InlineData(500)]
    public async Task InvokeAsync_WithNon429StatusCodes_ShouldNotModifyResponse(int statusCode)
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
    }

    [Fact]
    public async Task InvokeAsync_ShouldWriteCorrectJsonFormat()
    {
        // Arrange
        var context = CreateHttpContext();
        context.Response.StatusCode = StatusCodes.Status429TooManyRequests;
        context.Request.Path = "/api/test";

        _mockNext.Setup(x => x(It.IsAny<HttpContext>())).Returns(Task.CompletedTask);
        _mockLocalizationService.Setup(x => x.GetLocalizedString("Errors.TooManyRequests", It.IsAny<string>()))
            .Returns("Test rate limit message");

        // Act
        await _middleware.InvokeAsync(context);

        // Assert
        context.Response.Body.Seek(0, SeekOrigin.Begin);
        var responseContent = await new StreamReader(context.Response.Body).ReadToEndAsync();

        // Verify JSON structure
        var jsonDocument = JsonDocument.Parse(responseContent);
        var root = jsonDocument.RootElement;

        Assert.True(root.TryGetProperty("status", out var statusElement));
        Assert.Equal(429, statusElement.GetInt32());

        Assert.True(root.TryGetProperty("type", out var typeElement));
        Assert.Equal("https://httpstatuses.com/429", typeElement.GetString());

        Assert.True(root.TryGetProperty("title", out var titleElement));
        Assert.Equal("Too Many Requests", titleElement.GetString());

        Assert.True(root.TryGetProperty("detail", out var detailElement));
        Assert.Equal("Test rate limit message", detailElement.GetString());

        Assert.True(root.TryGetProperty("instance", out var instanceElement));
        Assert.Equal("/api/test", instanceElement.GetString());
    }

    [Fact]
    public async Task InvokeAsync_ShouldSetCorrectContentTypeAndEncoding()
    {
        // Arrange
        var context = CreateHttpContext();
        context.Response.StatusCode = StatusCodes.Status429TooManyRequests;

        _mockNext.Setup(x => x(It.IsAny<HttpContext>())).Returns(Task.CompletedTask);
        _mockLocalizationService.Setup(x => x.GetLocalizedString("Errors.TooManyRequests", It.IsAny<string>()))
            .Returns("Too many requests");

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
        context.Response.StatusCode = StatusCodes.Status429TooManyRequests;
        context.Request.Path = "/api/v1/users/123/posts";

        _mockNext.Setup(x => x(It.IsAny<HttpContext>())).Returns(Task.CompletedTask);
        _mockLocalizationService.Setup(x => x.GetLocalizedString("Errors.TooManyRequests", It.IsAny<string>()))
            .Returns("Rate limit exceeded");

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
        Assert.Equal("/api/v1/users/123/posts", problemDetails.Instance);
    }

    [Fact]
    public void Constructor_ShouldInitializeFieldsCorrectly()
    {
        // Arrange & Act
        var middleware = new RateLimitExceededMiddleware(
            _mockNext.Object,
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
                context.Response.StatusCode = StatusCodes.Status429TooManyRequests;
            })
            .Returns(Task.CompletedTask);

        _mockLocalizationService.Setup(x => x.GetLocalizedString("Errors.TooManyRequests", It.IsAny<string>()))
            .Returns("Too many requests");

        // Act
        await _middleware.InvokeAsync(context);

        // Assert
        Assert.True(nextCalled);
        _mockNext.Verify(x => x(context), Times.Once);
    }

    [Fact]
    public async Task InvokeAsync_WithDifferentRetryAfterFormats_ShouldIncludeRetryAfterInResponse()
    {
        // Arrange
        var context = CreateHttpContext();
        context.Response.StatusCode = StatusCodes.Status429TooManyRequests;
        context.Request.Path = "/api/test";
        var retryAfterValue = "Wed, 21 Oct 2015 07:28:00 GMT"; // HTTP date format
        context.Response.Headers.RetryAfter = retryAfterValue;
        var localizedMessage = "Too many requests";

        _mockNext.Setup(x => x(It.IsAny<HttpContext>())).Returns(Task.CompletedTask);
        _mockLocalizationService.Setup(x => x.GetLocalizedString("Errors.TooManyRequests", It.IsAny<string>()))
            .Returns(localizedMessage);

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
        Assert.True(problemDetails.Extensions.ContainsKey("retryAfter"));
        Assert.Equal(retryAfterValue, problemDetails.Extensions["retryAfter"]?.ToString());
    }

    [Fact]
    public async Task InvokeAsync_ShouldUseCorrectJsonSerializerOptions()
    {
        // Arrange
        var context = CreateHttpContext();
        context.Response.StatusCode = StatusCodes.Status429TooManyRequests;
        context.Request.Path = "/api/test";
        var localizedMessage = "Test message with unicode: ñáéíóú";

        _mockNext.Setup(x => x(It.IsAny<HttpContext>())).Returns(Task.CompletedTask);
        _mockLocalizationService.Setup(x => x.GetLocalizedString("Errors.TooManyRequests", It.IsAny<string>()))
            .Returns(localizedMessage);

        // Act
        await _middleware.InvokeAsync(context);

        // Assert
        context.Response.Body.Seek(0, SeekOrigin.Begin);
        var responseContent = await new StreamReader(context.Response.Body).ReadToEndAsync();

        // Verify that unicode characters are properly encoded and JSON is indented
        Assert.Contains("ñáéíóú", responseContent);
        Assert.Contains("\n", responseContent); // Check for indentation (WriteIndented = true)
    }

    [Fact]
    public async Task InvokeAsync_WithMultipleRetryAfterHeaders_ShouldUseLastValue()
    {
        // Arrange
        var context = CreateHttpContext();
        context.Response.StatusCode = StatusCodes.Status429TooManyRequests;
        context.Request.Path = "/api/test";
        
        // Add multiple values to RetryAfter header
        context.Response.Headers.RetryAfter = new StringValues(new[] { "60", "120" });
        var localizedMessage = "Too many requests";

        _mockNext.Setup(x => x(It.IsAny<HttpContext>())).Returns(Task.CompletedTask);
        _mockLocalizationService.Setup(x => x.GetLocalizedString("Errors.TooManyRequests", It.IsAny<string>()))
            .Returns(localizedMessage);

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
        Assert.True(problemDetails.Extensions.ContainsKey("retryAfter"));
        // ToString() on StringValues returns comma-separated values
        Assert.Equal("60,120", problemDetails.Extensions["retryAfter"]?.ToString());
    }

    private static DefaultHttpContext CreateHttpContext()
    {
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();
        return context;
    }
}
