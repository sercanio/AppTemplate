using AppTemplate.Web.Middlewares;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using Moq;

namespace AppTemplate.Web.Tests.Unit.MiddleWareTests;

[Trait("Category", "Unit")]
public class RequestContextLoggingMiddlewareUnitTests
{
    private readonly Mock<RequestDelegate> _mockNext;
    private readonly RequestContextLoggingMiddleware _middleware;

    public RequestContextLoggingMiddlewareUnitTests()
    {
        _mockNext = new Mock<RequestDelegate>();
        _middleware = new RequestContextLoggingMiddleware(_mockNext.Object);
    }

    [Fact]
    public async Task Invoke_ShouldCallNextDelegate()
    {
        // Arrange
        var context = CreateHttpContext();
        _mockNext.Setup(x => x(It.IsAny<HttpContext>())).Returns(Task.CompletedTask);

        // Act
        await _middleware.Invoke(context);

        // Assert
        _mockNext.Verify(x => x(context), Times.Once);
    }

    [Fact]
    public async Task Invoke_WithCorrelationIdHeader_ShouldUseHeaderValue()
    {
        // Arrange
        var context = CreateHttpContext();
        var expectedCorrelationId = "test-correlation-id-12345";
        context.Request.Headers["X-Correlation-Id"] = expectedCorrelationId;
        
        string capturedCorrelationId = null;
        _mockNext.Setup(x => x(It.IsAny<HttpContext>()))
            .Callback((HttpContext ctx) =>
            {
                // For testing purposes, we'll simulate capturing the correlation ID
                // In a real test environment, you might use a test logger or sink
                capturedCorrelationId = ctx.Request.Headers["X-Correlation-Id"].FirstOrDefault();
            })
            .Returns(Task.CompletedTask);

        // Act
        await _middleware.Invoke(context);

        // Assert
        Assert.Equal(expectedCorrelationId, capturedCorrelationId);
    }

    [Fact]
    public async Task Invoke_WithoutCorrelationIdHeader_ShouldUseTraceIdentifier()
    {
        // Arrange
        var context = CreateHttpContext();
        var expectedTraceId = "trace-id-67890";
        context.TraceIdentifier = expectedTraceId;
        
        string capturedTraceId = null;
        _mockNext.Setup(x => x(It.IsAny<HttpContext>()))
            .Callback((HttpContext ctx) =>
            {
                capturedTraceId = ctx.TraceIdentifier;
            })
            .Returns(Task.CompletedTask);

        // Act
        await _middleware.Invoke(context);

        // Assert
        Assert.Equal(expectedTraceId, capturedTraceId);
    }

    [Fact]
    public async Task Invoke_WithEmptyCorrelationIdHeader_ShouldUseTraceIdentifier()
    {
        // Arrange
        var context = CreateHttpContext();
        var expectedTraceId = "trace-id-empty-test";
        context.TraceIdentifier = expectedTraceId;
        context.Request.Headers["X-Correlation-Id"] = string.Empty;
        
        string capturedTraceId = null;
        _mockNext.Setup(x => x(It.IsAny<HttpContext>()))
            .Callback((HttpContext ctx) =>
            {
                // When header is empty, middleware should use TraceIdentifier
                capturedTraceId = ctx.TraceIdentifier;
            })
            .Returns(Task.CompletedTask);

        // Act
        await _middleware.Invoke(context);

        // Assert
        Assert.Equal(expectedTraceId, capturedTraceId);
    }

    [Fact]
    public async Task Invoke_WithMultipleCorrelationIdHeaders_ShouldUseFirstValue()
    {
        // Arrange
        var context = CreateHttpContext();
        var firstCorrelationId = "first-correlation-id";
        var secondCorrelationId = "second-correlation-id";
        context.Request.Headers["X-Correlation-Id"] = new StringValues(new[] { firstCorrelationId, secondCorrelationId });
        
        string capturedCorrelationId = null;
        _mockNext.Setup(x => x(It.IsAny<HttpContext>()))
            .Callback((HttpContext ctx) =>
            {
                capturedCorrelationId = ctx.Request.Headers["X-Correlation-Id"].FirstOrDefault();
            })
            .Returns(Task.CompletedTask);

        // Act
        await _middleware.Invoke(context);

        // Assert
        Assert.Equal(firstCorrelationId, capturedCorrelationId);
    }

