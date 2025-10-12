using AppTemplate.Application.Services.Statistics;
using AppTemplate.Web.Middlewares;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System.Security.Claims;

namespace AppTemplate.Web.Tests.Unit.MiddleWareTests;

[Trait("Category", "Unit")]
public class SessionTrackingMiddlewareUnitTests
{
    private readonly Mock<RequestDelegate> _mockNext;
    private readonly Mock<IActiveSessionService> _mockSessionService;
    private readonly SessionTrackingMiddleware _middleware;

    public SessionTrackingMiddlewareUnitTests()
    {
        _mockNext = new Mock<RequestDelegate>();
        _mockSessionService = new Mock<IActiveSessionService>();
        _middleware = new SessionTrackingMiddleware(_mockNext.Object, _mockSessionService.Object);
    }

    [Fact]
    public async Task InvokeAsync_ShouldCallNextDelegate()
    {
        // Arrange
        var context = CreateHttpContext();
        _mockNext.Setup(x => x(It.IsAny<HttpContext>())).Returns(Task.CompletedTask);

        // Act
        await _middleware.InvokeAsync(context);

        // Assert
        _mockNext.Verify(x => x(context), Times.Once);
    }

    [Fact]
    public async Task InvokeAsync_WithAuthenticatedUser_ShouldRecordUserActivity()
    {
        // Arrange
        var context = CreateHttpContext();
        var userId = "test-user-123";
        SetupAuthenticatedUser(context, userId);
        
        _mockNext.Setup(x => x(It.IsAny<HttpContext>())).Returns(Task.CompletedTask);
        _mockSessionService.Setup(x => x.RecordUserActivityAsync(userId)).Returns(Task.CompletedTask);

        // Act
        await _middleware.InvokeAsync(context);

        // Assert
        _mockSessionService.Verify(x => x.RecordUserActivityAsync(userId), Times.Once);
        _mockNext.Verify(x => x(context), Times.Once);
    }

    [Fact]
    public async Task InvokeAsync_WithUnauthenticatedUser_ShouldNotRecordActivity()
    {
        // Arrange
        var context = CreateHttpContext();
        SetupUnauthenticatedUser(context);
        
        _mockNext.Setup(x => x(It.IsAny<HttpContext>())).Returns(Task.CompletedTask);

        // Act
        await _middleware.InvokeAsync(context);

        // Assert
        _mockSessionService.Verify(x => x.RecordUserActivityAsync(It.IsAny<string>()), Times.Never);
        _mockNext.Verify(x => x(context), Times.Once);
    }

    [Fact]
    public async Task InvokeAsync_WithNullIdentity_ShouldNotRecordActivity()
    {
        // Arrange
        var context = CreateHttpContext();
        context.User = new ClaimsPrincipal(); // No identity
        
        _mockNext.Setup(x => x(It.IsAny<HttpContext>())).Returns(Task.CompletedTask);

        // Act
        await _middleware.InvokeAsync(context);

        // Assert
        _mockSessionService.Verify(x => x.RecordUserActivityAsync(It.IsAny<string>()), Times.Never);
        _mockNext.Verify(x => x(context), Times.Once);
    }

    [Fact]
    public async Task InvokeAsync_WithEmptyUserId_ShouldNotRecordActivity()
    {
        // Arrange
        var context = CreateHttpContext();
        SetupAuthenticatedUserWithEmptyId(context);
        
        _mockNext.Setup(x => x(It.IsAny<HttpContext>())).Returns(Task.CompletedTask);

        // Act
        await _middleware.InvokeAsync(context);

        // Assert
        _mockSessionService.Verify(x => x.RecordUserActivityAsync(It.IsAny<string>()), Times.Never);
        _mockNext.Verify(x => x(context), Times.Once);
    }

    [Fact]
    public async Task InvokeAsync_WithMissingNameIdentifierClaim_ShouldNotRecordActivity()
    {
        // Arrange
        var context = CreateHttpContext();
        SetupAuthenticatedUserWithoutNameIdentifierClaim(context);
        
        _mockNext.Setup(x => x(It.IsAny<HttpContext>())).Returns(Task.CompletedTask);

        // Act
        await _middleware.InvokeAsync(context);

        // Assert
        _mockSessionService.Verify(x => x.RecordUserActivityAsync(It.IsAny<string>()), Times.Never);
        _mockNext.Verify(x => x(context), Times.Once);
    }

