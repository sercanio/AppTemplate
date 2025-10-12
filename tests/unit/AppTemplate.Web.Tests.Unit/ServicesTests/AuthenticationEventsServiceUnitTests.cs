using AppTemplate.Application.Services.Statistics;
using AppTemplate.Web.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Moq;
using System.Security.Claims;

namespace AppTemplate.Web.Tests.Unit.ServicesTests;

[Trait("Category", "Unit")]
public class AuthenticationEventsServiceUnitTests
{
  private readonly Mock<IActiveSessionService> _mockSessionService;
  private readonly AuthenticationEventsService _authenticationEventsService;

  public AuthenticationEventsServiceUnitTests()
  {
    _mockSessionService = new Mock<IActiveSessionService>();
    _authenticationEventsService = new AuthenticationEventsService(_mockSessionService.Object);
  }

  [Fact]
  public async Task OnSignedIn_WithAuthenticatedUserAndValidUserId_ShouldRecordUserActivity()
  {
    // Arrange
    var userId = "test-user-123";
    var context = CreateCookieSignedInContext(userId, isAuthenticated: true);

    _mockSessionService.Setup(x => x.RecordUserActivityAsync(userId)).Returns(Task.CompletedTask);

    // Act
    await _authenticationEventsService.OnSignedIn(context);

    // Assert
    _mockSessionService.Verify(x => x.RecordUserActivityAsync(userId), Times.Once);
  }

  [Fact]
  public async Task OnSignedIn_WithUnauthenticatedUser_ShouldNotRecordActivity()
  {
    // Arrange
    var context = CreateCookieSignedInContext("test-user", isAuthenticated: false);

    // Act
    await _authenticationEventsService.OnSignedIn(context);

    // Assert
    _mockSessionService.Verify(x => x.RecordUserActivityAsync(It.IsAny<string>()), Times.Never);
  }

  [Fact]
  public async Task OnSignedIn_WithNullPrincipal_ShouldNotRecordActivity()
  {
    // Arrange
    var context = CreateCookieSignedInContextWithNullPrincipal();

    // Act
    await _authenticationEventsService.OnSignedIn(context);

    // Assert
    _mockSessionService.Verify(x => x.RecordUserActivityAsync(It.IsAny<string>()), Times.Never);
  }

  [Fact]
  public async Task OnSignedIn_WithNullIdentity_ShouldNotRecordActivity()
  {
    // Arrange
    var context = CreateCookieSignedInContextWithNullIdentity();

    // Act
    await _authenticationEventsService.OnSignedIn(context);

    // Assert
    _mockSessionService.Verify(x => x.RecordUserActivityAsync(It.IsAny<string>()), Times.Never);
  }

  [Fact]
  public async Task OnSignedIn_WithEmptyUserId_ShouldNotRecordActivity()
  {
    // Arrange
    var context = CreateCookieSignedInContext(string.Empty, isAuthenticated: true);

    // Act
    await _authenticationEventsService.OnSignedIn(context);

    // Assert
    _mockSessionService.Verify(x => x.RecordUserActivityAsync(It.IsAny<string>()), Times.Never);
  }

  [Fact]
  public async Task OnSignedIn_WithMissingNameIdentifierClaim_ShouldNotRecordActivity()
  {
    // Arrange
    var context = CreateCookieSignedInContextWithoutNameIdentifierClaim();

    // Act
    await _authenticationEventsService.OnSignedIn(context);

    // Assert
    _mockSessionService.Verify(x => x.RecordUserActivityAsync(It.IsAny<string>()), Times.Never);
  }

  [Theory]
  [InlineData("user-1")]
  [InlineData("12345")]
  [InlineData("user@example.com")]
  [InlineData("GUID-LIKE-ID-123-456-789")]
  [InlineData("user_with_underscores")]
  [InlineData("user-with-dashes")]
  public async Task OnSignedIn_WithVariousUserIdFormats_ShouldRecordActivity(string userId)
  {
    // Arrange
    var context = CreateCookieSignedInContext(userId, isAuthenticated: true);
    _mockSessionService.Setup(x => x.RecordUserActivityAsync(userId)).Returns(Task.CompletedTask);

    // Act
    await _authenticationEventsService.OnSignedIn(context);

    // Assert
    _mockSessionService.Verify(x => x.RecordUserActivityAsync(userId), Times.Once);
  }