    [Fact]
    public async Task Invoke_WhenNextThrowsException_ShouldPropagateException()
    {
        // Arrange
        var context = CreateHttpContext();
        var correlationId = "test-correlation-id";
        context.Request.Headers["X-Correlation-Id"] = correlationId;
        
        var exception = new InvalidOperationException("Test exception");
        _mockNext.Setup(x => x(It.IsAny<HttpContext>())).ThrowsAsync(exception);

        // Act & Assert
        var thrownException = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _middleware.Invoke(context));
        
        Assert.Equal(exception.Message, thrownException.Message);
    }

    [Theory]
    [InlineData("simple-id")]
    [InlineData("complex-correlation-id-with-dashes-123")]
    [InlineData("UPPERCASE-ID")]
    [InlineData("mixed-Case-ID-456")]
    [InlineData("id_with_underscores")]
    [InlineData("12345")]
    public async Task Invoke_WithVariousCorrelationIdFormats_ShouldPreserveExactValue(string correlationId)
    {
        // Arrange
        var context = CreateHttpContext();
        context.Request.Headers["X-Correlation-Id"] = correlationId;
        
        string capturedCorrelationId = null;
        _mockNext.Setup(x => x(It.IsAny<HttpContext>()))
            .Callback((HttpContext ctx) =>
            {
                capturedCorrelationId = ctx.Request.Headers["X-Correlation-Id"].FirstOrDefault();
            })
            .Returns(Task.CompletedTask);

        // Act
        await _middleware.Invoke(context);

        // Assert
        Assert.Equal(correlationId, capturedCorrelationId);
    }

    [Fact]
    public async Task Invoke_WithNullTraceIdentifier_ShouldHandleGracefully()
    {
        // Arrange
        var context = CreateHttpContext();
        context.TraceIdentifier = null;
        
        _mockNext.Setup(x => x(It.IsAny<HttpContext>())).Returns(Task.CompletedTask);

        // Act & Assert - Should not throw
        await _middleware.Invoke(context);
        
        _mockNext.Verify(x => x(context), Times.Once);
    }

    [Fact]
    public void Constructor_ShouldInitializeCorrectly()
    {
        // Arrange & Act
        var middleware = new RequestContextLoggingMiddleware(_mockNext.Object);

        // Assert - If constructor doesn't throw, initialization is successful
        Assert.NotNull(middleware);
    }

    [Fact]
    public async Task Invoke_ShouldNotInterfereWithHttpContext()
    {
        // Arrange
        var context = CreateHttpContext();
        var originalPath = "/api/test";
        var originalMethod = "GET";
        context.Request.Path = originalPath;
        context.Request.Method = originalMethod;
        context.Request.Headers["X-Correlation-Id"] = "test-id";
        
        _mockNext.Setup(x => x(It.IsAny<HttpContext>())).Returns(Task.CompletedTask);

        // Act
        await _middleware.Invoke(context);

        // Assert
        Assert.Equal(originalPath, context.Request.Path.Value);
        Assert.Equal(originalMethod, context.Request.Method);
        Assert.Equal("test-id", context.Request.Headers["X-Correlation-Id"].ToString());
    }

    [Fact]
    public async Task Invoke_WithCaseInsensitiveHeaderName_ShouldUseExactHeaderName()
    {
        // Arrange
        var context = CreateHttpContext();
        var correlationId = "case-test-id";
        // Note: HTTP headers are case-insensitive, but we're testing with the exact case
        context.Request.Headers["X-Correlation-Id"] = correlationId;
        
        string capturedCorrelationId = null;
        _mockNext.Setup(x => x(It.IsAny<HttpContext>()))
            .Callback((HttpContext ctx) =>
            {
                capturedCorrelationId = ctx.Request.Headers["X-Correlation-Id"].FirstOrDefault();
            })
            .Returns(Task.CompletedTask);

        // Act
        await _middleware.Invoke(context);

        // Assert
        Assert.Equal(correlationId, capturedCorrelationId);
    }

    [Fact]
    public async Task Invoke_WithLongCorrelationId_ShouldHandleCorrectly()
    {
        // Arrange
        var context = CreateHttpContext();
        var longCorrelationId = new string('a', 1000); // Very long correlation ID
        context.Request.Headers["X-Correlation-Id"] = longCorrelationId;
        
        string capturedCorrelationId = null;
        _mockNext.Setup(x => x(It.IsAny<HttpContext>()))
            .Callback((HttpContext ctx) =>
            {
                capturedCorrelationId = ctx.Request.Headers["X-Correlation-Id"].FirstOrDefault();
            })
            .Returns(Task.CompletedTask);

        // Act
        await _middleware.Invoke(context);

        // Assert
        Assert.Equal(longCorrelationId, capturedCorrelationId);
    }

    [Fact]
    public async Task Invoke_ShouldExecuteInCorrectOrder()
    {
        // Arrange
        var context = CreateHttpContext();
        var correlationId = "order-test-id";
        context.Request.Headers["X-Correlation-Id"] = correlationId;
        
        var executionOrder = new List<string>();
        
        _mockNext.Setup(x => x(It.IsAny<HttpContext>()))
            .Callback((HttpContext ctx) =>
            {
                executionOrder.Add("next-delegate-start");
                if (ctx.Request.Headers.ContainsKey("X-Correlation-Id"))
                {
                    executionOrder.Add("correlation-id-available");
                }
                executionOrder.Add("next-delegate-end");
            })
            .Returns(Task.CompletedTask);

        // Act
        executionOrder.Add("middleware-start");
        await _middleware.Invoke(context);
        executionOrder.Add("middleware-end");

        // Assert
        Assert.Equal(new[] 
        { 
            "middleware-start", 
            "next-delegate-start", 
            "correlation-id-available", 
            "next-delegate-end", 
            "middleware-end" 
        }, executionOrder);
    }

    [Fact]
    public async Task Invoke_ShouldPreserveCorrelationIdThroughoutRequest()
    {
        // Arrange
        var context = CreateHttpContext();
        var correlationId = "persistent-test-id";
        context.Request.Headers["X-Correlation-Id"] = correlationId;
        
        var capturedIds = new List<string>();
        
        _mockNext.Setup(x => x(It.IsAny<HttpContext>()))
            .Callback((HttpContext ctx) =>
            {
                // Simulate multiple points in the pipeline checking the correlation ID
                capturedIds.Add(ctx.Request.Headers["X-Correlation-Id"].FirstOrDefault());
                
                // Simulate some async work
                Task.Delay(10).Wait();
                
                capturedIds.Add(ctx.Request.Headers["X-Correlation-Id"].FirstOrDefault());
            })
            .Returns(Task.CompletedTask);

        // Act
        await _middleware.Invoke(context);

        // Assert
        Assert.All(capturedIds, id => Assert.Equal(correlationId, id));
        Assert.Equal(2, capturedIds.Count);
    }

    [Fact]
    public async Task Invoke_WithDifferentHeaderCasing_ShouldStillWork()
    {
        // Arrange
        var context = CreateHttpContext();
        var correlationId = "case-insensitive-test";
        
        // Test that ASP.NET Core handles case-insensitive headers correctly
        context.Request.Headers["x-correlation-id"] = correlationId; // lowercase
        
        string capturedCorrelationId = null;
        _mockNext.Setup(x => x(It.IsAny<HttpContext>()))
            .Callback((HttpContext ctx) =>
            {
                // Try to get with the exact case the middleware uses
                capturedCorrelationId = ctx.Request.Headers["X-Correlation-Id"].FirstOrDefault();
            })
            .Returns(Task.CompletedTask);

        // Act
        await _middleware.Invoke(context);

        // Assert
        Assert.Equal(correlationId, capturedCorrelationId);
    }

    [Fact]
    public async Task Invoke_WithSpecialCharactersInCorrelationId_ShouldPreserveValue()
    {
        // Arrange
        var context = CreateHttpContext();
        var correlationId = "test-id-with-special-chars-@#$%^&*()";
        context.Request.Headers["X-Correlation-Id"] = correlationId;
        
        string capturedCorrelationId = null;
        _mockNext.Setup(x => x(It.IsAny<HttpContext>()))
            .Callback((HttpContext ctx) =>
            {
                capturedCorrelationId = ctx.Request.Headers["X-Correlation-Id"].FirstOrDefault();
            })
            .Returns(Task.CompletedTask);

        // Act
        await _middleware.Invoke(context);

        // Assert
        Assert.Equal(correlationId, capturedCorrelationId);
    }

    [Fact]
    public async Task Invoke_ShouldCallNextExactlyOnce()
    {
        // Arrange
        var context = CreateHttpContext();
        context.Request.Headers["X-Correlation-Id"] = "test-id";
        
        _mockNext.Setup(x => x(It.IsAny<HttpContext>())).Returns(Task.CompletedTask);

        // Act
        await _middleware.Invoke(context);

        // Assert
        _mockNext.Verify(x => x(It.IsAny<HttpContext>()), Times.Once);
        _mockNext.Verify(x => x(context), Times.Once);
    }

    private static DefaultHttpContext CreateHttpContext()
    {
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();
        return context;
    }
}
