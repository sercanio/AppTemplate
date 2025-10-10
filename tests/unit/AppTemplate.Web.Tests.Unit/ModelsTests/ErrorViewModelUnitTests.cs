using AppTemplate.Models;

namespace AppTemplate.Web.Tests.Unit.ModelsTests;

[Trait("Category", "Unit")]
public class ErrorViewModelUnitTests
{
    [Fact]
    public void RequestId_ShouldBeSettable()
    {
        // Arrange
        var viewModel = new ErrorViewModel();
        var requestId = "test-request-id-123";

        // Act
        viewModel.RequestId = requestId;

        // Assert
        Assert.Equal(requestId, viewModel.RequestId);
    }

    [Fact]
    public void RequestId_ShouldAcceptNull()
    {
        // Arrange
        var viewModel = new ErrorViewModel();

        // Act
        viewModel.RequestId = null;

        // Assert
        Assert.Null(viewModel.RequestId);
    }

    [Fact]
    public void RequestId_DefaultValue_ShouldBeNull()
    {
        // Arrange & Act
        var viewModel = new ErrorViewModel();

        // Assert
        Assert.Null(viewModel.RequestId);
    }

    [Fact]
    public void ShowRequestId_WhenRequestIdIsNull_ShouldReturnFalse()
    {
        // Arrange
        var viewModel = new ErrorViewModel
        {
            RequestId = null
        };

        // Act
        var result = viewModel.ShowRequestId;

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void ShowRequestId_WhenRequestIdIsEmpty_ShouldReturnFalse()
    {
        // Arrange
        var viewModel = new ErrorViewModel
        {
            RequestId = string.Empty
        };

        // Act
        var result = viewModel.ShowRequestId;

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void ShowRequestId_WhenRequestIdIsWhitespace_ShouldReturnTrue()
    {
        // Arrange
        var viewModel = new ErrorViewModel
        {
            RequestId = "   "
        };

        // Act
        var result = viewModel.ShowRequestId;

        // Assert
        Assert.True(result); // Changed from False to True
    }

    [Fact]
    public void ShowRequestId_WhenRequestIdIsTab_ShouldReturnTrue()
    {
        // Arrange
        var viewModel = new ErrorViewModel
        {
            RequestId = "\t"
        };

        // Act
        var result = viewModel.ShowRequestId;

        // Assert
        Assert.True(result); // Changed from False to True
    }

    [Fact]
    public void ShowRequestId_WhenRequestIdIsNewline_ShouldReturnTrue()
    {
        // Arrange
        var viewModel = new ErrorViewModel
        {
            RequestId = "\n"
        };

        // Act
        var result = viewModel.ShowRequestId;

        // Assert
        Assert.True(result); // Changed from False to True
    }

    [Fact]
    public void ShowRequestId_WhenRequestIdHasValue_ShouldReturnTrue()
    {
        // Arrange
        var viewModel = new ErrorViewModel
        {
            RequestId = "valid-request-id"
        };

        // Act
        var result = viewModel.ShowRequestId;

        // Assert
        Assert.True(result);
    }

    [Theory]
    [InlineData("simple-id")]
    [InlineData("123456")]
    [InlineData("request-id-with-dashes")]
    [InlineData("RequestId_With_Underscores")]
    [InlineData("a")]
    [InlineData("very-long-request-id-that-could-be-generated-by-system")]
    [InlineData("GUID-LIKE-ID-12345678-1234-1234-1234-123456789ABC")]
    public void ShowRequestId_WithVariousValidRequestIds_ShouldReturnTrue(string requestId)
    {
        // Arrange
        var viewModel = new ErrorViewModel
        {
            RequestId = requestId
        };

        // Act
        var result = viewModel.ShowRequestId;

        // Assert
        Assert.True(result);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void ShowRequestId_WithNullOrEmptyRequestIds_ShouldReturnFalse(string requestId)
    {
        // Arrange
        var viewModel = new ErrorViewModel
        {
            RequestId = requestId
        };

        // Act
        var result = viewModel.ShowRequestId;

        // Assert
        Assert.False(result);
    }

    [Theory]
    [InlineData(" ")]
    [InlineData("  ")]
    [InlineData("\t")]
    [InlineData("\n")]
    [InlineData("\r")]
    [InlineData("\r\n")]
    [InlineData("   \t   ")]
    public void ShowRequestId_WithWhitespaceRequestIds_ShouldReturnTrue(string requestId)
    {
        // Arrange
        var viewModel = new ErrorViewModel
        {
            RequestId = requestId
        };

        // Act
        var result = viewModel.ShowRequestId;

        // Assert
        Assert.True(result); // All whitespace strings return True because string.IsNullOrEmpty returns False for them
    }

    [Fact]
    public void ShowRequestId_WhenRequestIdContainsActualContent_ShouldReturnTrue()
    {
        // Arrange
        var viewModel = new ErrorViewModel
        {
            RequestId = "  actual-content  "
        };

        // Act
        var result = viewModel.ShowRequestId;

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void ErrorViewModel_ShouldBeInstantiableWithObjectInitializer()
    {
        // Arrange & Act
        var viewModel = new ErrorViewModel
        {
            RequestId = "test-id"
        };

        // Assert
        Assert.Equal("test-id", viewModel.RequestId);
        Assert.True(viewModel.ShowRequestId);
    }

    [Fact]
    public void ErrorViewModel_ShouldBeInstantiableWithDefaultConstructor()
    {
        // Arrange & Act
        var viewModel = new ErrorViewModel();

        // Assert
        Assert.Null(viewModel.RequestId);
        Assert.False(viewModel.ShowRequestId);
    }

    [Fact]
    public void RequestId_PropertySetter_ShouldUpdateShowRequestIdAccordingly()
    {
        // Arrange
        var viewModel = new ErrorViewModel();

        // Act & Assert - Initially null
        Assert.Null(viewModel.RequestId);
        Assert.False(viewModel.ShowRequestId);

        // Act & Assert - Set to valid value
        viewModel.RequestId = "valid-id";
        Assert.Equal("valid-id", viewModel.RequestId);
        Assert.True(viewModel.ShowRequestId);

        // Act & Assert - Set back to null
        viewModel.RequestId = null;
        Assert.Null(viewModel.RequestId);
        Assert.False(viewModel.ShowRequestId);

        // Act & Assert - Set to empty
        viewModel.RequestId = "";
        Assert.Equal("", viewModel.RequestId);
        Assert.False(viewModel.ShowRequestId);
    }

    [Fact]
    public void ShowRequestId_ShouldConsistentlyReturnSameValueForSameRequestId()
    {
        // Arrange
        var viewModel = new ErrorViewModel
        {
            RequestId = "consistent-test-id"
        };

        // Act
        var result1 = viewModel.ShowRequestId;
        var result2 = viewModel.ShowRequestId;
        var result3 = viewModel.ShowRequestId;

        // Assert
        Assert.True(result1);
        Assert.True(result2);
        Assert.True(result3);
        Assert.Equal(result1, result2);
        Assert.Equal(result2, result3);
    }

    [Fact]
    public void ErrorViewModel_WithSpecialCharacters_ShouldWorkCorrectly()
    {
        // Arrange
        var specialCharacterIds = new[]
        {
            "id-with-special-chars-!@#$%^&*()",
            "id_with_unicode_🔥_emojis_✨",
            "id/with/slashes",
            "id\\with\\backslashes",
            "id with spaces",
            "id\twith\ttabs",
            "request-id-123-ABC-xyz"
        };

        foreach (var requestId in specialCharacterIds)
        {
            // Act
            var viewModel = new ErrorViewModel { RequestId = requestId };

            // Assert
            Assert.Equal(requestId, viewModel.RequestId);
            Assert.True(viewModel.ShowRequestId);
        }
    }

    [Fact]
    public void ErrorViewModel_PropertyChanges_ShouldNotAffectOtherInstances()
    {
        // Arrange
        var viewModel1 = new ErrorViewModel { RequestId = "id-1" };
        var viewModel2 = new ErrorViewModel { RequestId = "id-2" };

        // Act
        viewModel1.RequestId = "changed-id-1";

        // Assert
        Assert.Equal("changed-id-1", viewModel1.RequestId);
        Assert.Equal("id-2", viewModel2.RequestId);
        Assert.True(viewModel1.ShowRequestId);
        Assert.True(viewModel2.ShowRequestId);
    }

    [Fact]
    public void ShowRequestId_ReflectsStringIsNullOrEmptyBehavior()
    {
        // This test verifies that ShowRequestId correctly implements the logic
        // equivalent to !string.IsNullOrEmpty(RequestId)

        var testCases = new[]
        {
            new { RequestId = (string)null, Expected = false },
            new { RequestId = "", Expected = false },
            new { RequestId = " ", Expected = true }, // string.IsNullOrEmpty returns false for whitespace
            new { RequestId = "\t", Expected = true }, // string.IsNullOrEmpty returns false for whitespace
            new { RequestId = "\n", Expected = true }, // string.IsNullOrEmpty returns false for whitespace
            new { RequestId = "test", Expected = true }
        };

        foreach (var testCase in testCases)
        {
            // Arrange
            var viewModel = new ErrorViewModel { RequestId = testCase.RequestId };

            // Act
            var result = viewModel.ShowRequestId;

            // Assert
            Assert.Equal(testCase.Expected, result);
            Assert.Equal(!string.IsNullOrEmpty(testCase.RequestId), result);
        }
    }
}