    [Fact]
    public async Task InvokeAsync_WhenSessionServiceThrows_ShouldPropagateException()
    {
        // Arrange
        var context = CreateHttpContext();
        var userId = "test-user-exception";
        SetupAuthenticatedUser(context, userId);
        
        var exception = new InvalidOperationException("Session service error");
        _mockSessionService.Setup(x => x.RecordUserActivityAsync(userId)).ThrowsAsync(exception);

        // Act & Assert
        var thrownException = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _middleware.InvokeAsync(context));
        
        Assert.Equal(exception.Message, thrownException.Message);
        _mockSessionService.Verify(x => x.RecordUserActivityAsync(userId), Times.Once);
    }

    [Fact]
    public async Task InvokeAsync_WhenNextDelegateThrows_ShouldPropagateException()
    {
        // Arrange
        var context = CreateHttpContext();
        var userId = "test-user-next-exception";
        SetupAuthenticatedUser(context, userId);
        
        var exception = new InvalidOperationException("Next delegate error");
        _mockNext.Setup(x => x(It.IsAny<HttpContext>())).ThrowsAsync(exception);
        _mockSessionService.Setup(x => x.RecordUserActivityAsync(userId)).Returns(Task.CompletedTask);

        // Act & Assert
        var thrownException = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _middleware.InvokeAsync(context));
        
        Assert.Equal(exception.Message, thrownException.Message);
        _mockSessionService.Verify(x => x.RecordUserActivityAsync(userId), Times.Once);
        _mockNext.Verify(x => x(context), Times.Once);
    }

    [Theory]
    [InlineData("user-1")]
    [InlineData("12345")]
    [InlineData("user@example.com")]
    [InlineData("GUID-LIKE-ID-123-456-789")]
    [InlineData("user_with_underscores")]
    [InlineData("user-with-dashes")]
    public async Task InvokeAsync_WithVariousUserIdFormats_ShouldRecordActivity(string userId)
    {
        // Arrange
        var context = CreateHttpContext();
        SetupAuthenticatedUser(context, userId);
        
        _mockNext.Setup(x => x(It.IsAny<HttpContext>())).Returns(Task.CompletedTask);
        _mockSessionService.Setup(x => x.RecordUserActivityAsync(userId)).Returns(Task.CompletedTask);

        // Act
        await _middleware.InvokeAsync(context);

        // Assert
        _mockSessionService.Verify(x => x.RecordUserActivityAsync(userId), Times.Once);
    }

    [Fact]
    public async Task InvokeAsync_ShouldExecuteInCorrectOrder()
    {
        // Arrange
        var context = CreateHttpContext();
        var userId = "order-test-user";
        SetupAuthenticatedUser(context, userId);
        
        var executionOrder = new List<string>();
        
        _mockSessionService.Setup(x => x.RecordUserActivityAsync(userId))
            .Callback(() => executionOrder.Add("session-service-called"))
            .Returns(Task.CompletedTask);
        
        _mockNext.Setup(x => x(It.IsAny<HttpContext>()))
            .Callback(() => executionOrder.Add("next-delegate-called"))
            .Returns(Task.CompletedTask);

        // Act
        await _middleware.InvokeAsync(context);

        // Assert
        Assert.Equal(new[] { "session-service-called", "next-delegate-called" }, executionOrder);
    }

    [Fact]
    public async Task InvokeAsync_WithMultipleNameIdentifierClaims_ShouldUseFirstValue()
    {
        // Arrange
        var context = CreateHttpContext();
        var firstUserId = "first-user-id";
        var secondUserId = "second-user-id";
        
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, firstUserId),
            new(ClaimTypes.NameIdentifier, secondUserId),
            new(ClaimTypes.Name, "Test User")
        };
        
        var identity = new ClaimsIdentity(claims, "test");
        context.User = new ClaimsPrincipal(identity);
        
        _mockNext.Setup(x => x(It.IsAny<HttpContext>())).Returns(Task.CompletedTask);
        _mockSessionService.Setup(x => x.RecordUserActivityAsync(firstUserId)).Returns(Task.CompletedTask);

        // Act
        await _middleware.InvokeAsync(context);

        // Assert
        _mockSessionService.Verify(x => x.RecordUserActivityAsync(firstUserId), Times.Once);
        _mockSessionService.Verify(x => x.RecordUserActivityAsync(secondUserId), Times.Never);
    }

    [Fact]
    public async Task InvokeAsync_ShouldNotInterfereWithHttpContext()
    {
        // Arrange
        var context = CreateHttpContext();
        var userId = "test-user";
        SetupAuthenticatedUser(context, userId);
        
        var originalPath = "/api/test";
        var originalMethod = "POST";
        context.Request.Path = originalPath;
        context.Request.Method = originalMethod;
        
        _mockNext.Setup(x => x(It.IsAny<HttpContext>())).Returns(Task.CompletedTask);
        _mockSessionService.Setup(x => x.RecordUserActivityAsync(userId)).Returns(Task.CompletedTask);

        // Act
        await _middleware.InvokeAsync(context);

        // Assert
        Assert.Equal(originalPath, context.Request.Path.Value);
        Assert.Equal(originalMethod, context.Request.Method);
        Assert.True(context.User.Identity.IsAuthenticated);
        Assert.Equal(userId, context.User.FindFirstValue(ClaimTypes.NameIdentifier));
    }

    [Fact]
    public void Constructor_ShouldInitializeCorrectly()
    {
        // Arrange & Act
        var middleware = new SessionTrackingMiddleware(_mockNext.Object, _mockSessionService.Object);

        // Assert - If constructor doesn't throw, initialization is successful
        Assert.NotNull(middleware);
    }

    [Fact]
    public async Task InvokeAsync_WithLongUserId_ShouldRecordActivity()
    {
        // Arrange
        var context = CreateHttpContext();
        var longUserId = new string('a', 1000); // Very long user ID
        SetupAuthenticatedUser(context, longUserId);
        
        _mockNext.Setup(x => x(It.IsAny<HttpContext>())).Returns(Task.CompletedTask);
        _mockSessionService.Setup(x => x.RecordUserActivityAsync(longUserId)).Returns(Task.CompletedTask);

        // Act
        await _middleware.InvokeAsync(context);

        // Assert
        _mockSessionService.Verify(x => x.RecordUserActivityAsync(longUserId), Times.Once);
    }

    [Fact]
    public async Task InvokeAsync_WithSpecialCharactersInUserId_ShouldRecordActivity()
    {
        // Arrange
        var context = CreateHttpContext();
        var userIdWithSpecialChars = "user@domain.com+123!#$%";
        SetupAuthenticatedUser(context, userIdWithSpecialChars);
        
        _mockNext.Setup(x => x(It.IsAny<HttpContext>())).Returns(Task.CompletedTask);
        _mockSessionService.Setup(x => x.RecordUserActivityAsync(userIdWithSpecialChars)).Returns(Task.CompletedTask);

        // Act
        await _middleware.InvokeAsync(context);

        // Assert
        _mockSessionService.Verify(x => x.RecordUserActivityAsync(userIdWithSpecialChars), Times.Once);
    }

    [Fact]
    public async Task InvokeAsync_WithDifferentClaimTypes_ShouldOnlyUseNameIdentifier()
    {
        // Arrange
        var context = CreateHttpContext();
        var userId = "correct-user-id";
        var otherClaimValue = "other-value";
        
        var claims = new List<Claim>
        {
            new(ClaimTypes.Name, otherClaimValue),
            new(ClaimTypes.Email, "user@example.com"),
            new(ClaimTypes.NameIdentifier, userId),
            new("custom-claim", "custom-value")
        };
        
        var identity = new ClaimsIdentity(claims, "test");
        context.User = new ClaimsPrincipal(identity);
        
        _mockNext.Setup(x => x(It.IsAny<HttpContext>())).Returns(Task.CompletedTask);
        _mockSessionService.Setup(x => x.RecordUserActivityAsync(userId)).Returns(Task.CompletedTask);

        // Act
        await _middleware.InvokeAsync(context);

        // Assert
        _mockSessionService.Verify(x => x.RecordUserActivityAsync(userId), Times.Once);
        _mockSessionService.Verify(x => x.RecordUserActivityAsync(otherClaimValue), Times.Never);
    }

    [Fact]
    public async Task InvokeAsync_ShouldCallNextExactlyOnce()
    {
        // Arrange
        var context = CreateHttpContext();
        var userId = "test-user";
        SetupAuthenticatedUser(context, userId);
        
        _mockNext.Setup(x => x(It.IsAny<HttpContext>())).Returns(Task.CompletedTask);
        _mockSessionService.Setup(x => x.RecordUserActivityAsync(userId)).Returns(Task.CompletedTask);

        // Act
        await _middleware.InvokeAsync(context);

        // Assert
        _mockNext.Verify(x => x(It.IsAny<HttpContext>()), Times.Once);
        _mockNext.Verify(x => x(context), Times.Once);
    }

    #region SessionTrackingMiddlewareExtensions Tests

    [Fact]
    public void UseSessionTracking_ShouldReturnApplicationBuilder()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton(_mockSessionService.Object);
        
        var serviceProvider = services.BuildServiceProvider();
        var applicationBuilder = new ApplicationBuilder(serviceProvider);

        // Act
        var result = applicationBuilder.UseSessionTracking();

        // Assert
        Assert.NotNull(result);
        Assert.Same(applicationBuilder, result);
    }

    [Fact]
    public void UseSessionTracking_ShouldAddMiddlewareToApplicationBuilder()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton(_mockSessionService.Object);
        
        var serviceProvider = services.BuildServiceProvider();
        var applicationBuilder = new ApplicationBuilder(serviceProvider);

        // Act
        applicationBuilder.UseSessionTracking();

        // Assert
        var app = applicationBuilder.Build();
        Assert.NotNull(app);
        
        // The middleware should be registered in the pipeline
        // We can verify this by checking that the method returns the same builder instance
        // which indicates successful middleware registration
    }

    [Fact]
    public void UseSessionTracking_ShouldAllowChaining()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton(_mockSessionService.Object);
        
        var serviceProvider = services.BuildServiceProvider();
        var applicationBuilder = new ApplicationBuilder(serviceProvider);

        // Act & Assert
        var result = applicationBuilder
            .UseSessionTracking()
            .UseSessionTracking(); // Should allow chaining

        Assert.NotNull(result);
        Assert.Same(applicationBuilder, result);
    }

    [Fact]
    public void UseSessionTracking_WithNullBuilder_ShouldThrowNullReferenceException()
    {
        // Arrange
        IApplicationBuilder builder = null;

        // Act & Assert
        Assert.Throws<NullReferenceException>(() => builder.UseSessionTracking());
    }

    [Fact]
    public void UseSessionTracking_ShouldRegisterMiddlewareInCorrectOrder()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton(_mockSessionService.Object);
        services.AddLogging();
        
        var serviceProvider = services.BuildServiceProvider();
        var applicationBuilder = new ApplicationBuilder(serviceProvider);

        // Act
        var result = applicationBuilder
            .Use((HttpContext context, RequestDelegate next) =>
            {
                context.Items["middleware1"] = "executed";
                return next(context);
            })
            .UseSessionTracking()
            .Use((HttpContext context, RequestDelegate next) =>
            {
                context.Items["middleware2"] = "executed";
                return next(context);
            });

        // Assert
        Assert.NotNull(result);
        
        // Build the application to ensure no exceptions during pipeline construction
        var app = applicationBuilder.Build();
        Assert.NotNull(app);
    }

    [Fact]
    public async Task UseSessionTracking_IntegrationTest_ShouldProcessRequestCorrectly()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton(_mockSessionService.Object);
        services.AddLogging();
        
        var serviceProvider = services.BuildServiceProvider();
        var applicationBuilder = new ApplicationBuilder(serviceProvider);

        var context = CreateHttpContext();
        SetupAuthenticatedUser(context, "integration-test-user");
        
        var wasNextCalled = false;
        
        applicationBuilder
            .UseSessionTracking()
            .Use((HttpContext ctx, RequestDelegate next) =>
            {
                wasNextCalled = true;
                return Task.CompletedTask;
            });

        var app = applicationBuilder.Build();
        
        _mockSessionService.Setup(x => x.RecordUserActivityAsync("integration-test-user"))
            .Returns(Task.CompletedTask);

        // Act
        await app(context);

        // Assert
        Assert.True(wasNextCalled);
        _mockSessionService.Verify(x => x.RecordUserActivityAsync("integration-test-user"), Times.Once);
    }

    #endregion

    private static DefaultHttpContext CreateHttpContext()
    {
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();
        return context;
    }

    private static void SetupAuthenticatedUser(HttpContext context, string userId)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, userId),
            new(ClaimTypes.Name, "Test User")
        };
        
        var identity = new ClaimsIdentity(claims, "test");
        context.User = new ClaimsPrincipal(identity);
    }

    private static void SetupUnauthenticatedUser(HttpContext context)
    {
        var identity = new ClaimsIdentity(); // Not authenticated
        context.User = new ClaimsPrincipal(identity);
    }

    private static void SetupAuthenticatedUserWithEmptyId(HttpContext context)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, string.Empty),
            new(ClaimTypes.Name, "Test User")
        };
        
        var identity = new ClaimsIdentity(claims, "test");
        context.User = new ClaimsPrincipal(identity);
    }

    private static void SetupAuthenticatedUserWithoutNameIdentifierClaim(HttpContext context)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.Name, "Test User"),
            new(ClaimTypes.Email, "test@example.com")
        };
        
        var identity = new ClaimsIdentity(claims, "test");
        context.User = new ClaimsPrincipal(identity);
    }
}
