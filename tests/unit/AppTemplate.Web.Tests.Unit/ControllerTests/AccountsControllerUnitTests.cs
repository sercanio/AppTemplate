using AppTemplate.Application.Authentication;
using AppTemplate.Application.Authentication.Models;
using AppTemplate.Application.Features.Accounts.UpdateNotificationPreferences;
using AppTemplate.Application.Services.AppUsers;
using AppTemplate.Application.Services.EmailSenders;
using AppTemplate.Application.Services.ErrorHandling;
using AppTemplate.Domain;
using AppTemplate.Domain.AppUsers;
using AppTemplate.Domain.AppUsers.ValueObjects;
using AppTemplate.Web.Controllers;
using Ardalis.Result;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using Moq;
using System.Security.Claims;
using System.Text;
using SignInResult = Microsoft.AspNetCore.Identity.SignInResult;

namespace AppTemplate.Web.Tests.Unit.ControllerTests;

public class AccountsControllerUnitTests
{
  private readonly Mock<UserManager<IdentityUser>> _mockUserManager;
  private readonly Mock<SignInManager<IdentityUser>> _mockSignInManager;
  private readonly Mock<IAppUsersService> _mockAppUsersService;
  private readonly Mock<IUnitOfWork> _mockUnitOfWork;
  private readonly Mock<IAccountEmailService> _mockAccountEmailService;
  private readonly Mock<ISender> _mockSender;
  private readonly Mock<IErrorHandlingService> _mockErrorHandlingService;
  private readonly Mock<IJwtTokenService> _mockJwtTokenService;
  private readonly Mock<IConfiguration> _mockConfiguration;
  private readonly AccountsController _controller;

  public AccountsControllerUnitTests()
  {
    _mockUserManager = CreateMockUserManager();
    _mockSignInManager = CreateMockSignInManager();
    _mockAppUsersService = new Mock<IAppUsersService>();
    _mockUnitOfWork = new Mock<IUnitOfWork>();
    _mockAccountEmailService = new Mock<IAccountEmailService>();
    _mockSender = new Mock<ISender>();
    _mockErrorHandlingService = new Mock<IErrorHandlingService>();
    _mockJwtTokenService = new Mock<IJwtTokenService>();
    _mockConfiguration = new Mock<IConfiguration>();

    // Setup configuration defaults
    SetupConfiguration();

    _controller = new AccountsController(
        _mockUserManager.Object,
        _mockSignInManager.Object,
        _mockAppUsersService.Object,
        _mockUnitOfWork.Object,
        _mockAccountEmailService.Object,
        _mockJwtTokenService.Object,
        _mockConfiguration.Object,
        _mockSender.Object,
        _mockErrorHandlingService.Object);

    SetupControllerContext();
  }

  private void SetupConfiguration()
  {
    _mockConfiguration.Setup(x => x["Authentication:Cookie:SameSite"]).Returns("Strict");
    var mockSection = new Mock<IConfigurationSection>();
    mockSection.Setup(x => x.Value).Returns("true");
    _mockConfiguration.Setup(x => x.GetSection("Authentication:Cookie:Secure")).Returns(mockSection.Object); _mockConfiguration.Setup(x => x["Jwt:RememberMeTokenExpiryInDays"]).Returns("30");
  }

  private static Mock<UserManager<IdentityUser>> CreateMockUserManager()
  {
    var store = new Mock<IUserStore<IdentityUser>>();
    var mgr = new Mock<UserManager<IdentityUser>>(
        store.Object, null, null, null, null, null, null, null, null);
    mgr.Object.UserValidators.Add(new UserValidator<IdentityUser>());
    mgr.Object.PasswordValidators.Add(new PasswordValidator<IdentityUser>());
    return mgr;
  }

  private Mock<SignInManager<IdentityUser>> CreateMockSignInManager()
  {
    var contextAccessor = new Mock<IHttpContextAccessor>();
    var userPrincipalFactory = new Mock<IUserClaimsPrincipalFactory<IdentityUser>>();
    var options = new Mock<IOptions<IdentityOptions>>();
    var logger = new Mock<ILogger<SignInManager<IdentityUser>>>();

    return new Mock<SignInManager<IdentityUser>>(
        _mockUserManager.Object,
        contextAccessor.Object,
        userPrincipalFactory.Object,
        options.Object,
        logger.Object,
        null, null);
  }

  private void SetupControllerContext()
  {
    var httpContext = new DefaultHttpContext();
    httpContext.User = new ClaimsPrincipal(new ClaimsIdentity(new[]
    {
            new Claim(ClaimTypes.NameIdentifier, "test-user-id"),
            new Claim(ClaimTypes.Name, "testuser"),
            new Claim(ClaimTypes.Email, "test@example.com"),
            new Claim("sub", "test-user-id"),
            new Claim(JwtRegisteredClaimNames.Jti, "test-jti")
        }, "mock"));

    // Setup request cookies for refresh token tests using a mock
    var mockCookies = new Mock<IRequestCookieCollection>();
    mockCookies.Setup(x => x.TryGetValue("session", out It.Ref<string>.IsAny))
        .Returns((string key, out string value) =>
        {
          value = "valid-refresh-token";
          return true;
        });
    mockCookies.Setup(x => x["session"]).Returns("valid-refresh-token");

    httpContext.Request.Cookies = mockCookies.Object;

    _controller.ControllerContext = new ControllerContext()
    {
      HttpContext = httpContext
    };
  }

  #region ConfirmEmail Tests

  [Fact]
  public async Task ConfirmEmail_WithValidParameters_ReturnsOkResult()
  {
    // Arrange
    var userId = "test-user-id";
    var code = "test-code";
    var encodedCode = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
    var user = new IdentityUser { Id = userId, Email = "test@example.com" };

    _mockUserManager.Setup(x => x.FindByIdAsync(userId))
        .ReturnsAsync(user);
    _mockUserManager.Setup(x => x.ConfirmEmailAsync(user, code))
        .ReturnsAsync(IdentityResult.Success);

    // Act
    var result = await _controller.ConfirmEmail(userId, encodedCode);

    // Assert
    var okResult = Assert.IsType<OkObjectResult>(result);
    var response = okResult.Value;
    Assert.NotNull(response);
  }