  [Fact]
  public async Task OnSignedIn_WhenSessionServiceThrows_ShouldPropagateException()
  {
    // Arrange
    var userId = "test-user-exception";
    var context = CreateCookieSignedInContext(userId, isAuthenticated: true);
    var exception = new InvalidOperationException("Session service error");

    _mockSessionService.Setup(x => x.RecordUserActivityAsync(userId)).ThrowsAsync(exception);

    // Act & Assert
    var thrownException = await Assert.ThrowsAsync<InvalidOperationException>(
        () => _authenticationEventsService.OnSignedIn(context));

    Assert.Equal(exception.Message, thrownException.Message);
    _mockSessionService.Verify(x => x.RecordUserActivityAsync(userId), Times.Once);
  }

  [Fact]
  public async Task OnSignedOut_WithAuthenticatedUserAndValidUserId_ShouldRemoveUserSession()
  {
    // Arrange
    var userId = "test-user-123";
    var context = CreateCookieSigningOutContext(userId, isAuthenticated: true);

    _mockSessionService.Setup(x => x.RemoveUserSessionAsync(userId)).Returns(Task.CompletedTask);

    // Act
    await _authenticationEventsService.OnSignedOut(context);

    // Assert
    _mockSessionService.Verify(x => x.RemoveUserSessionAsync(userId), Times.Once);
  }

  [Fact]
  public async Task OnSignedOut_WithUnauthenticatedUser_ShouldNotRemoveSession()
  {
    // Arrange
    var context = CreateCookieSigningOutContext("test-user", isAuthenticated: false);

    // Act
    await _authenticationEventsService.OnSignedOut(context);

    // Assert
    _mockSessionService.Verify(x => x.RemoveUserSessionAsync(It.IsAny<string>()), Times.Never);
  }

  [Fact]
  public async Task OnSignedOut_WithNullIdentity_ShouldNotRemoveSession()
  {
    // Arrange
    var context = CreateCookieSigningOutContextWithNullIdentity();

    // Act
    await _authenticationEventsService.OnSignedOut(context);

    // Assert
    _mockSessionService.Verify(x => x.RemoveUserSessionAsync(It.IsAny<string>()), Times.Never);
  }

  [Fact]
  public async Task OnSignedOut_WithEmptyUserId_ShouldNotRemoveSession()
  {
    // Arrange
    var context = CreateCookieSigningOutContext(string.Empty, isAuthenticated: true);

    // Act
    await _authenticationEventsService.OnSignedOut(context);

    // Assert
    _mockSessionService.Verify(x => x.RemoveUserSessionAsync(It.IsAny<string>()), Times.Never);
  }

  [Fact]
  public async Task OnSignedOut_WithMissingNameIdentifierClaim_ShouldNotRemoveSession()
  {
    // Arrange
    var context = CreateCookieSigningOutContextWithoutNameIdentifierClaim();

    // Act
    await _authenticationEventsService.OnSignedOut(context);

    // Assert
    _mockSessionService.Verify(x => x.RemoveUserSessionAsync(It.IsAny<string>()), Times.Never);
  }

  [Theory]
  [InlineData("user-1")]
  [InlineData("12345")]
  [InlineData("user@example.com")]
  [InlineData("GUID-LIKE-ID-123-456-789")]
  [InlineData("user_with_underscores")]
  [InlineData("user-with-dashes")]
  public async Task OnSignedOut_WithVariousUserIdFormats_ShouldRemoveSession(string userId)
  {
    // Arrange
    var context = CreateCookieSigningOutContext(userId, isAuthenticated: true);
    _mockSessionService.Setup(x => x.RemoveUserSessionAsync(userId)).Returns(Task.CompletedTask);

    // Act
    await _authenticationEventsService.OnSignedOut(context);

    // Assert
    _mockSessionService.Verify(x => x.RemoveUserSessionAsync(userId), Times.Once);
  }

