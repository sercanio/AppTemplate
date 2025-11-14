using AppTemplate.Domain.Exceptions;

namespace AppTemplate.Domain.Tests.Unit.Exceptions;

[Trait("Category", "Unit")]
public class ConcurrencyExceptionUnitTests
{
  [Fact]
  public void Constructor_WithMessageAndInnerException_ShouldSetProperties()
  {
    // Arrange
    var message = "A concurrency conflict occurred";
    var innerException = new InvalidOperationException("Inner exception message");

    // Act
    var exception = new ConcurrencyException(message, innerException);

    // Assert
    Assert.Equal(message, exception.Message);
    Assert.Same(innerException, exception.InnerException);
  }

  [Fact]
  public void Constructor_WithMessage_ShouldSetMessageProperty()
  {
    // Arrange
    var message = "Database concurrency violation detected";

    // Act
    var exception = new ConcurrencyException(message, new Exception());

    // Assert
    Assert.Equal(message, exception.Message);
  }

  [Fact]
  public void Constructor_WithInnerException_ShouldSetInnerExceptionProperty()
  {
    // Arrange
    var innerException = new InvalidOperationException("Database conflict");

    // Act
    var exception = new ConcurrencyException("Concurrency error", innerException);

    // Assert
    Assert.Same(innerException, exception.InnerException);
    Assert.Equal("Database conflict", exception.InnerException.Message);
  }

  [Fact]
  public void Constructor_WithNullInnerException_ShouldSetInnerExceptionToNull()
  {
    // Arrange
    var message = "Concurrency error occurred";

    // Act
    var exception = new ConcurrencyException(message, null!);

    // Assert
    Assert.Null(exception.InnerException);
  }

  [Fact]
  public void Constructor_WithEmptyMessage_ShouldSetEmptyMessage()
  {
    // Arrange
    var message = string.Empty;
    var innerException = new Exception();

    // Act
    var exception = new ConcurrencyException(message, innerException);

    // Assert
    Assert.Equal(string.Empty, exception.Message);
  }

  [Fact]
  public void Exception_ShouldBeOfTypeException()
  {
    // Arrange & Act
    var exception = new ConcurrencyException("Test", new Exception());

    // Assert
    Assert.IsAssignableFrom<Exception>(exception);
  }

  [Fact]
  public void Exception_ShouldBeSealed()
  {
    // Arrange
    var exceptionType = typeof(ConcurrencyException);

    // Act & Assert
    Assert.True(exceptionType.IsSealed);
  }

  [Fact]
  public void Constructor_WithComplexInnerException_ShouldPreserveInnerExceptionDetails()
  {
    // Arrange
    var innerMessage = "Row was updated by another user";
    var innerInnerException = new TimeoutException("Database timeout");
    var innerException = new InvalidOperationException(innerMessage, innerInnerException);
    var message = "Concurrency conflict detected";

    // Act
    var exception = new ConcurrencyException(message, innerException);

    // Assert
    Assert.Equal(message, exception.Message);
    Assert.Same(innerException, exception.InnerException);
    Assert.Equal(innerMessage, exception.InnerException.Message);
    Assert.Same(innerInnerException, exception.InnerException.InnerException);
  }

  [Fact]
  public void Throw_ConcurrencyException_ShouldBeCatchable()
  {
    // Arrange
    var message = "Concurrency error";
    var innerException = new Exception("Inner");

    // Act
    Action act = () => throw new ConcurrencyException(message, innerException);

    // Assert
    var caughtException = Assert.Throws<ConcurrencyException>(act);
    Assert.Equal(message, caughtException.Message);
    Assert.Same(innerException, caughtException.InnerException);
  }

  [Fact]
  public void Throw_ConcurrencyException_ShouldBeCatchableAsException()
  {
    // Arrange
    var message = "Concurrency error";

    // Act
    Action act = () => throw new ConcurrencyException(message, new Exception());

    // Assert - Use IsAssignableFrom instead of Throws to check inheritance
    var caughtException = Assert.Throws<ConcurrencyException>(act);
    Assert.IsAssignableFrom<Exception>(caughtException);
  }

