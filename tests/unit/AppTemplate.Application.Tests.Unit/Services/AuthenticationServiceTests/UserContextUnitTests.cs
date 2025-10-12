using AppTemplate.Application.Services.Authentication;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Moq;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using Xunit;

namespace AppTemplate.Application.Tests.Unit.Services.AuthenticationServiceTests;

[Trait("Category", "Unit")]
public class UserContextUnitTests
{
  private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock;
  private readonly UserContext _userContext;
  // Use the claim type that matches what ClaimsPrincipalExtensions.GetUserId() expects
  private const string UserIdClaimType = JwtRegisteredClaimNames.Sub; // or "sub"

  public UserContextUnitTests()
  {
    _httpContextAccessorMock = new Mock<IHttpContextAccessor>();
    _userContext = new UserContext(_httpContextAccessorMock.Object);
  }

  #region Constructor Tests

  [Fact]
  public void Constructor_ShouldInitialize_WithValidDependencies()
  {
    // Arrange & Act
    var userContext = new UserContext(_httpContextAccessorMock.Object);

    // Assert
    userContext.Should().NotBeNull();
  }

  [Fact]
  public void Constructor_ShouldNotThrow_WhenHttpContextAccessorIsNull()
  {
    // Arrange & Act
    Action act = () => new UserContext(null!);

    // Assert
    act.Should().NotThrow();
  }

  #endregion

  #region UserId Property Tests

  [Fact]
  public void UserId_ShouldReturnGuid_WhenUserIdClaimExists()
  {
    // Arrange
    var expectedUserId = Guid.NewGuid();
    var claims = new List<Claim>
        {
            new Claim(UserIdClaimType, expectedUserId.ToString())
        };
    var identity = new ClaimsIdentity(claims, "TestAuth");
    var claimsPrincipal = new ClaimsPrincipal(identity);

    var httpContext = new DefaultHttpContext
    {
      User = claimsPrincipal
    };
    _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(httpContext);

    // Act
    var result = _userContext.UserId;

    // Assert
    result.Should().Be(expectedUserId);
  }

  [Fact]
  public void UserId_ShouldThrowApplicationException_WhenHttpContextIsNull()
  {
    // Arrange
    _httpContextAccessorMock.Setup(x => x.HttpContext).Returns((HttpContext?)null);

    // Act
    Action act = () => _ = _userContext.UserId;

    // Assert
    act.Should().Throw<ApplicationException>()
        .WithMessage("User context is unavailable");
  }

  [Fact]
  public void UserId_ShouldThrowApplicationException_WhenUserIsNull()
  {
    // Arrange
    var httpContext = new DefaultHttpContext
    {
      User = null!
    };
    _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(httpContext);

    // Act
    Action act = () => _ = _userContext.UserId;

    // Assert
    act.Should().Throw<ApplicationException>()
        .WithMessage("User id is unavailable");
  }