  [Fact]
  public async Task OnSignedOut_WhenSessionServiceThrows_ShouldPropagateException()
  {
    // Arrange
    var userId = "test-user-exception";
    var context = CreateCookieSigningOutContext(userId, isAuthenticated: true);
    var exception = new InvalidOperationException("Session service error");

    _mockSessionService.Setup(x => x.RemoveUserSessionAsync(userId)).ThrowsAsync(exception);

    // Act & Assert
    var thrownException = await Assert.ThrowsAsync<InvalidOperationException>(
        () => _authenticationEventsService.OnSignedOut(context));

    Assert.Equal(exception.Message, thrownException.Message);
    _mockSessionService.Verify(x => x.RemoveUserSessionAsync(userId), Times.Once);
  }

  [Fact]
  public async Task OnSignedIn_WithMultipleNameIdentifierClaims_ShouldUseFirstValue()
  {
    // Arrange
    var firstUserId = "first-user-id";
    var secondUserId = "second-user-id";
    var context = CreateCookieSignedInContextWithMultipleNameIdentifierClaims(firstUserId, secondUserId);

    _mockSessionService.Setup(x => x.RecordUserActivityAsync(firstUserId)).Returns(Task.CompletedTask);

    // Act
    await _authenticationEventsService.OnSignedIn(context);

    // Assert
    _mockSessionService.Verify(x => x.RecordUserActivityAsync(firstUserId), Times.Once);
    _mockSessionService.Verify(x => x.RecordUserActivityAsync(secondUserId), Times.Never);
  }

  [Fact]
  public async Task OnSignedOut_WithMultipleNameIdentifierClaims_ShouldUseFirstValue()
  {
    // Arrange
    var firstUserId = "first-user-id";
    var secondUserId = "second-user-id";
    var context = CreateCookieSigningOutContextWithMultipleNameIdentifierClaims(firstUserId, secondUserId);

    _mockSessionService.Setup(x => x.RemoveUserSessionAsync(firstUserId)).Returns(Task.CompletedTask);

    // Act
    await _authenticationEventsService.OnSignedOut(context);

    // Assert
    _mockSessionService.Verify(x => x.RemoveUserSessionAsync(firstUserId), Times.Once);
    _mockSessionService.Verify(x => x.RemoveUserSessionAsync(secondUserId), Times.Never);
  }

  [Fact]
  public async Task OnSignedIn_WithLongUserId_ShouldRecordActivity()
  {
    // Arrange
    var longUserId = new string('a', 1000);
    var context = CreateCookieSignedInContext(longUserId, isAuthenticated: true);

    _mockSessionService.Setup(x => x.RecordUserActivityAsync(longUserId)).Returns(Task.CompletedTask);

    // Act
    await _authenticationEventsService.OnSignedIn(context);

    // Assert
    _mockSessionService.Verify(x => x.RecordUserActivityAsync(longUserId), Times.Once);
  }

  [Fact]
  public async Task OnSignedOut_WithLongUserId_ShouldRemoveSession()
  {
    // Arrange
    var longUserId = new string('b', 1000);
    var context = CreateCookieSigningOutContext(longUserId, isAuthenticated: true);

    _mockSessionService.Setup(x => x.RemoveUserSessionAsync(longUserId)).Returns(Task.CompletedTask);

    // Act
    await _authenticationEventsService.OnSignedOut(context);

    // Assert
    _mockSessionService.Verify(x => x.RemoveUserSessionAsync(longUserId), Times.Once);
  }

  [Fact]
  public void Constructor_ShouldInitializeCorrectly()
  {
    // Arrange & Act
    var service = new AuthenticationEventsService(_mockSessionService.Object);

    // Assert - If constructor doesn't throw, initialization is successful
    Assert.NotNull(service);
  }