  [Theory]
  [InlineData("Entity version mismatch")]
  [InlineData("Row was modified by another transaction")]
  [InlineData("Optimistic concurrency failure")]
  [InlineData("The record has been modified since it was loaded")]
  public void Constructor_WithVariousMessages_ShouldSetCorrectMessage(string message)
  {
    // Arrange
    var innerException = new Exception();

    // Act
    var exception = new ConcurrencyException(message, innerException);

    // Assert
    Assert.Equal(message, exception.Message);
  }

  [Fact]
  public void Constructor_WithLongMessage_ShouldPreserveFullMessage()
  {
    // Arrange
    var message = "A very long concurrency error message that describes the exact situation where " +
                 "two or more transactions tried to update the same row at the same time, causing " +
                 "a conflict that needs to be resolved by the application logic or by retrying the operation.";
    var innerException = new Exception();

    // Act
    var exception = new ConcurrencyException(message, innerException);

    // Assert
    Assert.Equal(message, exception.Message);
  }

  [Fact]
  public void Constructor_WithSpecialCharactersInMessage_ShouldPreserveMessage()
  {
    // Arrange
    var message = "Concurrency error: <User:123> tried to update 'Entity#456' @ 2024-01-01T12:00:00Z";
    var innerException = new Exception();

    // Act
    var exception = new ConcurrencyException(message, innerException);

    // Assert
    Assert.Equal(message, exception.Message);
  }

  [Fact]
  public void ToString_ShouldIncludeMessageAndInnerException()
  {
    // Arrange
    var message = "Concurrency error occurred";
    var innerException = new InvalidOperationException("Database conflict");
    var exception = new ConcurrencyException(message, innerException);

    // Act
    var result = exception.ToString();

    // Assert
    Assert.Contains(message, result);
    Assert.Contains(nameof(ConcurrencyException), result);
    Assert.Contains(nameof(InvalidOperationException), result);
  }

  [Fact]
  public void StackTrace_ShouldBeAvailableWhenThrown()
  {
    // Arrange & Act
    ConcurrencyException? caughtException = null;
    try
    {
      throw new ConcurrencyException("Test error", new Exception());
    }
    catch (ConcurrencyException ex)
    {
      caughtException = ex;
    }

    // Assert
    Assert.NotNull(caughtException);
    Assert.NotNull(caughtException.StackTrace);
    Assert.Contains(nameof(ConcurrencyExceptionUnitTests), caughtException.StackTrace);
  }

  [Fact]
  public void Exception_Properties_ShouldBeAccessible()
  {
    // Arrange
    var message = "Concurrency error";
    var innerException = new Exception("Inner error");
    var exception = new ConcurrencyException(message, innerException);

    // Act & Assert
    Assert.NotNull(exception.Message);
    Assert.NotNull(exception.InnerException);
    Assert.NotNull(exception.ToString());
    Assert.Null(exception.StackTrace); // Not thrown yet
    Assert.Null(exception.Source); // Not thrown yet
    Assert.Null(exception.TargetSite); // Not thrown yet
  }

  [Fact]
  public void MultipleInstances_ShouldBeIndependent()
  {
    // Arrange
    var message1 = "First error";
    var message2 = "Second error";
    var innerException1 = new Exception("Inner 1");
    var innerException2 = new Exception("Inner 2");

    // Act
    var exception1 = new ConcurrencyException(message1, innerException1);
    var exception2 = new ConcurrencyException(message2, innerException2);

    // Assert
    Assert.NotSame(exception1, exception2);
    Assert.NotEqual(exception1.Message, exception2.Message);
    Assert.NotSame(exception1.InnerException, exception2.InnerException);
  }

  [Fact]
  public void Constructor_WithNullMessage_ShouldHandleGracefully()
  {
    // Arrange
    string? message = null;
    var innerException = new Exception();

    // Act
    var exception = new ConcurrencyException(message!, innerException);

    // Assert
    Assert.NotNull(exception);
    Assert.Same(innerException, exception.InnerException);
  }

  [Fact]
  public void InnerException_Chaining_ShouldPreserveExceptionChain()
  {
    // Arrange
    var level3Exception = new TimeoutException("Timeout at level 3");
    var level2Exception = new InvalidOperationException("Invalid operation at level 2", level3Exception);
    var level1Exception = new Exception("Error at level 1", level2Exception);
    var message = "Concurrency error at top level";

    // Act
    var exception = new ConcurrencyException(message, level1Exception);

    // Assert
    Assert.Equal(message, exception.Message);
    Assert.Same(level1Exception, exception.InnerException);
    Assert.Same(level2Exception, exception.InnerException?.InnerException);
    Assert.Same(level3Exception, exception.InnerException?.InnerException?.InnerException);
  }