  [Fact]
  public void UserId_ShouldThrowApplicationException_WhenUserIdClaimIsMissing()
  {
    // Arrange
    var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, "testuser")
        };
    var identity = new ClaimsIdentity(claims, "TestAuth");
    var claimsPrincipal = new ClaimsPrincipal(identity);

    var httpContext = new DefaultHttpContext
    {
      User = claimsPrincipal
    };
    _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(httpContext);

    // Act
    Action act = () => _ = _userContext.UserId;

    // Assert
    act.Should().Throw<ApplicationException>()
        .WithMessage("User id is unavailable");
  }

  [Fact]
  public void UserId_ShouldThrowApplicationException_WhenUserIdClaimIsInvalid()
  {
    // Arrange
    var claims = new List<Claim>
        {
            new Claim(UserIdClaimType, "invalid-guid")
        };
    var identity = new ClaimsIdentity(claims, "TestAuth");
    var claimsPrincipal = new ClaimsPrincipal(identity);

    var httpContext = new DefaultHttpContext
    {
      User = claimsPrincipal
    };
    _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(httpContext);

    // Act
    Action act = () => _ = _userContext.UserId;

    // Assert
    act.Should().Throw<ApplicationException>()
        .WithMessage("User id is unavailable");
  }

  [Fact]
  public void UserId_ShouldThrowApplicationException_WhenUserIdClaimIsEmpty()
  {
    // Arrange
    var claims = new List<Claim>
        {
            new Claim(UserIdClaimType, string.Empty)
        };
    var identity = new ClaimsIdentity(claims, "TestAuth");
    var claimsPrincipal = new ClaimsPrincipal(identity);

    var httpContext = new DefaultHttpContext
    {
      User = claimsPrincipal
    };
    _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(httpContext);

    // Act
    Action act = () => _ = _userContext.UserId;

    // Assert
    act.Should().Throw<ApplicationException>()
        .WithMessage("User id is unavailable");
  }

  [Fact]
  public void UserId_ShouldReturnSameValue_WhenCalledMultipleTimes()
  {
    // Arrange
    var expectedUserId = Guid.NewGuid();
    var claims = new List<Claim>
        {
            new Claim(UserIdClaimType, expectedUserId.ToString())
        };
    var identity = new ClaimsIdentity(claims, "TestAuth");
    var claimsPrincipal = new ClaimsPrincipal(identity);

    var httpContext = new DefaultHttpContext
    {
      User = claimsPrincipal
    };
    _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(httpContext);

    // Act
    var result1 = _userContext.UserId;
    var result2 = _userContext.UserId;

    // Assert
    result1.Should().Be(expectedUserId);
    result2.Should().Be(expectedUserId);
    result1.Should().Be(result2);
  }

  #endregion

  #region IdentityId Property Tests

  [Fact]
  public void IdentityId_Get_ShouldReturnString_WhenIdentityIdClaimExists()
  {
    // Arrange
    var expectedIdentityId = "user-identity-123";
    var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, expectedIdentityId)
        };
    var identity = new ClaimsIdentity(claims, "TestAuth");
    var claimsPrincipal = new ClaimsPrincipal(identity);

    var httpContext = new DefaultHttpContext
    {
      User = claimsPrincipal
    };
    _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(httpContext);

    // Act
    var result = _userContext.IdentityId;

    // Assert
    result.Should().Be(expectedIdentityId);
  }

  [Fact]
  public void IdentityId_Get_ShouldThrowApplicationException_WhenHttpContextIsNull()
  {
    // Arrange
    _httpContextAccessorMock.Setup(x => x.HttpContext).Returns((HttpContext?)null);

    // Act
    Action act = () => _ = _userContext.IdentityId;

    // Assert
    act.Should().Throw<ApplicationException>()
        .WithMessage("User context is unavailable");
  }

  [Fact]
  public void IdentityId_Get_ShouldThrowApplicationException_WhenUserIsNull()
  {
    // Arrange
    var httpContext = new DefaultHttpContext
    {
      User = null!
    };
    _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(httpContext);

    // Act
    Action act = () => _ = _userContext.IdentityId;

    // Assert
    act.Should().Throw<ApplicationException>()
        .WithMessage("User identity is unavailable");
  }

  [Fact]
  public void IdentityId_Get_ShouldThrowApplicationException_WhenIdentityIdClaimIsMissing()
  {
    // Arrange
    var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, "testuser")
        };
    var identity = new ClaimsIdentity(claims, "TestAuth");
    var claimsPrincipal = new ClaimsPrincipal(identity);

    var httpContext = new DefaultHttpContext
    {
      User = claimsPrincipal
    };
    _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(httpContext);

    // Act
    Action act = () => _ = _userContext.IdentityId;

    // Assert
    act.Should().Throw<ApplicationException>()
        .WithMessage("User identity is unavailable");
  }

  [Fact]
  public void IdentityId_Get_ShouldThrowApplicationException_WhenIdentityIdClaimIsEmpty()
  {
    // Arrange
    var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, string.Empty)
        };
    var identity = new ClaimsIdentity(claims, "TestAuth");
    var claimsPrincipal = new ClaimsPrincipal(identity);

    var httpContext = new DefaultHttpContext
    {
      User = claimsPrincipal
    };
    _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(httpContext);

    // Act
    Action act = () => _ = _userContext.IdentityId;

    // Assert
    act.Should().Throw<ApplicationException>()
        .WithMessage("User identity is unavailable");
  }

  [Fact]
  public void IdentityId_Set_ShouldNotThrow()
  {
    // Arrange
    var identityId = "new-identity-id";

    // Act
    Action act = () => _userContext.IdentityId = identityId;

    // Assert
    act.Should().NotThrow();
  }

  [Fact]
  public void IdentityId_Set_ShouldAcceptNullValue()
  {
    // Arrange & Act
    Action act = () => _userContext.IdentityId = null!;

    // Assert
    act.Should().NotThrow();
  }

  [Fact]
  public void IdentityId_Set_ShouldAcceptEmptyString()
  {
    // Arrange & Act
    Action act = () => _userContext.IdentityId = string.Empty;

    // Assert
    act.Should().NotThrow();
  }

  [Fact]
  public void IdentityId_Get_ShouldReturnSameValue_WhenCalledMultipleTimes()
  {
    // Arrange
    var expectedIdentityId = "user-identity-456";
    var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, expectedIdentityId)
        };
    var identity = new ClaimsIdentity(claims, "TestAuth");
    var claimsPrincipal = new ClaimsPrincipal(identity);

    var httpContext = new DefaultHttpContext
    {
      User = claimsPrincipal
    };
    _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(httpContext);

    // Act
    var result1 = _userContext.IdentityId;
    var result2 = _userContext.IdentityId;

    // Assert
    result1.Should().Be(expectedIdentityId);
    result2.Should().Be(expectedIdentityId);
    result1.Should().Be(result2);
  }

  [Fact]
  public void IdentityId_Get_ShouldThrowApplicationException_WhenClaimContainsWhitespace()
  {
    // Arrange
    var whitespaceId = "   ";
    var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, whitespaceId)
        };
    var identity = new ClaimsIdentity(claims, "TestAuth");
    var claimsPrincipal = new ClaimsPrincipal(identity);

    var httpContext = new DefaultHttpContext
    {
      User = claimsPrincipal
    };
    _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(httpContext);

    // Act
    Action act = () => _ = _userContext.IdentityId;

    // Assert
    act.Should().Throw<ApplicationException>()
        .WithMessage("User identity is unavailable");
  }

  #endregion

  #region Combined Tests

  [Fact]
  public void UserContext_ShouldReturnBothIds_WhenBothClaimsExist()
  {
    // Arrange
    var expectedUserId = Guid.NewGuid();
    var expectedIdentityId = "user-identity-789";
    var claims = new List<Claim>
        {
            new Claim(UserIdClaimType, expectedUserId.ToString()),
            new Claim(ClaimTypes.NameIdentifier, expectedIdentityId)
        };
    var identity = new ClaimsIdentity(claims, "TestAuth");
    var claimsPrincipal = new ClaimsPrincipal(identity);

    var httpContext = new DefaultHttpContext
    {
      User = claimsPrincipal
    };
    _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(httpContext);

    // Act
    var userId = _userContext.UserId;
    var identityId = _userContext.IdentityId;

    // Assert
    userId.Should().Be(expectedUserId);
    identityId.Should().Be(expectedIdentityId);
  }

  [Fact]
  public void UserContext_ShouldThrow_WhenHttpContextAccessorReturnsNull()
  {
    // Arrange
    _httpContextAccessorMock.Setup(x => x.HttpContext).Returns((HttpContext?)null);

    // Act & Assert
    Action actUserId = () => _ = _userContext.UserId;
    Action actIdentityId = () => _ = _userContext.IdentityId;

    actUserId.Should().Throw<ApplicationException>();
    actIdentityId.Should().Throw<ApplicationException>();
  }

  [Fact]
  public void UserContext_ShouldWork_WithAuthenticatedUser()
  {
    // Arrange
    var userId = Guid.NewGuid();
    var identityId = "authenticated-user-id";
    var claims = new List<Claim>
        {
            new Claim(UserIdClaimType, userId.ToString()),
            new Claim(ClaimTypes.NameIdentifier, identityId),
            new Claim(ClaimTypes.Name, "testuser"),
            new Claim(ClaimTypes.Email, "test@example.com")
        };
    var identity = new ClaimsIdentity(claims, "Bearer");
    var claimsPrincipal = new ClaimsPrincipal(identity);

    var httpContext = new DefaultHttpContext
    {
      User = claimsPrincipal
    };
    _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(httpContext);

    // Act
    var resultUserId = _userContext.UserId;
    var resultIdentityId = _userContext.IdentityId;

    // Assert
    resultUserId.Should().Be(userId);
    resultIdentityId.Should().Be(identityId);
  }

  [Fact]
  public void UserContext_ShouldWork_WithMultipleIdentities()
  {
    // Arrange
    var userId = Guid.NewGuid();
    var identityId = "primary-identity";
    var claims1 = new List<Claim>
        {
            new Claim(UserIdClaimType, userId.ToString()),
            new Claim(ClaimTypes.NameIdentifier, identityId)
        };
    var identity1 = new ClaimsIdentity(claims1, "Bearer");

    var claims2 = new List<Claim>
        {
            new Claim(ClaimTypes.Role, "Admin")
        };
    var identity2 = new ClaimsIdentity(claims2, "Cookie");

    var claimsPrincipal = new ClaimsPrincipal(new[] { identity1, identity2 });

    var httpContext = new DefaultHttpContext
    {
      User = claimsPrincipal
    };
    _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(httpContext);

    // Act
    var resultUserId = _userContext.UserId;
    var resultIdentityId = _userContext.IdentityId;

    // Assert
    resultUserId.Should().Be(userId);
    resultIdentityId.Should().Be(identityId);
  }

  #endregion

  #region Edge Cases

  [Fact]
  public void UserId_ShouldThrowApplicationException_WhenUserIdIsGuidEmpty()
  {
    // Arrange
    var claims = new List<Claim>
        {
            new Claim(UserIdClaimType, Guid.Empty.ToString())
        };
    var identity = new ClaimsIdentity(claims, "TestAuth");
    var claimsPrincipal = new ClaimsPrincipal(identity);

    var httpContext = new DefaultHttpContext
    {
      User = claimsPrincipal
    };
    _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(httpContext);

    // Act
    Action act = () => _ = _userContext.UserId;

    // Assert
    act.Should().Throw<ApplicationException>()
        .WithMessage("User id is unavailable");
  }

  [Fact]
  public async Task UserContext_ShouldHandleConcurrentAccess()
  {
    // Arrange
    var userId = Guid.NewGuid();
    var identityId = "concurrent-user";
    var claims = new List<Claim>
        {
            new Claim(UserIdClaimType, userId.ToString()),
            new Claim(ClaimTypes.NameIdentifier, identityId)
        };
    var identity = new ClaimsIdentity(claims, "TestAuth");
    var claimsPrincipal = new ClaimsPrincipal(identity);

    var httpContext = new DefaultHttpContext
    {
      User = claimsPrincipal
    };
    _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(httpContext);

    // Act
    var tasks = Enumerable.Range(0, 10).Select(_ => Task.Run(() =>
    {
      var uid = _userContext.UserId;
      var iid = _userContext.IdentityId;
      return (uid, iid);
    }));

    var results = await Task.WhenAll(tasks);

    // Assert
    results.Should().AllSatisfy(r =>
    {
      r.uid.Should().Be(userId);
      r.iid.Should().Be(identityId);
    });
  }

  #endregion
}