  [Fact]
  public async Task OnSignedIn_WithSpecialCharactersInUserId_ShouldRecordActivity()
  {
    // Arrange
    var userIdWithSpecialChars = "user@domain.com+123!#$%";
    var context = CreateCookieSignedInContext(userIdWithSpecialChars, isAuthenticated: true);

    _mockSessionService.Setup(x => x.RecordUserActivityAsync(userIdWithSpecialChars)).Returns(Task.CompletedTask);

    // Act
    await _authenticationEventsService.OnSignedIn(context);

    // Assert
    _mockSessionService.Verify(x => x.RecordUserActivityAsync(userIdWithSpecialChars), Times.Once);
  }

  [Fact]
  public async Task OnSignedOut_WithSpecialCharactersInUserId_ShouldRemoveSession()
  {
    // Arrange
    var userIdWithSpecialChars = "user@domain.com+123!#$%";
    var context = CreateCookieSigningOutContext(userIdWithSpecialChars, isAuthenticated: true);

    _mockSessionService.Setup(x => x.RemoveUserSessionAsync(userIdWithSpecialChars)).Returns(Task.CompletedTask);

    // Act
    await _authenticationEventsService.OnSignedOut(context);

    // Assert
    _mockSessionService.Verify(x => x.RemoveUserSessionAsync(userIdWithSpecialChars), Times.Once);
  }

  // Helper methods for creating test contexts
  private static CookieSignedInContext CreateCookieSignedInContext(string userId, bool isAuthenticated)
  {
    var httpContext = new DefaultHttpContext();
    var scheme = new AuthenticationScheme("Cookies", "Cookies", typeof(CookieAuthenticationHandler));
    var options = new CookieAuthenticationOptions();
    var properties = new AuthenticationProperties();

    var claims = new List<Claim>();
    if (!string.IsNullOrEmpty(userId))
    {
      claims.Add(new Claim(ClaimTypes.NameIdentifier, userId));
    }
    claims.Add(new Claim(ClaimTypes.Name, "Test User"));

    var identity = new ClaimsIdentity(claims, isAuthenticated ? "test" : null);
    var principal = new ClaimsPrincipal(identity);

    // Correct constructor: (HttpContext, AuthenticationScheme, ClaimsPrincipal, AuthenticationProperties, CookieAuthenticationOptions)
    return new CookieSignedInContext(httpContext, scheme, principal, properties, options);
  }

  private static CookieSignedInContext CreateCookieSignedInContextWithNullPrincipal()
  {
    var httpContext = new DefaultHttpContext();
    var scheme = new AuthenticationScheme("Cookies", "Cookies", typeof(CookieAuthenticationHandler));
    var options = new CookieAuthenticationOptions();
    var properties = new AuthenticationProperties();

    return new CookieSignedInContext(httpContext, scheme, null, properties, options);
  }

  private static CookieSignedInContext CreateCookieSignedInContextWithNullIdentity()
  {
    var httpContext = new DefaultHttpContext();
    var scheme = new AuthenticationScheme("Cookies", "Cookies", typeof(CookieAuthenticationHandler));
    var options = new CookieAuthenticationOptions();
    var properties = new AuthenticationProperties();
    var principal = new ClaimsPrincipal();

    return new CookieSignedInContext(httpContext, scheme, principal, properties, options);
  }

  private static CookieSignedInContext CreateCookieSignedInContextWithoutNameIdentifierClaim()
  {
    var httpContext = new DefaultHttpContext();
    var scheme = new AuthenticationScheme("Cookies", "Cookies", typeof(CookieAuthenticationHandler));
    var options = new CookieAuthenticationOptions();
    var properties = new AuthenticationProperties();

    var claims = new List<Claim>
        {
            new(ClaimTypes.Name, "Test User"),
            new(ClaimTypes.Email, "test@example.com")
        };

    var identity = new ClaimsIdentity(claims, "test");
    var principal = new ClaimsPrincipal(identity);

    return new CookieSignedInContext(httpContext, scheme, principal, properties, options);
  }

