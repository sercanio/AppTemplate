using AppTemplate.Application.Services.ErrorHandling;
using AppTemplate.Web.Controllers;
using MediatR;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Security.Claims;

namespace AppTemplate.Web.Tests.Unit.ControllerTests;

public class SecurityControllerUnitTests
{
  private readonly Mock<ISender> _mockSender;
  private readonly Mock<IErrorHandlingService> _mockErrorHandlingService;
  private readonly Mock<IAntiforgery> _mockAntiforgery;
  private readonly SecurityController _controller;

  public SecurityControllerUnitTests()
  {
    _mockSender = new Mock<ISender>();
    _mockErrorHandlingService = new Mock<IErrorHandlingService>();
    _mockAntiforgery = new Mock<IAntiforgery>();

    _controller = new SecurityController(
        _mockSender.Object,
        _mockErrorHandlingService.Object,
        _mockAntiforgery.Object);

    SetupControllerContext();
  }

  private void SetupControllerContext()
  {
    var httpContext = new DefaultHttpContext();
    httpContext.User = new ClaimsPrincipal(new ClaimsIdentity(new[]
    {
            new Claim(ClaimTypes.NameIdentifier, "test-user-id"),
            new Claim(ClaimTypes.Name, "testuser"),
            new Claim(ClaimTypes.Email, "test@example.com")
        }, "mock"));

    _controller.ControllerContext = new ControllerContext()
    {
      HttpContext = httpContext
    };
  }

  [Fact]
  public void GetToken_WithValidRequest_ReturnsJsonResultWithToken()
  {
    // Arrange
    var expectedToken = "test-antiforgery-token";
    var tokenSet = new TestAntiforgeryTokenSet(expectedToken);

    _mockAntiforgery.Setup(x => x.GetAndStoreTokens(It.IsAny<HttpContext>()))
        .Returns(tokenSet);

    // Act
    var result = _controller.GetToken();

    // Assert
    var jsonResult = Assert.IsType<JsonResult>(result);
    Assert.NotNull(jsonResult.Value);

    // Verify the returned object has the correct structure
    var resultValue = jsonResult.Value;
    var tokenProperty = resultValue?.GetType().GetProperty("token");
    Assert.NotNull(tokenProperty);
    Assert.Equal(expectedToken, tokenProperty.GetValue(resultValue));

    // Verify that the antiforgery service was called with the correct HttpContext
    _mockAntiforgery.Verify(x => x.GetAndStoreTokens(_controller.HttpContext), Times.Once);
  }

  [Fact]
  public void GetToken_WithNullRequestToken_ReturnsJsonResultWithNullToken()
  {
    // Arrange
    var tokenSet = new TestAntiforgeryTokenSet(null);

    _mockAntiforgery.Setup(x => x.GetAndStoreTokens(It.IsAny<HttpContext>()))
        .Returns(tokenSet);

    // Act
    var result = _controller.GetToken();

    // Assert
    var jsonResult = Assert.IsType<JsonResult>(result);
    Assert.NotNull(jsonResult.Value);

    var resultValue = jsonResult.Value;
    var tokenProperty = resultValue?.GetType().GetProperty("token");
    Assert.NotNull(tokenProperty);
    Assert.Null(tokenProperty.GetValue(resultValue));

    _mockAntiforgery.Verify(x => x.GetAndStoreTokens(_controller.HttpContext), Times.Once);
  }

  [Fact]
  public void GetToken_WithEmptyRequestToken_ReturnsJsonResultWithEmptyToken()
  {
    // Arrange
    var expectedToken = string.Empty;
    var tokenSet = new TestAntiforgeryTokenSet(expectedToken);

    _mockAntiforgery.Setup(x => x.GetAndStoreTokens(It.IsAny<HttpContext>()))
        .Returns(tokenSet);

    // Act
    var result = _controller.GetToken();

    // Assert
    var jsonResult = Assert.IsType<JsonResult>(result);
    Assert.NotNull(jsonResult.Value);

    var resultValue = jsonResult.Value;
    var tokenProperty = resultValue?.GetType().GetProperty("token");
    Assert.NotNull(tokenProperty);
    Assert.Equal(expectedToken, tokenProperty.GetValue(resultValue));

    _mockAntiforgery.Verify(x => x.GetAndStoreTokens(_controller.HttpContext), Times.Once);
  }

