using AppTemplate.Application.Behaviors;
using Ardalis.Result;
using MediatR;
using Microsoft.Extensions.Logging;
using Moq;

namespace AppTemplate.Application.Tests.Unit.Behaviours;

public class LoggingBehaviourUnitTests
{
  private readonly Mock<ILogger<LoggingBehavior<TestRequest, Result>>> _mockLogger;
  private readonly LoggingBehavior<TestRequest, Result> _loggingBehavior;
  private readonly CancellationToken _cancellationToken;

  public LoggingBehaviourUnitTests()
  {
    _mockLogger = new Mock<ILogger<LoggingBehavior<TestRequest, Result>>>();
    _loggingBehavior = new LoggingBehavior<TestRequest, Result>(_mockLogger.Object);
    _cancellationToken = CancellationToken.None;

  }

  [Fact]
  public async Task Handle_WhenRequestIsSuccessful_ShouldLogInformationMessages()
  {
    // Arrange
    var request = new TestRequest();
    var successResult = Result.Success();

    // Act
    var result = await _loggingBehavior.Handle(request, _ => Task.FromResult(successResult), _cancellationToken);

    // Assert
    Assert.True(result.IsSuccess);

    // Verify execution start log
    VerifyLogCalled(LogLevel.Information, "Executing request TestRequest");

    // Verify success log
    VerifyLogCalled(LogLevel.Information, "Request TestRequest processed successfully");

    // Verify no error logs
    VerifyLogNotCalled(LogLevel.Error);
  }

  [Fact]
  public async Task Handle_WhenRequestReturnsError_ShouldLogErrorMessage()
  {
    // Arrange
    var request = new TestRequest();
    var errorResult = Result.Error("Something went wrong");

    // Act
    var result = await _loggingBehavior.Handle(request, _ => Task.FromResult(errorResult), _cancellationToken);

    // Assert
    Assert.False(result.IsSuccess);

    // Verify execution start log
    VerifyLogCalled(LogLevel.Information, "Executing request TestRequest");

    // Verify error log
    VerifyLogCalled(LogLevel.Error, "Request TestRequest processed with error");

    // Verify no success log
    VerifyLogNotCalled(LogLevel.Information, "processed successfully");
  }

  [Fact]
  public async Task Handle_WhenExceptionIsThrown_ShouldLogExceptionAndRethrow()
  {
    // Arrange
    var request = new TestRequest();
    var exception = new InvalidOperationException("Test exception");

    // Act & Assert
    var thrownException = await Assert.ThrowsAsync<InvalidOperationException>(
        () => _loggingBehavior.Handle(request, _ => throw exception, _cancellationToken));

    Assert.Same(exception, thrownException);

    // Verify execution start log
    VerifyLogCalled(LogLevel.Information, "Executing request TestRequest");

    // Verify exception log
    VerifyLogCalledWithException(LogLevel.Error, "Request TestRequest processing failed", exception);
  }

  [Fact]
  public async Task Handle_WhenCancellationIsRequested_ShouldStillLogAndPropagateCancellation()
  {
    // Arrange
    var request = new TestRequest();
    var cancellationTokenSource = new CancellationTokenSource();
    cancellationTokenSource.Cancel();
    var cancelledToken = cancellationTokenSource.Token;

    // Act & Assert
    await Assert.ThrowsAsync<OperationCanceledException>(
        () => _loggingBehavior.Handle(request, _ => throw new OperationCanceledException(cancelledToken), cancelledToken));

    // Verify execution start log was called
    VerifyLogCalled(LogLevel.Information, "Executing request TestRequest");
  }

  [Fact]
  public async Task Handle_WithDifferentRequestTypes_ShouldLogCorrectRequestName()
  {
    // Arrange
    var customLogger = new Mock<ILogger<LoggingBehavior<CustomTestRequest, Result>>>();
    var customBehavior = new LoggingBehavior<CustomTestRequest, Result>(customLogger.Object);
    var request = new CustomTestRequest();
    var successResult = Result.Success();

    // Act
    await customBehavior.Handle(request, _ => Task.FromResult(successResult), _cancellationToken);

    // Assert
    VerifyLogCalled(customLogger, LogLevel.Information, "Executing request CustomTestRequest");
    VerifyLogCalled(customLogger, LogLevel.Information, "Request CustomTestRequest processed successfully");
  }

  [Fact]
  public async Task Handle_WhenNextDelegateIsNull_ShouldThrowArgumentNullException()
  {
    // Arrange
    var request = new TestRequest();

    // Act & Assert
    await Assert.ThrowsAsync<ArgumentNullException>(
        () => _loggingBehavior.Handle(request, null!, _cancellationToken));
  }

  [Fact]
  public async Task Handle_WhenRequestIsNull_ShouldThrowArgumentNullException()
  {
    // Act & Assert
    await Assert.ThrowsAsync<ArgumentNullException>(
        () => _loggingBehavior.Handle(null!, _ => Task.FromResult(Result.Success()), _cancellationToken));
  }

  [Fact]
  public void Constructor_WhenLoggerIsNull_ShouldThrowArgumentNullException()
  {
    // Act & Assert
    Assert.Throws<ArgumentNullException>(() => new LoggingBehavior<TestRequest, Result>(null!));
  }