  private static CookieSignedInContext CreateCookieSignedInContextWithMultipleNameIdentifierClaims(string firstUserId, string secondUserId)
  {
    var httpContext = new DefaultHttpContext();
    var scheme = new AuthenticationScheme("Cookies", "Cookies", typeof(CookieAuthenticationHandler));
    var options = new CookieAuthenticationOptions();
    var properties = new AuthenticationProperties();

    var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, firstUserId),
            new(ClaimTypes.NameIdentifier, secondUserId),
            new(ClaimTypes.Name, "Test User")
        };

    var identity = new ClaimsIdentity(claims, "test");
    var principal = new ClaimsPrincipal(identity);

    return new CookieSignedInContext(httpContext, scheme, principal, properties, options);
  }

  private static CookieSigningOutContext CreateCookieSigningOutContext(string userId, bool isAuthenticated)
  {
    var httpContext = new DefaultHttpContext();
    var scheme = new AuthenticationScheme("Cookies", "Cookies", typeof(CookieAuthenticationHandler));
    var options = new CookieAuthenticationOptions();
    var properties = new AuthenticationProperties();
    var cookieOptions = new CookieOptions();

    var claims = new List<Claim>();
    if (!string.IsNullOrEmpty(userId))
    {
      claims.Add(new Claim(ClaimTypes.NameIdentifier, userId));
    }
    claims.Add(new Claim(ClaimTypes.Name, "Test User"));

    var identity = new ClaimsIdentity(claims, isAuthenticated ? "test" : null);
    var user = new ClaimsPrincipal(identity);
    httpContext.User = user;

    return new CookieSigningOutContext(httpContext, scheme, options, properties, cookieOptions);
  }

  private static CookieSigningOutContext CreateCookieSigningOutContextWithNullIdentity()
  {
    var httpContext = new DefaultHttpContext();
    var scheme = new AuthenticationScheme("Cookies", "Cookies", typeof(CookieAuthenticationHandler));
    var options = new CookieAuthenticationOptions();
    var properties = new AuthenticationProperties();
    var cookieOptions = new CookieOptions();

    httpContext.User = new ClaimsPrincipal();
    return new CookieSigningOutContext(httpContext, scheme, options, properties, cookieOptions);
  }

  private static CookieSigningOutContext CreateCookieSigningOutContextWithoutNameIdentifierClaim()
  {
    var httpContext = new DefaultHttpContext();
    var scheme = new AuthenticationScheme("Cookies", "Cookies", typeof(CookieAuthenticationHandler));
    var options = new CookieAuthenticationOptions();
    var properties = new AuthenticationProperties();
    var cookieOptions = new CookieOptions();

    var claims = new List<Claim>
        {
            new(ClaimTypes.Name, "Test User"),
            new(ClaimTypes.Email, "test@example.com")
        };

    var identity = new ClaimsIdentity(claims, "test");
    var user = new ClaimsPrincipal(identity);
    httpContext.User = user;

    return new CookieSigningOutContext(httpContext, scheme, options, properties, cookieOptions);
  }

  private static CookieSigningOutContext CreateCookieSigningOutContextWithMultipleNameIdentifierClaims(string firstUserId, string secondUserId)
  {
    var httpContext = new DefaultHttpContext();
    var scheme = new AuthenticationScheme("Cookies", "Cookies", typeof(CookieAuthenticationHandler));
    var options = new CookieAuthenticationOptions();
    var properties = new AuthenticationProperties();
    var cookieOptions = new CookieOptions();

    var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, firstUserId),
            new(ClaimTypes.NameIdentifier, secondUserId),
            new(ClaimTypes.Name, "Test User")
        };

    var identity = new ClaimsIdentity(claims, "test");
    var user = new ClaimsPrincipal(identity);
    httpContext.User = user;

    return new CookieSigningOutContext(httpContext, scheme, options, properties, cookieOptions);
  }
}