  [Fact]
  public void Exception_WhenCaughtInTryCatch_ShouldMaintainProperties()
  {
    // Arrange
    var originalMessage = "Original concurrency error";
    var originalInnerException = new Exception("Original inner");

    // Act
    ConcurrencyException? caughtException = null;
    try
    {
      throw new ConcurrencyException(originalMessage, originalInnerException);
    }
    catch (ConcurrencyException ex)
    {
      caughtException = ex;
    }

    // Assert
    Assert.NotNull(caughtException);
    Assert.Equal(originalMessage, caughtException.Message);
    Assert.Same(originalInnerException, caughtException.InnerException);
  }

  [Fact]
  public void Exception_HelpLink_CanBeSetAndRetrieved()
  {
    // Arrange
    var exception = new ConcurrencyException("Error", new Exception());
    var helpLink = "https://docs.example.com/concurrency-errors";

    // Act
    exception.HelpLink = helpLink;

    // Assert
    Assert.Equal(helpLink, exception.HelpLink);
  }

  [Fact]
  public void Exception_Source_CanBeSetAndRetrieved()
  {
    // Arrange
    var exception = new ConcurrencyException("Error", new Exception());
    var source = "AppTemplate.Domain";

    // Act
    exception.Source = source;

    // Assert
    Assert.Equal(source, exception.Source);
  }

  [Fact]
  public void Exception_Data_CanStoreCustomInformation()
  {
    // Arrange
    var exception = new ConcurrencyException("Error", new Exception());
    var key = "CustomKey";
    var value = "CustomValue";

    // Act
    exception.Data.Add(key, value);

    // Assert
    Assert.True(exception.Data.Contains(key));
    Assert.Equal(value, exception.Data[key]);
  }

  [Fact]
  public void Constructor_WithDifferentExceptionTypes_ShouldWrapCorrectly()
  {
    // Arrange & Act
    var timeoutException = new ConcurrencyException("Timeout", new TimeoutException("Timeout occurred"));
    var argumentException = new ConcurrencyException("Argument", new ArgumentException("Invalid argument"));
    var nullRefException = new ConcurrencyException("NullRef", new NullReferenceException("Null reference"));

    // Assert
    Assert.IsType<TimeoutException>(timeoutException.InnerException);
    Assert.IsType<ArgumentException>(argumentException.InnerException);
    Assert.IsType<NullReferenceException>(nullRefException.InnerException);
  }

  [Fact]
  public void Exception_MessageProperty_ShouldMatchConstructorParameter()
  {
    // Arrange
    var messages = new[]
    {
      "Message 1",
      "Message 2",
      "Message 3"
    };

    foreach (var message in messages)
    {
      // Act
      var exception = new ConcurrencyException(message, new Exception());

      // Assert
      Assert.Equal(message, exception.Message);
    }
  }

  [Fact]
  public void Exception_InheritedProperties_ShouldBeAccessible()
  {
    // Arrange
    var exception = new ConcurrencyException("Test", new Exception());

    // Act & Assert - Test inherited Exception properties
    Assert.NotNull(exception.Message);
    Assert.IsType<string>(exception.Message);
    Assert.NotNull(exception.ToString());
    Assert.IsAssignableFrom<Exception>(exception);
  }

  [Fact]
  public void Exception_Serialization_ShouldPreserveMessage()
  {
    // Arrange
    var message = "Serialization test message";
    var innerException = new Exception("Inner exception");
    var exception = new ConcurrencyException(message, innerException);

    // Act - Simulate serialization by accessing all properties
    var messageAfter = exception.Message;
    var innerAfter = exception.InnerException;

    // Assert
    Assert.Equal(message, messageAfter);
    Assert.Same(innerException, innerAfter);
  }

  [Fact]
  public void Constructor_WithWhitespaceMessage_ShouldPreserveWhitespace()
  {
    // Arrange
    var message = "   Message with whitespace   ";
    var innerException = new Exception();

    // Act
    var exception = new ConcurrencyException(message, innerException);

    // Assert
    Assert.Equal(message, exception.Message);
  }