  [Fact]
  public async Task Handle_WhenResultValueIsError_ShouldLogErrorWithValue()
  {
    // Arrange
    var request = new TestRequest();
    var errorMessage = "Specific error occurred";
    var errorResult = Result.Error(errorMessage);

    // Act
    var result = await _loggingBehavior.Handle(request, _ => Task.FromResult(errorResult), _cancellationToken);

    // Assert
    Assert.False(result.IsSuccess);
    Assert.Contains(errorMessage, result.Errors.Select(e => e.ToString()));

    // Verify that the error is logged
    VerifyLogCalled(LogLevel.Error, "Request TestRequest processed with error");
  }

  [Fact]
  public async Task Handle_WhenGenericExceptionIsThrown_ShouldLogAndRethrow()
  {
    // Arrange
    var request = new TestRequest();
    var exception = new Exception("Generic exception");

    // Act & Assert
    var thrownException = await Assert.ThrowsAsync<Exception>(
        () => _loggingBehavior.Handle(request, _ => throw exception, _cancellationToken));

    Assert.Same(exception, thrownException);

    // Verify exception is logged with the actual exception object
    VerifyLogCalledWithException(LogLevel.Error, "Request TestRequest processing failed", exception);
  }

  [Fact]
  public async Task Handle_WithSuccessResult_ShouldOnlyLogInformationLevel()
  {
    // Arrange
    var request = new TestRequest();
    var successResult = Result.Success();

    // Act
    await _loggingBehavior.Handle(request, _ => Task.FromResult(successResult), _cancellationToken);

    // Assert - Verify only Information level logs, no Warning or Error
    VerifyLogNotCalled(LogLevel.Warning);
    VerifyLogNotCalled(LogLevel.Error);

    // Verify exactly 2 Information logs (start and success)
    _mockLogger.Verify(
        x => x.Log(
            It.Is<LogLevel>(l => l == LogLevel.Information),
            It.IsAny<EventId>(),
            It.IsAny<It.IsAnyType>(),
            It.IsAny<Exception?>(),
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
        Times.Exactly(2));
  }

  [Fact]
  public async Task Handle_WhenNextDelegateReturnsAsync_ShouldHandleCorrectly()
  {
    // Arrange
    var request = new TestRequest();
    var successResult = Result.Success();
    var taskCompletionSource = new TaskCompletionSource<Result>();
    taskCompletionSource.SetResult(successResult);

    // Act
    var result = await _loggingBehavior.Handle(request, _ => taskCompletionSource.Task, _cancellationToken);

    // Assert
    Assert.True(result.IsSuccess);
    VerifyLogCalled(LogLevel.Information, "Executing request TestRequest");
    VerifyLogCalled(LogLevel.Information, "Request TestRequest processed successfully");
  }

  [Fact]
  public async Task Handle_WithDelayedResult_ShouldWaitForCompletion()
  {
    // Arrange
    var request = new TestRequest();
    var successResult = Result.Success();

    // Act
    var result = await _loggingBehavior.Handle(request, async cancellationToken =>
    {
      await Task.Delay(50, cancellationToken);
      return successResult;
    }, _cancellationToken);

    // Assert
    Assert.True(result.IsSuccess);
    VerifyLogCalled(LogLevel.Information, "Request TestRequest processed successfully");
  }

  [Fact]
  public async Task Handle_WhenCancellationTokenIsUsed_ShouldPassThroughToNextDelegate()
  {
    // Arrange
    var request = new TestRequest();
    var successResult = Result.Success();
    var cts = new CancellationTokenSource();
    var receivedToken = CancellationToken.None;

    // Act
    await _loggingBehavior.Handle(request, ct =>
    {
      receivedToken = ct;
      return Task.FromResult(successResult);
    }, cts.Token);

    // Assert
    Assert.Equal(cts.Token, receivedToken);
  }

  // Helper methods to avoid expression tree issues
  private void VerifyLogCalled(LogLevel logLevel, string expectedMessage)
  {
    VerifyLogCalled(_mockLogger, logLevel, expectedMessage);
  }

  private static void VerifyLogCalled<T>(Mock<ILogger<T>> logger, LogLevel logLevel, string expectedMessage)
  {
    logger.Verify(
        x => x.Log(
            It.Is<LogLevel>(l => l == logLevel),
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, _) => CheckMessageContains(v, expectedMessage)),
            It.IsAny<Exception?>(),
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
        Times.AtLeastOnce);
  }

  private void VerifyLogCalledWithException(LogLevel logLevel, string expectedMessage, Exception expectedException)
  {
    _mockLogger.Verify(
        x => x.Log(
            It.Is<LogLevel>(l => l == logLevel),
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, _) => CheckMessageContains(v, expectedMessage)),
            It.Is<Exception?>(ex => ex == expectedException),
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
        Times.AtLeastOnce);
  }

  private void VerifyLogNotCalled(LogLevel logLevel)
  {
    _mockLogger.Verify(
        x => x.Log(
            It.Is<LogLevel>(l => l == logLevel),
            It.IsAny<EventId>(),
            It.IsAny<It.IsAnyType>(),
            It.IsAny<Exception?>(),
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
        Times.Never);
  }

  private void VerifyLogNotCalled(LogLevel logLevel, string messageToAvoid)
  {
    _mockLogger.Verify(
        x => x.Log(
            It.Is<LogLevel>(l => l == logLevel),
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, _) => CheckMessageContains(v, messageToAvoid)),
            It.IsAny<Exception?>(),
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
        Times.Never);
  }

  // Static helper method to check message content without ToString() in expression tree
  private static bool CheckMessageContains(object message, string expectedText)
  {
    if (message == null) return false;
    var messageString = message.ToString();
    return messageString != null && messageString.Contains(expectedText);
  }
}

// Test request classes
public class TestRequest : IBaseRequest
{
}

public class CustomTestRequest : IBaseRequest
{
}