  [Fact]
  public void GetToken_CallsAntiforgeryServiceWithCorrectHttpContext()
  {
    // Arrange
    var tokenSet = new TestAntiforgeryTokenSet("test-token");

    _mockAntiforgery.Setup(x => x.GetAndStoreTokens(It.IsAny<HttpContext>()))
        .Returns(tokenSet);

    // Act
    _controller.GetToken();

    // Assert
    _mockAntiforgery.Verify(x => x.GetAndStoreTokens(
        It.Is<HttpContext>(ctx => ctx == _controller.HttpContext)), Times.Once);
  }

  [Fact]
  public void GetToken_ReturnsCorrectJsonStructure()
  {
    // Arrange
    var expectedToken = "secure-antiforgery-token-12345";
    var tokenSet = new TestAntiforgeryTokenSet(expectedToken);

    _mockAntiforgery.Setup(x => x.GetAndStoreTokens(It.IsAny<HttpContext>()))
        .Returns(tokenSet);

    // Act
    var result = _controller.GetToken();

    // Assert
    var jsonResult = Assert.IsType<JsonResult>(result);
    Assert.NotNull(jsonResult.Value);

    // Verify the result has the expected anonymous object structure
    var resultValue = jsonResult.Value;
    var properties = resultValue?.GetType().GetProperties();
    Assert.NotNull(properties);
    Assert.Single(properties);
    Assert.Equal("token", properties[0].Name);
    Assert.Equal(expectedToken, properties[0].GetValue(resultValue));
  }

  [Theory]
  [InlineData("simple-token")]
  [InlineData("complex-token-with-special-chars-!@#$%")]
  [InlineData("very-long-token-with-multiple-segments-and-complex-structure-12345-abcdef")]
  public void GetToken_WithVariousTokenFormats_ReturnsCorrectToken(string expectedToken)
  {
    // Arrange
    var tokenSet = new TestAntiforgeryTokenSet(expectedToken);

    _mockAntiforgery.Setup(x => x.GetAndStoreTokens(It.IsAny<HttpContext>()))
        .Returns(tokenSet);

    // Act
    var result = _controller.GetToken();

    // Assert
    var jsonResult = Assert.IsType<JsonResult>(result);
    var resultValue = jsonResult.Value;
    var tokenProperty = resultValue?.GetType().GetProperty("token");
    Assert.Equal(expectedToken, tokenProperty?.GetValue(resultValue));
  }

  [Fact]
  public void GetToken_WithDifferentHttpContexts_CallsAntiforgeryWithCorrectContext()
  {
    // Arrange
    var customHttpContext = new DefaultHttpContext();
    customHttpContext.User = new ClaimsPrincipal(new ClaimsIdentity(new[]
    {
            new Claim(ClaimTypes.NameIdentifier, "different-user-id"),
        }, "mock"));

    _controller.ControllerContext = new ControllerContext()
    {
      HttpContext = customHttpContext
    };

    var tokenSet = new TestAntiforgeryTokenSet("test-token");

    _mockAntiforgery.Setup(x => x.GetAndStoreTokens(It.IsAny<HttpContext>()))
        .Returns(tokenSet);

    // Act
    _controller.GetToken();

    // Assert
    _mockAntiforgery.Verify(x => x.GetAndStoreTokens(
        It.Is<HttpContext>(ctx => ctx == customHttpContext)), Times.Once);
  }

  [Fact]
  public void GetToken_DoesNotCallErrorHandlingService()
  {
    // Arrange
    var tokenSet = new TestAntiforgeryTokenSet("test-token");

    _mockAntiforgery.Setup(x => x.GetAndStoreTokens(It.IsAny<HttpContext>()))
        .Returns(tokenSet);

    // Act
    _controller.GetToken();

    // Assert
    _mockErrorHandlingService.VerifyNoOtherCalls();
  }

  [Fact]
  public void GetToken_DoesNotCallMediatRSender()
  {
    // Arrange
    var tokenSet = new TestAntiforgeryTokenSet("test-token");

    _mockAntiforgery.Setup(x => x.GetAndStoreTokens(It.IsAny<HttpContext>()))
        .Returns(tokenSet);

    // Act
    _controller.GetToken();

    // Assert
    _mockSender.VerifyNoOtherCalls();
  }
}

// Fixed test implementation of AntiforgeryTokenSet
public class TestAntiforgeryTokenSet : AntiforgeryTokenSet
{
  public TestAntiforgeryTokenSet(string requestToken)
      : base(requestToken, "cookie-token", "__RequestVerificationToken", "X-XSRF-TOKEN")
  {
  }
}