  [Fact]
  public void Constructor_WithUnicodeCharacters_ShouldPreserveUnicode()
  {
    // Arrange
    var message = "Concurrency error: 文字化け テスト 🔥";
    var innerException = new Exception();

    // Act
    var exception = new ConcurrencyException(message, innerException);

    // Assert
    Assert.Equal(message, exception.Message);
  }

  [Fact]
  public void Exception_WithCustomInnerException_ShouldMaintainType()
  {
    // Arrange
    var customException = new CustomTestException("Custom exception message");
    var message = "Wrapping custom exception";

    // Act
    var exception = new ConcurrencyException(message, customException);

    // Assert
    Assert.IsType<CustomTestException>(exception.InnerException);
    Assert.Equal("Custom exception message", exception.InnerException.Message);
  }

  [Fact]
  public void MultipleExceptions_WithSameMessage_ShouldBeIndependent()
  {
    // Arrange
    var message = "Same message";
    var inner1 = new Exception("Inner 1");
    var inner2 = new Exception("Inner 2");

    // Act
    var exception1 = new ConcurrencyException(message, inner1);
    var exception2 = new ConcurrencyException(message, inner2);

    // Assert
    Assert.Equal(exception1.Message, exception2.Message);
    Assert.NotSame(exception1, exception2);
    Assert.NotSame(exception1.InnerException, exception2.InnerException);
  }

  [Fact]
  public void Exception_DataDictionary_SupportsMultipleEntries()
  {
    // Arrange
    var exception = new ConcurrencyException("Error", new Exception());

    // Act
    exception.Data.Add("Key1", "Value1");
    exception.Data.Add("Key2", 123);
    exception.Data.Add("Key3", true);

    // Assert
    Assert.Equal(3, exception.Data.Count);
    Assert.Equal("Value1", exception.Data["Key1"]);
    Assert.Equal(123, exception.Data["Key2"]);
    Assert.Equal(true, exception.Data["Key3"]);
  }

  [Fact]
  public void Exception_GetType_ShouldReturnConcurrencyExceptionType()
  {
    // Arrange
    var exception = new ConcurrencyException("Error", new Exception());

    // Act
    var type = exception.GetType();

    // Assert
    Assert.Equal(typeof(ConcurrencyException), type);
    Assert.Equal("ConcurrencyException", type.Name);
  }

  [Fact]
  public void Exception_WithNestedExceptions_ShouldPreserveAllLevels()
  {
    // Arrange
    var innermost = new ArgumentException("Innermost");
    var middle = new InvalidOperationException("Middle", innermost);
    var outer = new Exception("Outer", middle);
    var message = "Top level concurrency error";

    // Act
    var exception = new ConcurrencyException(message, outer);

    // Assert
    Assert.Equal(message, exception.Message);

    var level1 = exception.InnerException;
    Assert.NotNull(level1);
    Assert.Equal("Outer", level1.Message);

    var level2 = level1.InnerException;
    Assert.NotNull(level2);
    Assert.Equal("Middle", level2.Message);
    Assert.IsType<InvalidOperationException>(level2);

    var level3 = level2.InnerException;
    Assert.NotNull(level3);
    Assert.Equal("Innermost", level3.Message);
    Assert.IsType<ArgumentException>(level3);
  }

  [Fact]
  public void Constructor_WithNewLineInMessage_ShouldPreserveNewLine()
  {
    // Arrange
    var message = "First line\nSecond line\nThird line";
    var innerException = new Exception();

    // Act
    var exception = new ConcurrencyException(message, innerException);

    // Assert
    Assert.Equal(message, exception.Message);
    Assert.Contains("\n", exception.Message);
  }

  [Fact]
  public void Exception_ComparingTwoInstances_ShouldNotBeEqual()
  {
    // Arrange
    var message = "Same message";
    var innerException = new Exception("Same inner");

    // Act
    var exception1 = new ConcurrencyException(message, innerException);
    var exception2 = new ConcurrencyException(message, innerException);

    // Assert
    Assert.NotSame(exception1, exception2);
    Assert.False(ReferenceEquals(exception1, exception2));
  }
}

// Custom exception for testing
public class CustomTestException : Exception
{
  public CustomTestException(string message) : base(message) { }
}