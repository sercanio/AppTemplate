using AppTemplate.Application.Services.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Moq;
using System.Security.Claims;
using System.Threading.Tasks;
using Xunit;
using Microsoft.AspNetCore.Authentication;
using AppTemplate.Application.Services.Statistics;

public class AuthenticationEventsServiceUnitTests
{
    private readonly Mock<IActiveSessionService> _sessionServiceMock;
    private readonly AuthenticationEventsService _service;

    public AuthenticationEventsServiceUnitTests()
    {
        _sessionServiceMock = new Mock<IActiveSessionService>();
        _service = new AuthenticationEventsService(_sessionServiceMock.Object);
    }

    [Fact]
    public async Task OnSignedIn_Calls_RecordUserActivityAsync_WhenAuthenticated()
    {
        var userId = "user-123";
        var claims = new[] { new Claim(ClaimTypes.NameIdentifier, userId) };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var principal = new ClaimsPrincipal(identity);
        var cookieOptions = new CookieAuthenticationOptions();
        var authProperties = new AuthenticationProperties();

        var context = new CookieSignedInContext(
            new DefaultHttpContext(),
            new AuthenticationScheme("Cookies", null, typeof(CookieAuthenticationHandler)),
            principal,
            authProperties,
            cookieOptions
        );

        await _service.OnSignedIn(context);

        _sessionServiceMock.Verify(s => s.RecordUserActivityAsync(userId), Times.Once);
    }

    [Fact]
    public async Task OnSignedIn_DoesNotCall_RecordUserActivityAsync_WhenNotAuthenticated()
    {
        var identity = new ClaimsIdentity(); // Not authenticated
        var principal = new ClaimsPrincipal(identity);
        var authProperties = new AuthenticationProperties();
        var cookieOptions = new CookieAuthenticationOptions();

        var context = new CookieSignedInContext(
            new DefaultHttpContext(),
            new AuthenticationScheme("Cookies", null, typeof(CookieAuthenticationHandler)),
            principal,
            authProperties,
            cookieOptions
        );

        await _service.OnSignedIn(context);

        _sessionServiceMock.Verify(s => s.RecordUserActivityAsync(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task OnSignedIn_DoesNotCall_RecordUserActivityAsync_WhenUserIdMissing()
    {
        var identity = new ClaimsIdentity(new[] { new Claim(ClaimTypes.Name, "test") }, "TestAuth");
        var principal = new ClaimsPrincipal(identity);
        var authProperties = new AuthenticationProperties();
        var cookieOptions = new CookieAuthenticationOptions();

        var context = new CookieSignedInContext(
            new DefaultHttpContext(),
            new AuthenticationScheme("Cookies", null, typeof(CookieAuthenticationHandler)),
            principal,
            authProperties,
            cookieOptions
        );

        await _service.OnSignedIn(context);

        _sessionServiceMock.Verify(s => s.RecordUserActivityAsync(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task OnSignedOut_Calls_RemoveUserSessionAsync_WhenAuthenticated()
    {
        var userId = "user-456";
        var claims = new[] { new Claim(ClaimTypes.NameIdentifier, userId) };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var principal = new ClaimsPrincipal(identity);
        var httpContext = new DefaultHttpContext { User = principal };
        var cookieOptions = new CookieAuthenticationOptions();
        var authProperties = new AuthenticationProperties();

        var context = new CookieSigningOutContext(
            httpContext,
            new AuthenticationScheme("Cookies", null, typeof(CookieAuthenticationHandler)),
            cookieOptions,
            authProperties,
            new CookieOptions()
        );

        await _service.OnSignedOut(context);

        _sessionServiceMock.Verify(s => s.RemoveUserSessionAsync(userId), Times.Once);
    }

    [Fact]
    public async Task OnSignedOut_DoesNotCall_RemoveUserSessionAsync_WhenNotAuthenticated()
    {
        var identity = new ClaimsIdentity(); // Not authenticated
        var principal = new ClaimsPrincipal(identity);
        var httpContext = new DefaultHttpContext { User = principal };
        var cookieOptions = new CookieAuthenticationOptions();
        var authProperties = new AuthenticationProperties();

        var context = new CookieSigningOutContext(
            httpContext,
            new AuthenticationScheme("Cookies", null, typeof(CookieAuthenticationHandler)),
            cookieOptions,
            authProperties,
            new CookieOptions()
        );

        await _service.OnSignedOut(context);

        _sessionServiceMock.Verify(s => s.RemoveUserSessionAsync(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task OnSignedOut_DoesNotCall_RemoveUserSessionAsync_WhenUserIdMissing()
    {
        var identity = new ClaimsIdentity(new[] { new Claim(ClaimTypes.Name, "test") }, "TestAuth");
        var principal = new ClaimsPrincipal(identity);
        var httpContext = new DefaultHttpContext { User = principal };
        var cookieOptions = new CookieAuthenticationOptions();
        var authProperties = new AuthenticationProperties();

        var context = new CookieSigningOutContext(
            httpContext,
            new AuthenticationScheme("Cookies", null, typeof(CookieAuthenticationHandler)),
            cookieOptions,
            authProperties,
            new CookieOptions()
        );

        await _service.OnSignedOut(context);

        _sessionServiceMock.Verify(s => s.RemoveUserSessionAsync(It.IsAny<string>()), Times.Never);
    }
}