  [Fact]
  public async Task ConfirmEmail_WithInvalidUserId_ReturnsErrorResponse()
  {
    // Arrange
    var userId = "invalid-user-id";
    var code = "test-code";
    var encodedCode = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));

    _mockUserManager.Setup(x => x.FindByIdAsync(userId))
        .ReturnsAsync((IdentityUser)null);

    var errorResult = new BadRequestObjectResult("User not found");
    _mockErrorHandlingService.Setup(x => x.HandleErrorResponse(It.IsAny<Result>()))
        .Returns(errorResult);

    // Act
    var result = await _controller.ConfirmEmail(userId, encodedCode);

    // Assert
    Assert.IsType<BadRequestObjectResult>(result);
    _mockErrorHandlingService.Verify(x => x.HandleErrorResponse(It.IsAny<Result>()), Times.Once);
  }

  [Theory]
  [InlineData(null, "valid-code")]
  [InlineData("", "valid-code")]
  [InlineData("valid-user-id", null)]
  [InlineData("valid-user-id", "")]
  public async Task ConfirmEmail_WithInvalidParameters_ReturnsErrorResponse(string userId, string code)
  {
    // Arrange
    var errorResult = new BadRequestObjectResult("Invalid parameters");
    _mockErrorHandlingService.Setup(x => x.HandleErrorResponse(It.IsAny<Result>()))
        .Returns(errorResult);

    // Act
    var result = await _controller.ConfirmEmail(userId, code);

    // Assert
    Assert.IsType<BadRequestObjectResult>(result);
    _mockErrorHandlingService.Verify(x => x.HandleErrorResponse(It.IsAny<Result>()), Times.Once);
  }

  [Fact]
  public async Task ConfirmEmail_WithFailedIdentityResult_ReturnsErrorResponse()
  {
    // Arrange
    var userId = "test-user-id";
    var code = "test-code";
    var encodedCode = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
    var user = new IdentityUser { Id = userId, Email = "test@example.com" };

    _mockUserManager.Setup(x => x.FindByIdAsync(userId))
        .ReturnsAsync(user);
    _mockUserManager.Setup(x => x.ConfirmEmailAsync(user, code))
        .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Email confirmation failed" }));

    var errorResult = new BadRequestObjectResult("Email confirmation failed");
    _mockErrorHandlingService.Setup(x => x.HandleErrorResponse(It.IsAny<Result<string>>()))
        .Returns(errorResult);

    // Act
    var result = await _controller.ConfirmEmail(userId, encodedCode);

    // Assert
    Assert.IsType<BadRequestObjectResult>(result);
    _mockErrorHandlingService.Verify(x => x.HandleErrorResponse(It.IsAny<Result<string>>()), Times.Once);
  }

  #endregion

  #region ChangeEmail Tests

  [Fact]
  public async Task ChangeEmail_WithValidRequest_SendsEmailAndReturnsOk()
  {
    // Arrange
    var request = new ChangeEmailRequest { NewEmail = "newemail@example.com" };
    var user = new IdentityUser { Id = "test-user-id", Email = "old@example.com", UserName = "testuser" };

    _mockUserManager.Setup(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
        .ReturnsAsync(user);
    _mockUserManager.Setup(x => x.GetEmailAsync(user))
        .ReturnsAsync(user.Email);
    _mockUserManager.Setup(x => x.FindByEmailAsync(request.NewEmail))
        .ReturnsAsync((IdentityUser)null);
    _mockUserManager.Setup(x => x.GetUserIdAsync(user))
        .ReturnsAsync(user.Id);
    _mockUserManager.Setup(x => x.GenerateChangeEmailTokenAsync(user, request.NewEmail))
        .ReturnsAsync("change-email-token");

    // Act
    var result = await _controller.ChangeEmail(request);

    // Assert
    var okResult = Assert.IsType<OkObjectResult>(result);
    Assert.NotNull(okResult.Value);
    _mockAccountEmailService.Verify(x => x.SendEmailChangeConfirmationAsync(
        request.NewEmail, user.Id, It.IsAny<string>(), user.UserName), Times.Once);
  }

  [Fact]
  public async Task ChangeEmail_WithNullUser_ReturnsNotFound()
  {
    // Arrange
    var request = new ChangeEmailRequest { NewEmail = "newemail@example.com" };

    _mockUserManager.Setup(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
        .ReturnsAsync((IdentityUser)null);

    // Act
    var result = await _controller.ChangeEmail(request);

    // Assert
    Assert.IsType<NotFoundObjectResult>(result);
  }

  [Fact]
  public async Task ChangeEmail_WithSameEmail_ReturnsOk()
  {
    // Arrange
    var request = new ChangeEmailRequest { NewEmail = "test@example.com" };
    var user = new IdentityUser { Id = "test-user-id", Email = "test@example.com" };

    _mockUserManager.Setup(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
        .ReturnsAsync(user);
    _mockUserManager.Setup(x => x.GetEmailAsync(user))
        .ReturnsAsync(user.Email);

    // Act
    var result = await _controller.ChangeEmail(request);

    // Assert
    var okResult = Assert.IsType<OkObjectResult>(result);
    Assert.NotNull(okResult.Value);
  }

  [Fact]
  public async Task ChangeEmail_WithEmailAlreadyInUse_ReturnsBadRequest()
  {
    // Arrange
    var request = new ChangeEmailRequest { NewEmail = "existing@example.com" };
    var user = new IdentityUser { Id = "test-user-id", Email = "old@example.com" };
    var existingUser = new IdentityUser { Id = "other-user-id", Email = "existing@example.com" };

    _mockUserManager.Setup(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
        .ReturnsAsync(user);
    _mockUserManager.Setup(x => x.GetEmailAsync(user))
        .ReturnsAsync(user.Email);
    _mockUserManager.Setup(x => x.FindByEmailAsync(request.NewEmail))
        .ReturnsAsync(existingUser);

    // Act
    var result = await _controller.ChangeEmail(request);

    // Assert
    Assert.IsType<BadRequestObjectResult>(result);
  }

  #endregion

  #region SendVerificationEmail Tests

  [Fact]
  public async Task SendVerificationEmail_WithValidUser_SendsEmailAndReturnsOk()
  {
    // Arrange
    var user = new IdentityUser { Id = "test-user-id", Email = "test@example.com", UserName = "testuser" };

    _mockUserManager.Setup(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
        .ReturnsAsync(user);
    _mockUserManager.Setup(x => x.IsEmailConfirmedAsync(user))
        .ReturnsAsync(false);
    _mockUserManager.Setup(x => x.GetUserIdAsync(user))
        .ReturnsAsync(user.Id);
    _mockUserManager.Setup(x => x.GetEmailAsync(user))
        .ReturnsAsync(user.Email);
    _mockUserManager.Setup(x => x.GenerateEmailConfirmationTokenAsync(user))
        .ReturnsAsync("confirmation-token");

    // Act
    var result = await _controller.SendVerificationEmail();

    // Assert
    var okResult = Assert.IsType<OkObjectResult>(result);
    Assert.NotNull(okResult.Value);
    _mockAccountEmailService.Verify(x => x.SendConfirmationEmailAsync(
        user.Email, user.Id, It.IsAny<string>(), user.UserName), Times.Once);
  }

  [Fact]
  public async Task SendVerificationEmail_WithNullUser_ReturnsNotFound()
  {
    // Arrange
    _mockUserManager.Setup(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
        .ReturnsAsync((IdentityUser)null);

    // Act
    var result = await _controller.SendVerificationEmail();

    // Assert
    Assert.IsType<NotFoundObjectResult>(result);
  }

  [Fact]
  public async Task SendVerificationEmail_WithConfirmedEmail_ReturnsOk()
  {
    // Arrange
    var user = new IdentityUser { Id = "test-user-id", Email = "test@example.com" };

    _mockUserManager.Setup(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
        .ReturnsAsync(user);
    _mockUserManager.Setup(x => x.IsEmailConfirmedAsync(user))
        .ReturnsAsync(true);

    // Act
    var result = await _controller.SendVerificationEmail();

    // Assert
    var okResult = Assert.IsType<OkObjectResult>(result);
    Assert.NotNull(okResult.Value);
    _mockAccountEmailService.Verify(x => x.SendConfirmationEmailAsync(
        It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
  }

  #endregion

  #region ResendEmailConfirmation Tests

  [Fact]
  public async Task ResendEmailConfirmation_WithValidEmail_SendsEmailAndReturnsOk()
  {
    // Arrange
    var request = new ResendEmailConfirmationRequest { Email = "test@example.com" };
    var user = new IdentityUser { Id = "test-user-id", Email = "test@example.com", UserName = "testuser" };

    _mockUserManager.Setup(x => x.FindByEmailAsync(request.Email))
        .ReturnsAsync(user);
    _mockUserManager.Setup(x => x.GetUserIdAsync(user))
        .ReturnsAsync(user.Id);
    _mockUserManager.Setup(x => x.GenerateEmailConfirmationTokenAsync(user))
        .ReturnsAsync("confirmation-token");

    // Act
    var result = await _controller.ResendEmailConfirmation(request);

    // Assert
    var okResult = Assert.IsType<OkObjectResult>(result);
    Assert.NotNull(okResult.Value);
    _mockAccountEmailService.Verify(x => x.SendConfirmationEmailAsync(
        request.Email, user.Id, It.IsAny<string>(), user.UserName), Times.Once);
  }

  [Fact]
  public async Task ResendEmailConfirmation_WithEmptyEmail_ReturnsBadRequest()
  {
    // Arrange
    var request = new ResendEmailConfirmationRequest { Email = "" };

    // Act
    var result = await _controller.ResendEmailConfirmation(request);

    // Assert
    Assert.IsType<BadRequestObjectResult>(result);
  }

  [Fact]
  public async Task ResendEmailConfirmation_WithNonExistentUser_ReturnsOkForSecurity()
  {
    // Arrange
    var request = new ResendEmailConfirmationRequest { Email = "nonexistent@example.com" };

    _mockUserManager.Setup(x => x.FindByEmailAsync(request.Email))
        .ReturnsAsync((IdentityUser)null);

    // Act
    var result = await _controller.ResendEmailConfirmation(request);

    // Assert
    var okResult = Assert.IsType<OkObjectResult>(result);
    Assert.NotNull(okResult.Value);
    _mockAccountEmailService.Verify(x => x.SendConfirmationEmailAsync(
        It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
  }

  [Fact]
  public async Task ResendEmailConfirmation_WithNullEmail_ReturnsBadRequest()
  {
    // Arrange
    var request = new ResendEmailConfirmationRequest { Email = null };

    // Act
    var result = await _controller.ResendEmailConfirmation(request);

    // Assert
    Assert.IsType<BadRequestObjectResult>(result);
  }

  #endregion

  #region ConfirmEmailChange Tests

  [Fact]
  public async Task ConfirmEmailChange_WithValidParameters_ReturnsOk()
  {
    // Arrange
    var userId = "test-user-id";
    var email = "newemail@example.com";
    var code = "test-code";
    var encodedCode = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
    var user = new IdentityUser { Id = userId, Email = "old@example.com" };

    _mockUserManager.Setup(x => x.FindByIdAsync(userId))
        .ReturnsAsync(user);
    _mockUserManager.Setup(x => x.ChangeEmailAsync(user, email, code))
        .ReturnsAsync(IdentityResult.Success);

    // Act
    var result = await _controller.ConfirmEmailChange(userId, email, encodedCode);

    // Assert
    var okResult = Assert.IsType<OkObjectResult>(result);
    Assert.NotNull(okResult.Value);
  }

  [Fact]
  public async Task ConfirmEmailChange_WithMissingParameters_ReturnsBadRequest()
  {
    // Act
    var result = await _controller.ConfirmEmailChange("", "email@test.com", "code");

    // Assert
    Assert.IsType<BadRequestObjectResult>(result);
  }

  [Fact]
  public async Task ConfirmEmailChange_WithNonExistentUser_ReturnsNotFound()
  {
    // Arrange
    _mockUserManager.Setup(x => x.FindByIdAsync(It.IsAny<string>()))
        .ReturnsAsync((IdentityUser)null);

    // Act
    var result = await _controller.ConfirmEmailChange("user-id", "email@test.com", "code");

    // Assert
    Assert.IsType<NotFoundObjectResult>(result);
  }

  [Fact]
  public async Task ConfirmEmailChange_WithFailedEmailChange_ReturnsBadRequest()
  {
    // Arrange
    var userId = "test-user-id";
    var email = "newemail@example.com";
    var code = "test-code";
    var encodedCode = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
    var user = new IdentityUser { Id = userId, Email = "old@example.com" };

    _mockUserManager.Setup(x => x.FindByIdAsync(userId))
        .ReturnsAsync(user);
    _mockUserManager.Setup(x => x.ChangeEmailAsync(user, email, code))
        .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Email change failed" }));

    // Act
    var result = await _controller.ConfirmEmailChange(userId, email, encodedCode);

    // Assert
    Assert.IsType<BadRequestObjectResult>(result);
  }

  #endregion

  #region Login Tests

  [Fact]
  public async Task LoginWithJwt_WithValidCredentials_ReturnsTokens()
  {
    // Arrange
    var request = new JwtLoginRequest
    {
      LoginIdentifier = "test@example.com",
      Password = "ValidPassword123!"
    };
    var user = new IdentityUser { Id = "user-id", Email = "test@example.com", UserName = "testuser" };
    var appUser = AppUser.Create();
    appUser.SetIdentityId(user.Id);

    var tokens = new JwtTokenResult(
        "access-token",
        "refresh-token",
        DateTime.UtcNow.AddHours(1),
        "Bearer"
    );

    _mockUserManager.Setup(x => x.FindByEmailAsync(request.LoginIdentifier)).ReturnsAsync(user);
    _mockUserManager.Setup(x => x.FindByNameAsync(request.LoginIdentifier)).ReturnsAsync((IdentityUser)null);
    _mockSignInManager.Setup(x => x.CheckPasswordSignInAsync(user, request.Password, false)).ReturnsAsync(SignInResult.Success);
    _mockUserManager.Setup(x => x.CheckPasswordAsync(user, request.Password)).ReturnsAsync(true);
    _mockUserManager.Setup(x => x.IsEmailConfirmedAsync(user)).ReturnsAsync(true);
    _mockAppUsersService.Setup(x => x.GetByIdentityIdAsync(user.Id, default)).ReturnsAsync(Result.Success(appUser));
    _mockUserManager.Setup(x => x.GetTwoFactorEnabledAsync(user)).ReturnsAsync(false);
    _mockJwtTokenService.Setup(x => x.GenerateTokensAsync(
        It.Is<IdentityUser>(u => u == user),
        It.Is<AppUser>(a => a == appUser),
        It.IsAny<DeviceInfo>())).ReturnsAsync(tokens);

    // Act
    var result = await _controller.LoginWithJwt(request);

    // Assert
    var okResult = Assert.IsType<OkObjectResult>(result);
    Assert.NotNull(okResult.Value);
  }

  [Fact]
  public async Task LoginWithJwt_WithInvalidCredentials_ReturnsBadRequest()
  {
    // Arrange
    var request = new JwtLoginRequest
    {
      LoginIdentifier = "test@example.com",
      Password = "WrongPassword"
    };

    _mockUserManager.Setup(x => x.FindByEmailAsync(request.LoginIdentifier)).ReturnsAsync((IdentityUser)null);
    _mockUserManager.Setup(x => x.FindByNameAsync(request.LoginIdentifier)).ReturnsAsync((IdentityUser)null);

    // Act
    var result = await _controller.LoginWithJwt(request);

    // Assert
    var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
    Assert.NotNull(badRequestResult.Value);
  }

  [Fact]
  public async Task LoginWithJwt_WithUnconfirmedEmail_ReturnsBadRequest()
  {
    // Arrange
    var request = new JwtLoginRequest
    {
      LoginIdentifier = "test@example.com",
      Password = "ValidPassword123!"
    };
    var user = new IdentityUser { Id = "user-id", Email = "test@example.com", UserName = "testuser" };

    _mockUserManager.Setup(x => x.FindByEmailAsync(request.LoginIdentifier)).ReturnsAsync(user);
    _mockSignInManager.Setup(x => x.CheckPasswordSignInAsync(user, request.Password, false)).ReturnsAsync(SignInResult.Success);
    _mockUserManager.Setup(x => x.CheckPasswordAsync(user, request.Password)).ReturnsAsync(true);
    _mockUserManager.Setup(x => x.IsEmailConfirmedAsync(user)).ReturnsAsync(false);

    // Act
    var result = await _controller.LoginWithJwt(request);

    // Assert
    var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
    Assert.NotNull(badRequestResult.Value);
  }

  [Fact]
  public async Task LoginWithJwt_WithTwoFactorEnabled_ReturnsTwoFactorRequired()
  {
    // Arrange
    var request = new JwtLoginRequest
    {
      LoginIdentifier = "test@example.com",
      Password = "ValidPassword123!"
    };
    var user = new IdentityUser { Id = "user-id", Email = "test@example.com", UserName = "testuser" };
    var appUser = AppUser.Create();
    appUser.SetIdentityId(user.Id);

    _mockUserManager.Setup(x => x.FindByEmailAsync(request.LoginIdentifier)).ReturnsAsync(user);
    _mockSignInManager.Setup(x => x.CheckPasswordSignInAsync(user, request.Password, false)).ReturnsAsync(SignInResult.Success);
    _mockUserManager.Setup(x => x.CheckPasswordAsync(user, request.Password)).ReturnsAsync(true);
    _mockUserManager.Setup(x => x.IsEmailConfirmedAsync(user)).ReturnsAsync(true);
    _mockAppUsersService.Setup(x => x.GetByIdentityIdAsync(user.Id, default)).ReturnsAsync(Result.Success(appUser));
    _mockUserManager.Setup(x => x.GetTwoFactorEnabledAsync(user)).ReturnsAsync(true);

    // Act
    var result = await _controller.LoginWithJwt(request);

    // Assert
    var okResult = Assert.IsType<OkObjectResult>(result);
    Assert.NotNull(okResult.Value);
  }

  [Fact]
  public async Task LoginWithJwt_WithInvalidModelState_ReturnsBadRequest()
  {
    // Arrange
    var request = new JwtLoginRequest
    {
        LoginIdentifier = "",
        Password = ""
    };

    _controller.ModelState.AddModelError("LoginIdentifier", "LoginIdentifier is required");
    _controller.ModelState.AddModelError("Password", "Password is required");

    // Act
    var result = await _controller.LoginWithJwt(request);

    // Assert
    var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
    Assert.NotNull(badRequestResult.Value);
  }

  [Fact]
  public async Task LoginWithJwt_WithSignInRequiresTwoFactor_ReturnsOkWithTwoFactorRequired()
  {
    // Arrange
    var request = new JwtLoginRequest
    {
        LoginIdentifier = "test@example.com",
        Password = "ValidPassword123!"
    };
    var user = new IdentityUser { Id = "user-id", Email = "test@example.com", UserName = "testuser" };

    _mockUserManager.Setup(x => x.FindByEmailAsync(request.LoginIdentifier)).ReturnsAsync(user);
    _mockSignInManager.Setup(x => x.CheckPasswordSignInAsync(user, request.Password, false))
        .ReturnsAsync(SignInResult.TwoFactorRequired);
    _mockUserManager.Setup(x => x.CheckPasswordAsync(user, request.Password)).ReturnsAsync(true);

    // Act
    var result = await _controller.LoginWithJwt(request);

    // Assert
    var okResult = Assert.IsType<OkObjectResult>(result);
    Assert.NotNull(okResult.Value);
  }

  [Fact]
  public async Task LoginWithJwt_WithFailedAppUserRetrieval_ReturnsBadRequest()
  {
    // Arrange
    var request = new JwtLoginRequest
    {
        LoginIdentifier = "test@example.com",
        Password = "ValidPassword123!"
    };
    var user = new IdentityUser { Id = "user-id", Email = "test@example.com", UserName = "testuser" };

    _mockUserManager.Setup(x => x.FindByEmailAsync(request.LoginIdentifier)).ReturnsAsync(user);
    _mockSignInManager.Setup(x => x.CheckPasswordSignInAsync(user, request.Password, false)).ReturnsAsync(SignInResult.Success);
    _mockUserManager.Setup(x => x.CheckPasswordAsync(user, request.Password)).ReturnsAsync(true);
    _mockUserManager.Setup(x => x.IsEmailConfirmedAsync(user)).ReturnsAsync(true);
    _mockAppUsersService.Setup(x => x.GetByIdentityIdAsync(user.Id, default))
        .ReturnsAsync(Result.Error("App user not found"));

    // Act
    var result = await _controller.LoginWithJwt(request);

    // Assert
    var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
    Assert.NotNull(badRequestResult.Value);
  }

  #endregion

  #region RefreshToken Tests

  [Fact]
  public async Task RefreshJwtToken_WithValidToken_ReturnsTokens()
  {
    // Arrange
    var tokens = new JwtTokenResult(
        "new-access-token",
        "new-refresh-token",
        DateTime.UtcNow.AddHours(1),
        "Bearer"
    );

    _mockJwtTokenService.Setup(x => x.RefreshTokensAsync(
        "valid-refresh-token",
        It.IsAny<DeviceInfo>())).ReturnsAsync(tokens);

    // Act
    var result = await _controller.RefreshJwtToken();

    // Assert
    var okResult = Assert.IsType<OkObjectResult>(result);
    Assert.NotNull(okResult.Value);
  }

  [Fact]
  public async Task RefreshJwtToken_WithInvalidToken_ReturnsBadRequest()
  {
    // Arrange
    _mockJwtTokenService.Setup(x => x.RefreshTokensAsync(
        "valid-refresh-token",
        It.IsAny<DeviceInfo>()))
        .ThrowsAsync(new SecurityTokenValidationException("Invalid token"));

    // Act
    var result = await _controller.RefreshJwtToken();

    // Assert
    var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
    Assert.NotNull(badRequestResult.Value);
  }

  [Fact]
  public async Task RefreshJwtToken_WithMissingCookie_ReturnsBadRequest()
  {
    // Arrange
    var httpContext = new DefaultHttpContext();
    httpContext.User = new ClaimsPrincipal(new ClaimsIdentity(new[]
    {
            new Claim(ClaimTypes.NameIdentifier, "test-user-id"),
            new Claim(ClaimTypes.Name, "testuser"),
            new Claim(ClaimTypes.Email, "test@example.com"),
            new Claim("sub", "test-user-id"),
            new Claim(JwtRegisteredClaimNames.Jti, "test-jti")
        }, "mock"));

    // Setup empty cookies
    var mockCookies = new Mock<IRequestCookieCollection>();
    mockCookies.Setup(x => x.TryGetValue("session", out It.Ref<string>.IsAny))
        .Returns((string key, out string value) =>
        {
          value = null;
          return false;
        });
    httpContext.Request.Cookies = mockCookies.Object;

    _controller.ControllerContext = new ControllerContext()
    {
      HttpContext = httpContext
    };

    // Act
    var result = await _controller.RefreshJwtToken();

    // Assert
    var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
    Assert.NotNull(badRequestResult.Value);
  }

  [Fact]
  public async Task RefreshJwtToken_WithGeneralException_ReturnsBadRequest()
  {
    // Arrange
    _mockJwtTokenService.Setup(x => x.RefreshTokensAsync(
        "valid-refresh-token",
        It.IsAny<DeviceInfo>()))
        .ThrowsAsync(new InvalidOperationException("Some error"));

    // Act
    var result = await _controller.RefreshJwtToken();

    // Assert
    var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
    Assert.NotNull(badRequestResult.Value);
  }

  #endregion

  #region Logout Tests

  [Fact]
  public async Task LogoutJwt_WithValidToken_ReturnsOk()
  {
    // Arrange
    _mockJwtTokenService.Setup(x => x.RevokeRefreshTokenAsync("valid-refresh-token")).Returns(Task.CompletedTask);

    // Act
    var result = await _controller.LogoutJwt();

    // Assert
    var okResult = Assert.IsType<OkObjectResult>(result);
    Assert.NotNull(okResult.Value);
  }

  [Fact]
  public async Task LogoutJwt_WithoutCookie_ReturnsOk()
  {
    // Arrange
    var httpContext = new DefaultHttpContext();
    httpContext.User = new ClaimsPrincipal(new ClaimsIdentity(new[]
    {
            new Claim(ClaimTypes.NameIdentifier, "test-user-id"),
            new Claim(ClaimTypes.Name, "testuser"),
            new Claim(ClaimTypes.Email, "test@example.com"),
            new Claim("sub", "test-user-id"),
            new Claim(JwtRegisteredClaimNames.Jti, "test-jti")
        }, "mock"));

    var mockCookies = new Mock<IRequestCookieCollection>();
    mockCookies.Setup(x => x.TryGetValue("session", out It.Ref<string>.IsAny))
        .Returns((string key, out string value) =>
        {
          value = null;
          return false;
        });
    httpContext.Request.Cookies = mockCookies.Object;

    _controller.ControllerContext = new ControllerContext()
    {
      HttpContext = httpContext
    };

    // Act
    var result = await _controller.LogoutJwt();

    // Assert
    var okResult = Assert.IsType<OkObjectResult>(result);
    Assert.NotNull(okResult.Value);
  }

  #endregion

  #region RevokeAll Tests

  [Fact]
  public async Task RevokeAllJwtTokens_WithAuthenticatedUser_ReturnsOk()
  {
    // Arrange
    var userId = "test-user-id";
    _mockJwtTokenService.Setup(x => x.RevokeAllUserRefreshTokensAsync(userId)).Returns(Task.CompletedTask);

    // Act
    var result = await _controller.RevokeAllJwtTokens();

    // Assert
    var okResult = Assert.IsType<OkObjectResult>(result);
    Assert.NotNull(okResult.Value);
  }

  #endregion

  #region ChangePassword Tests

  [Fact]
  public async Task ChangePassword_WithValidRequest_ReturnsOkResult()
  {
    // Arrange
    var request = new ChangePasswordRequest
    {
      OldPassword = "OldPassword123!",
      NewPassword = "NewPassword123!",
      ConfirmPassword = "NewPassword123!"
    };

    var user = new IdentityUser { Id = "test-user-id", Email = "test@example.com" };

    _mockUserManager.Setup(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
        .ReturnsAsync(user);
    _mockUserManager.Setup(x => x.ChangePasswordAsync(user, request.OldPassword, request.NewPassword))
        .ReturnsAsync(IdentityResult.Success);

    // Act
    var result = await _controller.ChangePassword(request);

    // Assert
    var okResult = Assert.IsType<OkObjectResult>(result);
    Assert.NotNull(okResult.Value);
    _mockUserManager.Verify(x => x.ChangePasswordAsync(user, request.OldPassword, request.NewPassword), Times.Once);
  }

  [Fact]
  public async Task ChangePassword_WithInvalidUser_ReturnsErrorResponse()
  {
    // Arrange
    var request = new ChangePasswordRequest
    {
      OldPassword = "OldPassword123!",
      NewPassword = "NewPassword123!",
      ConfirmPassword = "NewPassword123!"
    };

    _mockUserManager.Setup(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
        .ReturnsAsync((IdentityUser)null);

    var errorResult = new NotFoundObjectResult("User not found");
    _mockErrorHandlingService.Setup(x => x.HandleErrorResponse(It.IsAny<Result>()))
        .Returns(errorResult);

    // Act
    var result = await _controller.ChangePassword(request);

    // Assert
    Assert.IsType<NotFoundObjectResult>(result);
    _mockErrorHandlingService.Verify(x => x.HandleErrorResponse(It.IsAny<Result>()), Times.Once);
  }

  [Fact]
  public async Task ChangePassword_WithFailedPasswordChange_ReturnsErrorResponse()
  {
    // Arrange
    var request = new ChangePasswordRequest
    {
      OldPassword = "OldPassword123!",
      NewPassword = "NewPassword123!",
      ConfirmPassword = "NewPassword123!"
    };

    var user = new IdentityUser { Id = "test-user-id", Email = "test@example.com" };

    _mockUserManager.Setup(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
        .ReturnsAsync(user);
    _mockUserManager.Setup(x => x.ChangePasswordAsync(user, request.OldPassword, request.NewPassword))
        .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Password change failed" }));

    var errorResult = new BadRequestObjectResult("Password change failed");
    _mockErrorHandlingService.Setup(x => x.HandleErrorResponse(It.IsAny<Result<string>>()))
        .Returns(errorResult);

    // Act
    var result = await _controller.ChangePassword(request);

    // Assert
    Assert.IsType<BadRequestObjectResult>(result);
    _mockErrorHandlingService.Verify(x => x.HandleErrorResponse(It.IsAny<Result<string>>()), Times.Once);
  }

  #endregion

  #region ForgotPassword Tests

  [Fact]
  public async Task ForgotPassword_WithValidEmail_ReturnsOkResult()
  {
    // Arrange
    var request = new ForgotPasswordRequest { Email = "test@example.com" };
    var user = new IdentityUser { Email = "test@example.com", UserName = "testuser" };

    _mockUserManager.Setup(x => x.FindByEmailAsync(request.Email))
        .ReturnsAsync(user);
    _mockUserManager.Setup(x => x.IsEmailConfirmedAsync(user))
        .ReturnsAsync(true);
    _mockUserManager.Setup(x => x.GeneratePasswordResetTokenAsync(user))
        .ReturnsAsync("reset-token");

    // Act
    var result = await _controller.ForgotPassword(request);

    // Assert
    var okResult = Assert.IsType<OkObjectResult>(result);
    Assert.NotNull(okResult.Value);
    _mockAccountEmailService.Verify(x => x.SendPasswordResetAsync(
        request.Email, It.IsAny<string>(), user.UserName), Times.Once);
  }

  [Fact]
  public async Task ForgotPassword_WithInvalidEmail_ReturnsOkResult()
  {
    // Arrange
    var request = new ForgotPasswordRequest { Email = "nonexistent@example.com" };

    _mockUserManager.Setup(x => x.FindByEmailAsync(request.Email))
        .ReturnsAsync((IdentityUser)null);

    // Act
    var result = await _controller.ForgotPassword(request);

    // Assert
    var okResult = Assert.IsType<OkObjectResult>(result);
    Assert.NotNull(okResult.Value);
    _mockAccountEmailService.Verify(x => x.SendPasswordResetAsync(
        It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
  }

  [Fact]
  public async Task ForgotPassword_WithEmptyEmail_ReturnsBadRequest()
  {
    // Arrange
    var request = new ForgotPasswordRequest { Email = "" };

    // Act
    var result = await _controller.ForgotPassword(request);

    // Assert
    Assert.IsType<BadRequestObjectResult>(result);
  }

  [Fact]
  public async Task ForgotPassword_WithUnconfirmedEmail_ReturnsOkForSecurity()
  {
    // Arrange
    var request = new ForgotPasswordRequest { Email = "unconfirmed@example.com" };
    var user = new IdentityUser { Email = "unconfirmed@example.com", UserName = "testuser" };

    _mockUserManager.Setup(x => x.FindByEmailAsync(request.Email))
        .ReturnsAsync(user);
    _mockUserManager.Setup(x => x.IsEmailConfirmedAsync(user))
        .ReturnsAsync(false);

    // Act
    var result = await _controller.ForgotPassword(request);

    // Assert
    var okResult = Assert.IsType<OkObjectResult>(result);
    Assert.NotNull(okResult.Value);
    _mockAccountEmailService.Verify(x => x.SendPasswordResetAsync(
        It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
  }

  [Fact]
  public async Task ForgotPassword_WithNullEmail_ReturnsBadRequest()
  {
    // Arrange
    var request = new ForgotPasswordRequest { Email = null };

    // Act
    var result = await _controller.ForgotPassword(request);

    // Assert
    Assert.IsType<BadRequestObjectResult>(result);
  }

  #endregion

  #region Register Tests

  [Fact]
  public async Task Register_WithValidRequest_ReturnsOkResult()
  {
    // Arrange
    var request = new RegisterRequest
    {
      Username = "testuser",
      Email = "test@example.com",
      Password = "ValidPassword123!"
    };

    var identityUser = new IdentityUser { Id = "new-user-id", UserName = request.Username, Email = request.Email };
    var appUser = AppUser.Create();

    _mockUserManager.Setup(x => x.CreateAsync(It.IsAny<IdentityUser>(), request.Password))
        .ReturnsAsync(IdentityResult.Success);
    _mockUserManager.Setup(x => x.GetUserIdAsync(It.IsAny<IdentityUser>()))
        .ReturnsAsync("new-user-id");
    _mockUserManager.Setup(x => x.GenerateEmailConfirmationTokenAsync(It.IsAny<IdentityUser>()))
        .ReturnsAsync("confirmation-token");
    _mockAppUsersService.Setup(x => x.AddAsync(It.IsAny<AppUser>(), default))
        .Returns(Task.CompletedTask);
    _mockUnitOfWork.Setup(x => x.SaveChangesAsync(default))
        .ReturnsAsync(1);

    // Act
    var result = await _controller.Register(request);

    // Assert
    var okResult = Assert.IsType<OkObjectResult>(result);
    Assert.NotNull(okResult.Value);
    _mockAccountEmailService.Verify(x => x.SendConfirmationEmailAsync(
        request.Email, "new-user-id", It.IsAny<string>(), request.Username), Times.Once);
  }

  [Fact]
  public async Task Register_WithFailedUserCreation_ReturnsBadRequest()
  {
    // Arrange
    var request = new RegisterRequest
    {
      Username = "testuser",
      Email = "test@example.com",
      Password = "WeakPassword"
    };

    _mockUserManager.Setup(x => x.CreateAsync(It.IsAny<IdentityUser>(), request.Password))
        .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Password too weak" }));

    // Act
    var result = await _controller.Register(request);

    // Assert
    var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
    Assert.NotNull(badRequestResult.Value);
  }

  [Fact]
  public async Task Register_WithInvalidModelState_ReturnsBadRequest()
  {
    // Arrange
    var request = new RegisterRequest
    {
        Username = "",
        Email = "invalid-email",
        Password = ""
    };

    _controller.ModelState.AddModelError("Username", "Username is required");
    _controller.ModelState.AddModelError("Email", "Invalid email format");
    _controller.ModelState.AddModelError("Password", "Password is required");

    // Act
    var result = await _controller.Register(request);

    // Assert
    var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
    Assert.NotNull(badRequestResult.Value);
    _mockAccountEmailService.Verify(x => x.SendConfirmationEmailAsync(
        It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
  }

  #endregion

  #region UpdateNotifications Tests

  [Fact]
  public async Task UpdateNotifications_WithValidRequest_ReturnsNoContentResult()
  {
    // Arrange
    var request = new UpdateUserNotificationsRequest(true, false, true);
    var notificationPreference = new NotificationPreference(true, false, true);
    var commandResponse = new UpdateNotificationPreferencesCommandResponse(Guid.NewGuid(), notificationPreference);
    var result = Result.Success(commandResponse);

    _mockSender.Setup(x => x.Send(It.IsAny<UpdateNotificationPreferencesCommand>(), It.IsAny<CancellationToken>()))
        .ReturnsAsync(result);

    // Act
    var actionResult = await _controller.UpdateNotifications(request, CancellationToken.None);

    // Assert
    Assert.IsType<NoContentResult>(actionResult);
    _mockSender.Verify(x => x.Send(It.IsAny<UpdateNotificationPreferencesCommand>(), It.IsAny<CancellationToken>()), Times.Once);
  }

  [Fact]
  public async Task UpdateNotifications_WithFailedResult_ReturnsErrorResponse()
  {
    // Arrange
    var request = new UpdateUserNotificationsRequest(true, false, true);
    var result = Result<UpdateNotificationPreferencesCommandResponse>.Error("Update failed");

    _mockSender.Setup(x => x.Send(It.IsAny<UpdateNotificationPreferencesCommand>(), It.IsAny<CancellationToken>()))
        .ReturnsAsync(result);

    var errorResult = new BadRequestObjectResult("Update failed");
    _mockErrorHandlingService.Setup(x => x.HandleErrorResponse(It.Is<Result<UpdateNotificationPreferencesCommandResponse>>(r => !r.IsSuccess)))
        .Returns(errorResult);

    // Act
    var actionResult = await _controller.UpdateNotifications(request, CancellationToken.None);

    // Assert
    Assert.IsType<BadRequestObjectResult>(actionResult);
    _mockErrorHandlingService.Verify(x => x.HandleErrorResponse(It.Is<Result<UpdateNotificationPreferencesCommandResponse>>(r => !r.IsSuccess)), Times.Once);
  }

  #endregion

  #region DeviceSessions Tests

  [Fact]
  public async Task GetDeviceSessions_WithAuthenticatedUser_ReturnsDeviceList()
  {
    // Arrange
    var userId = "test-user-id";
    var currentJti = "test-jti";
    var deviceSessions = new List<DeviceSessionDto>
    {
        new DeviceSessionDto(
            Token: "refresh-token-1",
            DeviceName: "Windows - Chrome",
            Platform: "Windows",
            Browser: "Chrome",
            IpAddress: "192.168.1.1",
            LastUsedAt: DateTime.UtcNow,
            CreatedAt: DateTime.UtcNow.AddDays(-1),
            IsCurrent: true),
        new DeviceSessionDto(
            Token: "refresh-token-2",
            DeviceName: "iOS - Safari",
            Platform: "iOS",
            Browser: "Safari",
            IpAddress: "192.168.1.2",
            LastUsedAt: DateTime.UtcNow.AddHours(-1),
            CreatedAt: DateTime.UtcNow.AddDays(-2),
            IsCurrent: false)
    };

    _mockJwtTokenService.Setup(x => x.GetUserDeviceSessionsAsync(userId, currentJti))
            .ReturnsAsync(deviceSessions);

    // Act
    var result = await _controller.GetDeviceSessions();

    // Assert
    var okResult = Assert.IsType<OkObjectResult>(result);
    Assert.NotNull(okResult.Value);
  }

  [Fact]
  public async Task RevokeDeviceSession_WithValidToken_ReturnsOk()
  {
    // Arrange
    var request = new RevokeDeviceRequest { RefreshToken = "refresh-token-to-revoke" };
    var userId = "test-user-id";

    _mockJwtTokenService.Setup(x => x.RevokeDeviceSessionAsync(request.RefreshToken, userId))
        .ReturnsAsync(true);

    // Act
    var result = await _controller.RevokeDeviceSession(request);

    // Assert
    var okResult = Assert.IsType<OkObjectResult>(result);
    Assert.NotNull(okResult.Value);
  }

  [Fact]
  public async Task RevokeDeviceSession_WithInvalidToken_ReturnsBadRequest()
  {
    // Arrange
    var request = new RevokeDeviceRequest { RefreshToken = "invalid-refresh-token" };
    var userId = "test-user-id";

    _mockJwtTokenService.Setup(x => x.RevokeDeviceSessionAsync(request.RefreshToken, userId))
        .ReturnsAsync(false);

    // Act
    var result = await _controller.RevokeDeviceSession(request);

    // Assert
    var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
    Assert.NotNull(badRequestResult.Value);
  }

  [Fact]
  public async Task RevokeDeviceSession_WithEmptyToken_ReturnsBadRequest()
  {
    // Arrange
    var request = new RevokeDeviceRequest { RefreshToken = "" };

    // Act
    var result = await _controller.RevokeDeviceSession(request);

    // Assert
    Assert.IsType<BadRequestObjectResult>(result);
  }

  [Fact]
  public async Task GetDeviceSessions_WithEmptyDeviceList_ReturnsOkWithEmptyList()
  {
    // Arrange
    var userId = "test-user-id";
    var currentJti = "test-jti";
    var deviceSessions = new List<DeviceSessionDto>();

    _mockJwtTokenService.Setup(x => x.GetUserDeviceSessionsAsync(userId, currentJti))
        .ReturnsAsync(deviceSessions);

    // Act
    var result = await _controller.GetDeviceSessions();

    // Assert
    var okResult = Assert.IsType<OkObjectResult>(result);
    Assert.NotNull(okResult.Value);
  }

  [Fact]
  public async Task RevokeDeviceSession_WithNullToken_ReturnsBadRequest()
  {
    // Arrange
    var request = new RevokeDeviceRequest { RefreshToken = null };

    // Act
    var result = await _controller.RevokeDeviceSession(request);

    // Assert
    Assert.IsType<BadRequestObjectResult>(result);
  }

  #endregion

  #region 2FA Tests

  [Fact]
  public async Task GetTwoFactorStatus_WithValidUser_ReturnsOkResult()
  {
    // Arrange
    var user = new IdentityUser { Id = "test-user-id", Email = "test@example.com" };

    _mockUserManager.Setup(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
        .ReturnsAsync(user);
    _mockUserManager.Setup(x => x.GetTwoFactorEnabledAsync(user))
        .ReturnsAsync(true);
    _mockUserManager.Setup(x => x.GetAuthenticatorKeyAsync(user))
        .ReturnsAsync("ABCD1234");
    _mockUserManager.Setup(x => x.CountRecoveryCodesAsync(user))
        .ReturnsAsync(8);
    _mockSignInManager.Setup(x => x.IsTwoFactorClientRememberedAsync(user))
        .ReturnsAsync(false);

    // Act
    var result = await _controller.GetTwoFactorStatus();

    // Assert
    var okResult = Assert.IsType<OkObjectResult>(result);
    Assert.NotNull(okResult.Value);
  }

  [Fact]
  public async Task Disable2fa_WithEnabledUser_ReturnsOkResult()
  {
    // Arrange
    var user = new IdentityUser { Id = "test-user-id", Email = "test@example.com" };

    _mockUserManager.Setup(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
        .ReturnsAsync(user);
    _mockUserManager.Setup(x => x.GetTwoFactorEnabledAsync(user))
        .ReturnsAsync(true);
    _mockUserManager.Setup(x => x.SetTwoFactorEnabledAsync(user, false))
        .ReturnsAsync(IdentityResult.Success);

    // Act
    var result = await _controller.Disable2fa();

    // Assert
    var okResult = Assert.IsType<OkObjectResult>(result);
    Assert.NotNull(okResult.Value);
  }

  [Fact]
  public async Task Disable2fa_WithDisabledUser_ReturnsErrorResponse()
  {
    // Arrange
    var user = new IdentityUser { Id = "test-user-id", Email = "test@example.com" };

    _mockUserManager.Setup(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
        .ReturnsAsync(user);
    _mockUserManager.Setup(x => x.GetTwoFactorEnabledAsync(user))
        .ReturnsAsync(false);

    var errorResult = new BadRequestObjectResult("2FA not enabled");
    _mockErrorHandlingService.Setup(x => x.HandleErrorResponse(It.IsAny<Result>()))
        .Returns(errorResult);

    // Act
    var result = await _controller.Disable2fa();

    // Assert
    Assert.IsType<BadRequestObjectResult>(result);
    _mockErrorHandlingService.Verify(x => x.HandleErrorResponse(It.IsAny<Result>()), Times.Once);
  }

  [Fact]
  public async Task ForgetBrowser_WithValidUser_ReturnsOkResult()
  {
    // Arrange
    var user = new IdentityUser { Id = "test-user-id", Email = "test@example.com" };

    _mockUserManager.Setup(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
        .ReturnsAsync(user);
    _mockSignInManager.Setup(x => x.ForgetTwoFactorClientAsync())
        .Returns(Task.CompletedTask);

    // Act
    var result = await _controller.ForgetBrowser();

    // Assert
    var okResult = Assert.IsType<OkObjectResult>(result);
    Assert.NotNull(okResult.Value);
  }

  [Fact]
  public async Task GetAuthenticatorInfo_WithValidUser_ReturnsOkResult()
  {
    // Arrange
    var user = new IdentityUser { Id = "test-user-id", Email = "test@example.com" };

    _mockUserManager.Setup(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
        .ReturnsAsync(user);
    _mockUserManager.Setup(x => x.GetAuthenticatorKeyAsync(user))
        .ReturnsAsync("ABCD1234EFGH5678");
    _mockUserManager.Setup(x => x.GetEmailAsync(user))
        .ReturnsAsync(user.Email);

    // Act
    var result = await _controller.GetAuthenticatorInfo();

    // Assert
    var okResult = Assert.IsType<OkObjectResult>(result);
    Assert.NotNull(okResult.Value);
  }

  [Fact]
  public async Task EnableAuthenticator_WithValidCode_ReturnsOkResult()
  {
    // Arrange
    var user = new IdentityUser { Id = "test-user-id", Email = "test@example.com" };
    var request = new EnableAuthenticatorRequest { Code = "123456" };

    _mockUserManager.Setup(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
        .ReturnsAsync(user);
    _mockUserManager.Setup(x => x.VerifyTwoFactorTokenAsync(user,
        It.IsAny<string>(), request.Code))
        .ReturnsAsync(true);
    _mockUserManager.Setup(x => x.SetTwoFactorEnabledAsync(user, true))
        .ReturnsAsync(IdentityResult.Success);
    _mockUserManager.Setup(x => x.GetUserIdAsync(user))
        .ReturnsAsync(user.Id);
    _mockUserManager.Setup(x => x.CountRecoveryCodesAsync(user))
        .ReturnsAsync(0);
    _mockUserManager.Setup(x => x.GenerateNewTwoFactorRecoveryCodesAsync(user, 10))
        .ReturnsAsync(new[] { "code1", "code2", "code3" });

    // Act
    var result = await _controller.EnableAuthenticator(request);

    // Assert
    var okResult = Assert.IsType<OkObjectResult>(result);
    Assert.NotNull(okResult.Value);
  }

  [Fact]
  public async Task EnableAuthenticator_WithInvalidCode_ReturnsErrorResponse()
  {
    // Arrange
    var user = new IdentityUser { Id = "test-user-id", Email = "test@example.com" };
    var request = new EnableAuthenticatorRequest { Code = "invalid" };

    _mockUserManager.Setup(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
        .ReturnsAsync(user);
    _mockUserManager.Setup(x => x.VerifyTwoFactorTokenAsync(user,
        It.IsAny<string>(), request.Code))
        .ReturnsAsync(false);

    var errorResult = new BadRequestObjectResult("Invalid code");
    _mockErrorHandlingService.Setup(x => x.HandleErrorResponse(It.IsAny<Result>()))
        .Returns(errorResult);

    // Act
    var result = await _controller.EnableAuthenticator(request);

    // Assert
    Assert.IsType<BadRequestObjectResult>(result);
    _mockErrorHandlingService.Verify(x => x.HandleErrorResponse(It.IsAny<Result>()), Times.Once);
  }

  [Fact]
  public async Task ResetAuthenticator_WithValidUser_ReturnsOkResult()
  {
    // Arrange
    var user = new IdentityUser { Id = "test-user-id", Email = "test@example.com" };

    _mockUserManager.Setup(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
        .ReturnsAsync(user);
    _mockUserManager.Setup(x => x.SetTwoFactorEnabledAsync(user, false))
        .ReturnsAsync(IdentityResult.Success);
    _mockUserManager.Setup(x => x.ResetAuthenticatorKeyAsync(user))
        .ReturnsAsync(IdentityResult.Success);

    // Act
    var result = await _controller.ResetAuthenticator();

    // Assert
    var okResult = Assert.IsType<OkObjectResult>(result);
    Assert.NotNull(okResult.Value);
  }

  [Fact]
  public async Task GenerateRecoveryCodes_WithEnabledUser_ReturnsOkResult()
  {
    // Arrange
    var user = new IdentityUser { Id = "test-user-id", Email = "test@example.com" };

    _mockUserManager.Setup(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
        .ReturnsAsync(user);
    _mockUserManager.Setup(x => x.GetTwoFactorEnabledAsync(user))
        .ReturnsAsync(true);
    _mockUserManager.Setup(x => x.GenerateNewTwoFactorRecoveryCodesAsync(user, 10))
        .ReturnsAsync(new[] { "code1", "code2", "code3" });

    // Act
    var result = await _controller.GenerateRecoveryCodes();

    // Assert
    var okResult = Assert.IsType<OkObjectResult>(result);
    Assert.NotNull(okResult.Value);
  }

  [Fact]
  public async Task GenerateRecoveryCodes_WithDisabledUser_ReturnsErrorResponse()
  {
    // Arrange
    var user = new IdentityUser { Id = "test-user-id", Email = "test@example.com" };

    _mockUserManager.Setup(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
        .ReturnsAsync(user);
    _mockUserManager.Setup(x => x.GetTwoFactorEnabledAsync(user))
        .ReturnsAsync(false);

    var errorResult = new BadRequestObjectResult("2FA not enabled");
    _mockErrorHandlingService.Setup(x => x.HandleErrorResponse(It.IsAny<Result>()))
        .Returns(errorResult);

    // Act
    var result = await _controller.GenerateRecoveryCodes();

    // Assert
    Assert.IsType<BadRequestObjectResult>(result);
    _mockErrorHandlingService.Verify(x => x.HandleErrorResponse(It.IsAny<Result>()), Times.Once);
  }

  [Fact]
  public async Task LoginWith2fa_WithValidCode_ReturnsTokens()
  {
    // Arrange
    var request = new LoginWith2faRequest
    {
      UserId = "user-id",
      TwoFactorCode = "123456"
    };
    var user = new IdentityUser { Id = "user-id", Email = "test@example.com" };
    var appUser = AppUser.Create();
    appUser.SetIdentityId(user.Id);

    var tokens = new JwtTokenResult(
        "access-token",
        "refresh-token",
        DateTime.UtcNow.AddHours(1),
        "Bearer"
    );

    _mockUserManager.Setup(x => x.FindByIdAsync(request.UserId))
        .ReturnsAsync(user);
    _mockUserManager.Setup(x => x.VerifyTwoFactorTokenAsync(user,
        It.IsAny<string>(), request.TwoFactorCode))
        .ReturnsAsync(true);
    _mockAppUsersService.Setup(x => x.GetByIdentityIdAsync(user.Id, default))
        .ReturnsAsync(Result.Success(appUser));
    _mockJwtTokenService.Setup(x => x.GenerateTokensAsync(
        It.Is<IdentityUser>(u => u == user),
        It.Is<AppUser>(a => a == appUser),
        It.IsAny<DeviceInfo>())).ReturnsAsync(tokens);

    // Act
    var result = await _controller.LoginWith2fa(request);

    // Assert
    var okResult = Assert.IsType<OkObjectResult>(result);
    Assert.NotNull(okResult.Value);
  }

  [Fact]
  public async Task LoginWith2fa_WithInvalidCode_ReturnsBadRequest()
  {
    // Arrange
    var request = new LoginWith2faRequest
    {
      UserId = "user-id",
      TwoFactorCode = "invalid"
    };
    var user = new IdentityUser { Id = "user-id", Email = "test@example.com" };

    _mockUserManager.Setup(x => x.FindByIdAsync(request.UserId))
        .ReturnsAsync(user);
    _mockUserManager.Setup(x => x.VerifyTwoFactorTokenAsync(user,
        It.IsAny<string>(), request.TwoFactorCode))
        .ReturnsAsync(false);

    // Act
    var result = await _controller.LoginWith2fa(request);

    // Assert
    var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
    Assert.NotNull(badRequestResult.Value);
  }

  [Fact]
  public async Task LoginWithRecoveryCode_WithValidCode_ReturnsTokens()
  {
    // Arrange
    var request = new LoginWithRecoveryCodeRequest
    {
      UserId = "user-id",
      RecoveryCode = "recovery123"
    };
    var user = new IdentityUser { Id = "user-id", Email = "test@example.com" };
    var appUser = AppUser.Create();
    appUser.SetIdentityId(user.Id);

    var tokens = new JwtTokenResult(
        "access-token",
        "refresh-token",
        DateTime.UtcNow.AddHours(1),
        "Bearer"
    );

    _mockUserManager.Setup(x => x.FindByIdAsync(request.UserId))
        .ReturnsAsync(user);
    _mockUserManager.Setup(x => x.RedeemTwoFactorRecoveryCodeAsync(user, request.RecoveryCode))
        .ReturnsAsync(IdentityResult.Success);
    _mockAppUsersService.Setup(x => x.GetByIdentityIdAsync(user.Id, default))
        .ReturnsAsync(Result.Success(appUser));
    _mockJwtTokenService.Setup(x => x.GenerateTokensAsync(
        It.Is<IdentityUser>(u => u == user),
        It.Is<AppUser>(a => a == appUser),
        It.IsAny<DeviceInfo>())).ReturnsAsync(tokens);

    // Act
    var result = await _controller.LoginWithRecoveryCode(request);

    // Assert
    var okResult = Assert.IsType<OkObjectResult>(result);
    var response = okResult.Value;
    Assert.NotNull(response);

    // Use reflection or dynamic to check the anonymous object properties
    var responseType = response.GetType();
    var accessTokenProperty = responseType.GetProperty("accessToken");
    var expiresAtProperty = responseType.GetProperty("expiresAt");

    Assert.Equal(tokens.AccessToken, accessTokenProperty?.GetValue(response));
    Assert.Equal(tokens.ExpiresAt, expiresAtProperty?.GetValue(response));
  }

  [Fact]
  public async Task LoginWithRecoveryCode_WithInvalidCode_ReturnsBadRequest()
  {
    // Arrange
    var request = new LoginWithRecoveryCodeRequest
    {
      UserId = "user-id",
      RecoveryCode = "invalid"
    };
    var user = new IdentityUser { Id = "user-id", Email = "test@example.com" };

    _mockUserManager.Setup(x => x.FindByIdAsync(request.UserId))
        .ReturnsAsync(user);
    _mockUserManager.Setup(x => x.RedeemTwoFactorRecoveryCodeAsync(user, request.RecoveryCode))
        .ReturnsAsync(IdentityResult.Failed());

    // Act
    var result = await _controller.LoginWithRecoveryCode(request);

    // Assert
    var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
    Assert.NotNull(badRequestResult.Value);
  }

  #endregion

  #region Additional 2FA Tests

  [Fact]
  public async Task GetTwoFactorStatus_WithNullUser_ReturnsErrorResponse()
  {
    // Arrange
    _mockUserManager.Setup(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
        .ReturnsAsync((IdentityUser)null);

    var errorResult = new NotFoundObjectResult("User not found");
    _mockErrorHandlingService.Setup(x => x.HandleErrorResponse(It.IsAny<Result>()))
        .Returns(errorResult);

    // Act
    var result = await _controller.GetTwoFactorStatus();

    // Assert
    Assert.IsType<NotFoundObjectResult>(result);
    _mockErrorHandlingService.Verify(x => x.HandleErrorResponse(It.IsAny<Result>()), Times.Once);
  }

  [Fact]
  public async Task GetAuthenticatorInfo_WithNullUser_ReturnsErrorResponse()
  {
    // Arrange
    _mockUserManager.Setup(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
        .ReturnsAsync((IdentityUser)null);

    var errorResult = new NotFoundObjectResult("User not found");
    _mockErrorHandlingService.Setup(x => x.HandleErrorResponse(It.IsAny<Result>()))
        .Returns(errorResult);

    // Act
    var result = await _controller.GetAuthenticatorInfo();

    // Assert
    Assert.IsType<NotFoundObjectResult>(result);
    _mockErrorHandlingService.Verify(x => x.HandleErrorResponse(It.IsAny<Result>()), Times.Once);
  }

  [Fact]
  public async Task GetAuthenticatorInfo_WithNullAuthenticatorKey_GeneratesNewKey()
  {
    // Arrange
    var user = new IdentityUser { Id = "test-user-id", Email = "test@example.com" };

    _mockUserManager.Setup(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
        .ReturnsAsync(user);
    _mockUserManager.SetupSequence(x => x.GetAuthenticatorKeyAsync(user))
        .ReturnsAsync((string)null)
        .ReturnsAsync("NEWKEY1234567890");
    _mockUserManager.Setup(x => x.ResetAuthenticatorKeyAsync(user))
        .ReturnsAsync(IdentityResult.Success);
    _mockUserManager.Setup(x => x.GetEmailAsync(user))
        .ReturnsAsync(user.Email);

    // Act
    var result = await _controller.GetAuthenticatorInfo();

    // Assert
    var okResult = Assert.IsType<OkObjectResult>(result);
    Assert.NotNull(okResult.Value);
    _mockUserManager.Verify(x => x.ResetAuthenticatorKeyAsync(user), Times.Once);
  }

  [Fact]
  public async Task EnableAuthenticator_WithNullUser_ReturnsErrorResponse()
  {
    // Arrange
    var request = new EnableAuthenticatorRequest { Code = "123456" };

    _mockUserManager.Setup(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
        .ReturnsAsync((IdentityUser)null);

    var errorResult = new NotFoundObjectResult("User not found");
    _mockErrorHandlingService.Setup(x => x.HandleErrorResponse(It.IsAny<Result>()))
        .Returns(errorResult);

    // Act
    var result = await _controller.EnableAuthenticator(request);

    // Assert
    Assert.IsType<NotFoundObjectResult>(result);
    _mockErrorHandlingService.Verify(x => x.HandleErrorResponse(It.IsAny<Result>()), Times.Once);
  }

  [Fact]
  public async Task EnableAuthenticator_WithExistingRecoveryCodes_DoesNotGenerateNew()
  {
    // Arrange
    var user = new IdentityUser { Id = "test-user-id", Email = "test@example.com" };
    var request = new EnableAuthenticatorRequest { Code = "123456" };

    _mockUserManager.Setup(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
        .ReturnsAsync(user);
    _mockUserManager.Setup(x => x.VerifyTwoFactorTokenAsync(user,
        It.IsAny<string>(), request.Code))
        .ReturnsAsync(true);
    _mockUserManager.Setup(x => x.SetTwoFactorEnabledAsync(user, true))
        .ReturnsAsync(IdentityResult.Success);
    _mockUserManager.Setup(x => x.GetUserIdAsync(user))
        .ReturnsAsync(user.Id);
    _mockUserManager.Setup(x => x.CountRecoveryCodesAsync(user))
        .ReturnsAsync(5); // Existing recovery codes

    // Act
    var result = await _controller.EnableAuthenticator(request);

    // Assert
    var okResult = Assert.IsType<OkObjectResult>(result);
    Assert.NotNull(okResult.Value);
    _mockUserManager.Verify(x => x.GenerateNewTwoFactorRecoveryCodesAsync(user, 10), Times.Never);
  }

  [Fact]
  public async Task ResetAuthenticator_WithNullUser_ReturnsErrorResponse()
  {
    // Arrange
    _mockUserManager.Setup(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
        .ReturnsAsync((IdentityUser)null);

    var errorResult = new NotFoundObjectResult("User not found");
    _mockErrorHandlingService.Setup(x => x.HandleErrorResponse(It.IsAny<Result>()))
        .Returns(errorResult);

    // Act
    var result = await _controller.ResetAuthenticator();

    // Assert
    Assert.IsType<NotFoundObjectResult>(result);
    _mockErrorHandlingService.Verify(x => x.HandleErrorResponse(It.IsAny<Result>()), Times.Once);
  }

  [Fact]
  public async Task GenerateRecoveryCodes_WithNullUser_ReturnsErrorResponse()
  {
    // Arrange
    _mockUserManager.Setup(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
        .ReturnsAsync((IdentityUser)null);

    var errorResult = new NotFoundObjectResult("User not found");
    _mockErrorHandlingService.Setup(x => x.HandleErrorResponse(It.IsAny<Result>()))
        .Returns(errorResult);

    // Act
    var result = await _controller.GenerateRecoveryCodes();

    // Assert
    Assert.IsType<NotFoundObjectResult>(result);
    _mockErrorHandlingService.Verify(x => x.HandleErrorResponse(It.IsAny<Result>()), Times.Once);
  }

  [Fact]
  public async Task ForgetBrowser_WithNullUser_ReturnsErrorResponse()
  {
    // Arrange
    _mockUserManager.Setup(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
        .ReturnsAsync((IdentityUser)null);

    var errorResult = new NotFoundObjectResult("User not found");
    _mockErrorHandlingService.Setup(x => x.HandleErrorResponse(It.IsAny<Result>()))
        .Returns(errorResult);

    // Act
    var result = await _controller.ForgetBrowser();

    // Assert
    Assert.IsType<NotFoundObjectResult>(result);
    _mockErrorHandlingService.Verify(x => x.HandleErrorResponse(It.IsAny<Result>()), Times.Once);
  }

  [Fact]
  public async Task Disable2fa_WithNullUser_ReturnsErrorResponse()
  {
    // Arrange
    _mockUserManager.Setup(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
        .ReturnsAsync((IdentityUser)null);

    var errorResult = new NotFoundObjectResult("User not found");
    _mockErrorHandlingService.Setup(x => x.HandleErrorResponse(It.IsAny<Result>()))
        .Returns(errorResult);

    // Act
    var result = await _controller.Disable2fa();

    // Assert
    Assert.IsType<NotFoundObjectResult>(result);
    _mockErrorHandlingService.Verify(x => x.HandleErrorResponse(It.IsAny<Result>()), Times.Once);
  }

  [Fact]
  public async Task Disable2fa_WithFailedDisable_ReturnsErrorResponse()
  {
    // Arrange
    var user = new IdentityUser { Id = "test-user-id", Email = "test@example.com" };

    _mockUserManager.Setup(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
        .ReturnsAsync(user);
    _mockUserManager.Setup(x => x.GetTwoFactorEnabledAsync(user))
        .ReturnsAsync(true);
    _mockUserManager.Setup(x => x.SetTwoFactorEnabledAsync(user, false))
        .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Failed to disable 2FA" }));

    var errorResult = new BadRequestObjectResult("Failed to disable 2FA");
    _mockErrorHandlingService.Setup(x => x.HandleErrorResponse(It.IsAny<Result<string>>()))
        .Returns(errorResult);

    // Act
    var result = await _controller.Disable2fa();

    // Assert
    Assert.IsType<BadRequestObjectResult>(result);
    _mockErrorHandlingService.Verify(x => x.HandleErrorResponse(It.IsAny<Result<string>>()), Times.Once);
  }

  [Fact]
  public async Task LoginWith2fa_WithNullParameters_ReturnsBadRequest()
  {
    // Arrange
    var request = new LoginWith2faRequest
    {
        UserId = null,
        TwoFactorCode = "123456"
    };

    // Act
    var result = await _controller.LoginWith2fa(request);

    // Assert
    Assert.IsType<BadRequestObjectResult>(result);
  }

  [Fact]
  public async Task LoginWith2fa_WithNullUser_ReturnsBadRequest()
  {
    // Arrange
    var request = new LoginWith2faRequest
    {
        UserId = "user-id",
        TwoFactorCode = "123456"
    };

    _mockUserManager.Setup(x => x.FindByIdAsync(request.UserId))
        .ReturnsAsync((IdentityUser)null);

    // Act
    var result = await _controller.LoginWith2fa(request);

    // Assert
    var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
    Assert.NotNull(badRequestResult.Value);
  }

  [Fact]
  public async Task LoginWith2fa_WithFailedAppUserRetrieval_ReturnsBadRequest()
  {
    // Arrange
    var request = new LoginWith2faRequest
    {
        UserId = "user-id",
        TwoFactorCode = "123456"
    };
    var user = new IdentityUser { Id = "user-id", Email = "test@example.com" };

    _mockUserManager.Setup(x => x.FindByIdAsync(request.UserId))
        .ReturnsAsync(user);
    _mockUserManager.Setup(x => x.VerifyTwoFactorTokenAsync(user,
        It.IsAny<string>(), request.TwoFactorCode))
        .ReturnsAsync(true);
    _mockAppUsersService.Setup(x => x.GetByIdentityIdAsync(user.Id, default))
        .ReturnsAsync(Result.Error("App user not found"));

    // Act
    var result = await _controller.LoginWith2fa(request);

    // Assert
    var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
    Assert.NotNull(badRequestResult.Value);
  }

  [Fact]
  public async Task LoginWithRecoveryCode_WithNullParameters_ReturnsBadRequest()
  {
    // Arrange
    var request = new LoginWithRecoveryCodeRequest
    {
        UserId = null,
        RecoveryCode = "recovery123"
    };

    // Act
    var result = await _controller.LoginWithRecoveryCode(request);

    // Assert
    Assert.IsType<BadRequestObjectResult>(result);
  }

  [Fact]
  public async Task LoginWithRecoveryCode_WithNullUser_ReturnsBadRequest()
  {
    // Arrange
    var request = new LoginWithRecoveryCodeRequest
    {
        UserId = "user-id",
        RecoveryCode = "recovery123"
    };

    _mockUserManager.Setup(x => x.FindByIdAsync(request.UserId))
        .ReturnsAsync((IdentityUser)null);

    // Act
    var result = await _controller.LoginWithRecoveryCode(request);

    // Assert
    var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
    Assert.NotNull(badRequestResult.Value);
  }

  [Fact]
  public async Task LoginWithRecoveryCode_WithFailedAppUserRetrieval_ReturnsBadRequest()
  {
    // Arrange
    var request = new LoginWithRecoveryCodeRequest
    {
        UserId = "user-id",
        RecoveryCode = "recovery123"
    };
    var user = new IdentityUser { Id = "user-id", Email = "test@example.com" };

    _mockUserManager.Setup(x => x.FindByIdAsync(request.UserId))
        .ReturnsAsync(user);
    _mockUserManager.Setup(x => x.RedeemTwoFactorRecoveryCodeAsync(user, request.RecoveryCode))
        .ReturnsAsync(IdentityResult.Success);
    _mockAppUsersService.Setup(x => x.GetByIdentityIdAsync(user.Id, default))
        .ReturnsAsync(Result.Error("App user not found"));

    // Act
    var result = await _controller.LoginWithRecoveryCode(request);

    // Assert
    var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
    Assert.NotNull(badRequestResult.Value);
  }

  #endregion
}
