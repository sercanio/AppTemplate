using System.ComponentModel.DataAnnotations;
using System.Text;
using AppTemplate.Application.Features.Accounts.UpdateNotificationPreferences;
using AppTemplate.Application.Services.AppUsers;
using AppTemplate.Application.Services.Authentication;
using AppTemplate.Application.Services.Authentication.Models;
using AppTemplate.Application.Services.Authorization;
using AppTemplate.Application.Services.EmailSenders;
using AppTemplate.Application.Services.ErrorHandling;
using AppTemplate.Domain;
using AppTemplate.Domain.AppUsers;
using AppTemplate.Domain.AppUsers.ValueObjects;
using Ardalis.Result;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using UAParser;

namespace AppTemplate.Web.Controllers.Api.v1;

[ApiController]
[ApiVersion(ApiVersions.V1)]
[Route("api/v{version:apiVersion}/[controller]")]
[EnableRateLimiting("Fixed")]
public class AccountsController : BaseController
{
  private readonly UserManager<IdentityUser> _userManager;
  private readonly SignInManager<IdentityUser> _signInManager;
  private readonly IAppUsersService _appUsersService;
  private readonly IUnitOfWork _unitOfWork;
  private readonly IAccountEmailService _accountEmailService;
  private readonly IJwtTokenService _jwtTokenService;
  private readonly IConfiguration _configuration;

  public AccountsController(
      UserManager<IdentityUser> userManager,
      SignInManager<IdentityUser> signInManager,
      IAppUsersService appUsersService,
      IUnitOfWork unitOfWork,
      IAccountEmailService accountEmailService,
      IJwtTokenService jwtTokenService,
      IConfiguration configuration,
      ISender sender,
      IErrorHandlingService errorHandlingService
      ) : base(sender, errorHandlingService)
  {
    _userManager = userManager;
    _signInManager = signInManager;
    _accountEmailService = accountEmailService;
    _appUsersService = appUsersService;
    _unitOfWork = unitOfWork;
    _jwtTokenService = jwtTokenService;
    _configuration = configuration;
  }

  private Result<T> ConvertIdentityResult<T>(IdentityResult identityResult, T value = default!, string defaultErrorMessage = "Operation failed.")
  {
    if (identityResult.Succeeded)
    {
      return Result.Success(value);
    }

    var errors = identityResult.Errors.Select(e => e.Description).ToList();
    var errorList = new ErrorList(errors);
    return Result.Error(errors.Count > 0 ? errorList : new ErrorList(new[] { defaultErrorMessage }));
  }

  [HttpGet("confirm-email")]
  public async Task<IActionResult> ConfirmEmail([FromQuery] string userId, [FromQuery] string code)
  {
    if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(code))
    {
      return _errorHandlingService.HandleErrorResponse(Result.Invalid(new ValidationError("UserId and code are required.")));
    }

    var user = await _userManager.FindByIdAsync(userId);
    if (user == null)
    {
      return _errorHandlingService.HandleErrorResponse(Result.NotFound($"Unable to load user with ID '{userId}'."));
    }

    code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(code) ?? Array.Empty<byte>());
    var identityResult = await _userManager.ConfirmEmailAsync(user, code);

    var result = ConvertIdentityResult(identityResult, "Email confirmed successfully.", "Error confirming email.");

    return result.IsSuccess
        ? Ok(new { message = result.Value })
        : _errorHandlingService.HandleErrorResponse(result);
  }

  [HttpPost("changeemail")]
  [Authorize]
  public async Task<IActionResult> ChangeEmail([FromBody] ChangeEmailRequest request)
  {
    var user = await _userManager.GetUserAsync(User);
    if (user == null)
    {
      return NotFound(new { error = $"Unable to load user." });
    }

    var email = await _userManager.GetEmailAsync(user);
    if (request.NewEmail == email)
    {
      return Ok(new { message = "Your email is unchanged." });
    }

    var existingUser = await _userManager.FindByEmailAsync(request.NewEmail);
    if (existingUser != null)
    {
      return BadRequest(new { error = "Email already in use." });
    }

    var userId = await _userManager.GetUserIdAsync(user);
    var code = await _userManager.GenerateChangeEmailTokenAsync(user, request.NewEmail);
    code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));

    await _accountEmailService.SendEmailChangeConfirmationAsync(
        request.NewEmail,
        userId,
        code,
        user.UserName!);

    return Ok(new { message = "Confirmation link to change email sent. Please check your email." });
  }

  [HttpPost("sendverificationemail")]
  [Authorize]
  public async Task<IActionResult> SendVerificationEmail()
  {
    var user = await _userManager.GetUserAsync(User);
    if (user == null)
    {
      return NotFound(new { error = $"Unable to load user." });
    }

    if (await _userManager.IsEmailConfirmedAsync(user))
    {
      return Ok(new { message = "Your email is already confirmed." });
    }

    var userId = await _userManager.GetUserIdAsync(user);
    var email = await _userManager.GetEmailAsync(user);
    var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
    code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));

    await _accountEmailService.SendConfirmationEmailAsync(
        email!,
        userId,
        code,
        user.UserName!);

    return Ok(new { message = "Verification email sent. Please check your email." });
  }

  [HttpPost("resendemailconfirmation")]
  public async Task<IActionResult> ResendEmailConfirmation([FromBody] ResendEmailConfirmationRequest request)
  {
    if (string.IsNullOrEmpty(request.Email))
    {
      return BadRequest(new { error = "Email is required." });
    }

    var user = await _userManager.FindByEmailAsync(request.Email);
    if (user == null)
    {
      // To prevent user enumeration, always return success.
      return Ok(new { message = "Verification email sent. Please check your email." });
    }

    var userId = await _userManager.GetUserIdAsync(user);
    var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
    code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));

    await _accountEmailService.SendConfirmationEmailAsync(
        request.Email,
        userId,
        code,
        user.UserName!);

    return Ok(new { message = "Verification email sent. Please check your email." });
  }

  [HttpGet("confirmemailchange")]
  public async Task<IActionResult> ConfirmEmailChange([FromQuery] string userId, [FromQuery] string email, [FromQuery] string code)
  {
    if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(email) || string.IsNullOrEmpty(code))
    {
      return BadRequest(new { error = "UserId, email, and code are required." });
    }

    var user = await _userManager.FindByIdAsync(userId);
    if (user == null)
    {
      return NotFound(new { error = $"Unable to load user with ID '{userId}'." });
    }

    code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(code) ?? Array.Empty<byte>());
    var changeResult = await _userManager.ChangeEmailAsync(user, email, code);
    if (!changeResult.Succeeded)
    {
      return BadRequest(new { error = "Error changing email.", details = changeResult.Errors });
    }

    return Ok(new { message = "Email change confirmed." });
  }

  [HttpPost("changepassword")]
  [Authorize]
  public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
  {
    var user = await _userManager.GetUserAsync(User);
    if (user == null)
    {
      return _errorHandlingService.HandleErrorResponse(Result.NotFound("Unable to load user."));
    }

    var identityResult = await _userManager.ChangePasswordAsync(user, request.OldPassword, request.NewPassword);
    var result = ConvertIdentityResult(identityResult, "Your password has been changed successfully.", "Password change failed.");

    return result.IsSuccess
        ? Ok(new { message = result.Value })
        : _errorHandlingService.HandleErrorResponse(result);
  }

  [HttpPost("forgotpassword")]
  public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
  {
    if (string.IsNullOrEmpty(request.Email))
    {
      return BadRequest(new { error = "Email is required." });
    }

    var user = await _userManager.FindByEmailAsync(request.Email);
    if (user == null || !await _userManager.IsEmailConfirmedAsync(user))
    {
      // To prevent user enumeration, always return success.
      return Ok(new { message = "Verification email sent. Please check your email." });
    }

    var code = await _userManager.GeneratePasswordResetTokenAsync(user);
    code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));

    await _accountEmailService.SendPasswordResetAsync(
        request.Email,
        code,
        user.UserName!);

    return Ok(new { message = "Verification email sent. Please check your email." });
  }

  // POST: /api/v1.0/account/login
  [HttpPost("login")]
  public async Task<IActionResult> LoginWithJwt(
      [FromBody] JwtLoginRequest request,
      [FromHeader(Name = "User-Agent")] string? userAgent,
      [FromHeader(Name = "X-Forwarded-For")] string? forwardedFor,
      [FromHeader(Name = "X-Real-IP")] string? realIp,
      [FromHeader(Name = "X-Browser-Info")] string? browserInfo)
  {
    var context = HttpContext;
    if (!ModelState.IsValid)
    {
      return BadRequest(ModelState);
    }

    var user = await _userManager.FindByEmailAsync(request.LoginIdentifier)
               ?? await _userManager.FindByNameAsync(request.LoginIdentifier);

    if (user == null)
    {
      return BadRequest(new { error = "Invalid credentials" });
    }

    var result = await _signInManager.CheckPasswordSignInAsync(user, request.Password, lockoutOnFailure: false);
    if (!await _userManager.CheckPasswordAsync(user, request.Password))
    {
      return BadRequest(new { error = "Invalid credentials" });
    }

    if (result.Succeeded)
    {
      if (!await _userManager.IsEmailConfirmedAsync(user))
      {
        return BadRequest(new { error = "Email not confirmed" });
      }

      var appUser = await _appUsersService.GetByIdentityIdAsync(user.Id);
      if (!appUser.IsSuccess)
      {
        return BadRequest(new { error = "App user not found" });
      }

      // Check if 2FA is required
      if (await _userManager.GetTwoFactorEnabledAsync(user))
      {
        return Ok(new
        {
          requiresTwoFactor = true,
          userId = user.Id
        });
      }

      var deviceInfo = CreateDeviceInfo(userAgent!, forwardedFor!, realIp!, browserInfo!, context);
      var tokens = await _jwtTokenService.GenerateTokensAsync(user, appUser.Value, deviceInfo);

      // Set refresh token as HTTP-only cookie with RememberMe consideration
      SetRefreshTokenCookie(tokens.RefreshToken, request.RememberMe);

      // Return only access token in response body
      return Ok(new
      {
        accessToken = tokens.AccessToken,
        expiresAt = tokens.ExpiresAt
      });
    }

    if (result.RequiresTwoFactor)
    {
      return Ok(new { requiresTwoFactor = true, userId = user.Id });
    }

    if (result.IsLockedOut)
    {
      return BadRequest(new { error = "Account locked" });
    }

    return BadRequest(new { error = "Invalid credentials" });
  }

  [HttpPost("refresh-token")]
  public async Task<IActionResult> RefreshJwtToken(
      [FromHeader(Name = "User-Agent")] string? userAgent,
      [FromHeader(Name = "X-Forwarded-For")] string? forwardedFor,
      [FromHeader(Name = "X-Real-IP")] string? realIp,
      [FromHeader(Name = "X-Browser-Info")] string? browserInfo)
  {
    var context = HttpContext;
    try
    {
      // Get refresh token from cookie using HttpContext
      if (!context.Request.Cookies.TryGetValue("session", out var refreshToken) ||
          string.IsNullOrEmpty(refreshToken))
      {
        return BadRequest(new { error = "Refresh token not found" });
      }

      var deviceInfo = CreateDeviceInfo(userAgent!, forwardedFor!, realIp!, browserInfo!, context);
      var tokens = await _jwtTokenService.RefreshTokensAsync(refreshToken, deviceInfo);

      // Set new refresh token in cookie
      SetRefreshTokenCookie(tokens.RefreshToken);

      // Return only access token
      return Ok(new
      {
        accessToken = tokens.AccessToken,
        expiresAt = tokens.ExpiresAt
      });
    }
    catch (SecurityTokenValidationException ex)
    {
      // Clear the invalid cookie
      context.Response.Cookies.Delete("session");
      return BadRequest(new { error = ex.Message });
    }
    catch (Exception)
    {
      return BadRequest(new { error = "An error occurred while refreshing the token." });
    }
  }

  [HttpPost("logout")]
  [Authorize(AuthenticationSchemes = "Bearer")]
  public async Task<IActionResult> LogoutJwt()
  {
    var context = HttpContext;
    if (context.Request.Cookies.TryGetValue("session", out var refreshToken) &&
        !string.IsNullOrEmpty(refreshToken))
    {
      await _jwtTokenService.RevokeRefreshTokenAsync(refreshToken);
    }

    // Clear the refresh token cookie
    context.Response.Cookies.Delete("session", new CookieOptions
    {
      Path = "/",
      Secure = true,
      SameSite = SameSiteMode.None
    });

    return Ok(new { message = "Logged out successfully" });
  }

  [HttpPost("revoke-all")]
  [Authorize(AuthenticationSchemes = "Bearer")]
  public async Task<IActionResult> RevokeAllJwtTokens()
  {
    var userId = User.GetIdentityId();
    await _jwtTokenService.RevokeAllUserRefreshTokensAsync(userId);
    return Ok(new { message = "All tokens revoked successfully" });
  }

  [HttpPost("revoke-others")]
  [Authorize(AuthenticationSchemes = "Bearer")]
  public async Task<IActionResult> RevokeOtherJwtTokens()
  {
    var userId = User.GetIdentityId();

    // Get the JTI from current access token to preserve current session
    var currentJti = User.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Jti)?.Value;

    if (string.IsNullOrEmpty(currentJti))
    {
      return BadRequest(new { error = "Unable to identify current session." });
    }

    await _jwtTokenService.RevokeOtherUserRefreshTokensAsync(userId, currentJti);
    return Ok(new { message = "All other sessions revoked successfully" });
  }

  // 2FA Endpoints
  [HttpGet("2fa/status")]
  [Authorize]
  public async Task<IActionResult> GetTwoFactorStatus()
  {
    var user = await _userManager.GetUserAsync(User);
    if (user == null)
    {
      return _errorHandlingService.HandleErrorResponse(Result.NotFound("Unable to load user."));
    }

    var isTwoFactorEnabled = await _userManager.GetTwoFactorEnabledAsync(user);
    var hasAuthenticator = await _userManager.GetAuthenticatorKeyAsync(user) != null;
    var recoveryCodesLeft = await _userManager.CountRecoveryCodesAsync(user);
    var isMachineRemembered = await _signInManager.IsTwoFactorClientRememberedAsync(user);

    return Ok(new
    {
      is2faEnabled = isTwoFactorEnabled,
      hasAuthenticator,
      recoveryCodesLeft,
      isMachineRemembered
    });
  }

  [HttpPost("2fa/disable")]
  [Authorize]
  public async Task<IActionResult> Disable2fa()
  {
    var user = await _userManager.GetUserAsync(User);
    if (user == null)
    {
      return _errorHandlingService.HandleErrorResponse(Result.NotFound("Unable to load user."));
    }

    var isTwoFactorEnabled = await _userManager.GetTwoFactorEnabledAsync(user);
    if (!isTwoFactorEnabled)
    {
      return _errorHandlingService.HandleErrorResponse(Result.Invalid(new ValidationError("Two-factor authentication is not currently enabled.")));
    }

    var result = await _userManager.SetTwoFactorEnabledAsync(user, false);
    if (!result.Succeeded)
    {
      return _errorHandlingService.HandleErrorResponse(ConvertIdentityResult<string>(result, defaultErrorMessage: "Error disabling 2FA."));
    }

    return Ok(new { message = "Two-factor authentication has been disabled." });
  }

  [HttpPost("2fa/forget-browser")]
  [Authorize]
  public async Task<IActionResult> ForgetBrowser()
  {
    var user = await _userManager.GetUserAsync(User);
    if (user == null)
    {
      return _errorHandlingService.HandleErrorResponse(Result.NotFound("Unable to load user."));
    }

    await _signInManager.ForgetTwoFactorClientAsync();
    return Ok(new { message = "The current browser has been forgotten. When you login again from this browser you will be prompted for your 2FA code." });
  }

  [HttpGet("2fa/authenticator")]
  [Authorize]
  public async Task<IActionResult> GetAuthenticatorInfo()
  {
    var user = await _userManager.GetUserAsync(User);
    if (user == null)
    {
      return _errorHandlingService.HandleErrorResponse(Result.NotFound("Unable to load user."));
    }

    // Load the authenticator key & QR code URI
    var unformattedKey = await _userManager.GetAuthenticatorKeyAsync(user);
    if (string.IsNullOrEmpty(unformattedKey))
    {
      await _userManager.ResetAuthenticatorKeyAsync(user);
      unformattedKey = await _userManager.GetAuthenticatorKeyAsync(user);
    }

    var sharedKey = FormatKey(unformattedKey!);
    var email = await _userManager.GetEmailAsync(user);
    var authenticatorUri = GenerateQrCodeUri(email!, unformattedKey!);

    return Ok(new
    {
      sharedKey,
      authenticatorUri
    });
  }

  [HttpPost("2fa/authenticator")]
  [Authorize]
  public async Task<IActionResult> EnableAuthenticator([FromBody] EnableAuthenticatorRequest request)
  {
    var user = await _userManager.GetUserAsync(User);
    if (user == null)
    {
      return _errorHandlingService.HandleErrorResponse(Result.NotFound("Unable to load user."));
    }

    // Strip spaces and hyphens
    var verificationCode = request.Code.Replace(" ", string.Empty).Replace("-", string.Empty);

    var is2faTokenValid = await _userManager.VerifyTwoFactorTokenAsync(
        user, _userManager.Options.Tokens.AuthenticatorTokenProvider, verificationCode);

    if (!is2faTokenValid)
    {
      return _errorHandlingService.HandleErrorResponse(Result.Invalid(new ValidationError("Verification code is invalid.")));
    }

    await _userManager.SetTwoFactorEnabledAsync(user, true);
    var userId = await _userManager.GetUserIdAsync(user);

    var recoveryCodes = new List<string>();
    if (await _userManager.CountRecoveryCodesAsync(user) == 0)
    {
      var newRecoveryCodes = await _userManager.GenerateNewTwoFactorRecoveryCodesAsync(user, 10);
      recoveryCodes.AddRange(newRecoveryCodes!);
    }

    return Ok(new
    {
      message = "Your authenticator app has been verified.",
      recoveryCodes = recoveryCodes.Count > 0 ? recoveryCodes : null
    });
  }

  [HttpPost("2fa/authenticator/reset")]
  [Authorize]
  public async Task<IActionResult> ResetAuthenticator()
  {
    var user = await _userManager.GetUserAsync(User);
    if (user == null)
    {
      return _errorHandlingService.HandleErrorResponse(Result.NotFound("Unable to load user."));
    }

    await _userManager.SetTwoFactorEnabledAsync(user, false);
    await _userManager.ResetAuthenticatorKeyAsync(user);

    return Ok(new { message = "Your authenticator app key has been reset." });
  }

  [HttpPost("2fa/recovery-codes/generate")]
  [Authorize]
  public async Task<IActionResult> GenerateRecoveryCodes()
  {
    var user = await _userManager.GetUserAsync(User);
    if (user == null)
    {
      return _errorHandlingService.HandleErrorResponse(Result.NotFound("Unable to load user."));
    }

    var isTwoFactorEnabled = await _userManager.GetTwoFactorEnabledAsync(user);
    if (!isTwoFactorEnabled)
    {
      return _errorHandlingService.HandleErrorResponse(Result.Invalid(new ValidationError("Cannot generate recovery codes for user because they do not have 2FA enabled.")));
    }

    var recoveryCodes = await _userManager.GenerateNewTwoFactorRecoveryCodesAsync(user, 10);

    if (recoveryCodes == null || !recoveryCodes.Any())
    {
      return _errorHandlingService.HandleErrorResponse(Result.Error(new ErrorList(new[] { "Error generating recovery codes." })));
    }

    return Ok(new
    {
      recoveryCodes = recoveryCodes.ToArray(),
      message = "You have generated new recovery codes."
    });
  }

  [HttpGet("devices")]
  [Authorize(AuthenticationSchemes = "Bearer")]
  public async Task<IActionResult> GetDeviceSessions()
  {
    var userId = User.GetIdentityId();

    // Get the JTI from current access token
    var currentJti = User.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Jti)?.Value;

    var deviceSessions = await _jwtTokenService.GetUserDeviceSessionsAsync(userId, currentJti);

    return Ok(new { devices = deviceSessions });
  }

  [HttpPost("devices/revoke")]
  [Authorize(AuthenticationSchemes = "Bearer")]
  public async Task<IActionResult> RevokeDeviceSession([FromBody] RevokeDeviceRequest request)
  {
    if (string.IsNullOrEmpty(request.RefreshToken))
    {
      return BadRequest(new { error = "RefreshToken is required." });
    }

    var userId = User.GetIdentityId();
    var success = await _jwtTokenService.RevokeDeviceSessionAsync(request.RefreshToken, userId);

    if (!success)
    {
      return BadRequest(new { error = "Device session not found or already revoked." });
    }

    return Ok(new { message = "Device session revoked successfully." });
  }

  [HttpPost("2fa/login")]
  public async Task<IActionResult> LoginWith2fa(
      [FromBody] LoginWith2faRequest request,
      [FromHeader(Name = "User-Agent")] string? userAgent,
      [FromHeader(Name = "X-Forwarded-For")] string? forwardedFor,
      [FromHeader(Name = "X-Real-IP")] string? realIp,
      [FromHeader(Name = "X-Browser-Info")] string? browserInfo)
  {
    var context = HttpContext;
    if (string.IsNullOrEmpty(request.UserId) || string.IsNullOrEmpty(request.TwoFactorCode))
    {
      return BadRequest(new { error = "UserId and TwoFactorCode are required." });
    }

    var user = await _userManager.FindByIdAsync(request.UserId);
    if (user == null)
    {
      return BadRequest(new { error = "Unable to load user." });
    }

    var code = request.TwoFactorCode.Replace(" ", string.Empty).Replace("-", string.Empty);
    var is2faTokenValid = await _userManager.VerifyTwoFactorTokenAsync(
        user, _userManager.Options.Tokens.AuthenticatorTokenProvider, code);

    if (!is2faTokenValid)
    {
      return BadRequest(new { error = "Invalid authenticator code." });
    }

    var appUser = await _appUsersService.GetByIdentityIdAsync(user.Id);
    if (!appUser.IsSuccess)
    {
      return BadRequest(new { error = "App user not found" });
    }

    var deviceInfo = CreateDeviceInfo(userAgent!, forwardedFor!, realIp!, browserInfo!, context);
    var tokens = await _jwtTokenService.GenerateTokensAsync(user, appUser.Value, deviceInfo);

    // Set refresh token as HTTP-only cookie with RememberMe consideration
    SetRefreshTokenCookie(tokens.RefreshToken, request.RememberMe);

    // Return only access token in response body
    return Ok(new
    {
      accessToken = tokens.AccessToken,
      expiresAt = tokens.ExpiresAt
    });
  }

  [HttpPost("2fa/login-recovery")]
  public async Task<IActionResult> LoginWithRecoveryCode(
      [FromBody] LoginWithRecoveryCodeRequest request,
      [FromHeader(Name = "User-Agent")] string? userAgent,
      [FromHeader(Name = "X-Forwarded-For")] string? forwardedFor,
      [FromHeader(Name = "X-Real-IP")] string? realIp,
      [FromHeader(Name = "X-Browser-Info")] string? browserInfo)
  {
    var context = HttpContext;
    if (string.IsNullOrEmpty(request.UserId) || string.IsNullOrEmpty(request.RecoveryCode))
    {
      return BadRequest(new { error = "UserId and RecoveryCode are required." });
    }

    var user = await _userManager.FindByIdAsync(request.UserId);
    if (user == null)
    {
      return BadRequest(new { error = "Unable to load user." });
    }

    var recoveryCode = request.RecoveryCode.Replace(" ", string.Empty);
    var result = await _userManager.RedeemTwoFactorRecoveryCodeAsync(user, recoveryCode);

    if (!result.Succeeded)
    {
      return BadRequest(new { error = "Invalid recovery code." });
    }

    var appUser = await _appUsersService.GetByIdentityIdAsync(user.Id);
    if (!appUser.IsSuccess)
    {
      return BadRequest(new { error = "App user not found" });
    }

    var deviceInfo = CreateDeviceInfo(userAgent!, forwardedFor!, realIp!, browserInfo!, context);
    var tokens = await _jwtTokenService.GenerateTokensAsync(user, appUser.Value, deviceInfo);

    // Set refresh token as HTTP-only cookie
    SetRefreshTokenCookie(tokens.RefreshToken);

    // Return only access token in response body
    return Ok(new
    {
      accessToken = tokens.AccessToken,
      expiresAt = tokens.ExpiresAt
    });
  }

  [HttpPatch("me/notifications")]
  [Authorize]
  [HasPermission("notifications.update")]
  public async Task<IActionResult> UpdateNotifications(
    UpdateUserNotificationsRequest request,
    CancellationToken cancellationToken)
  {
    NotificationPreference notificationPreference = new(
        request.InAppNotification,
        request.EmailNotification,
        request.PushNotification);

    UpdateNotificationPreferencesCommand command = new(notificationPreference);
    Result<UpdateNotificationPreferencesCommandResponse> result = await _sender.Send(command, cancellationToken);

    return !result.IsSuccess ? _errorHandlingService.HandleErrorResponse(result) : NoContent();
  }

  [HttpPost("register")]
  public async Task<IActionResult> Register([FromBody] RegisterRequest request)
  {
    if (!ModelState.IsValid)
    {
      return BadRequest(ModelState);
    }

    var identityUser = new IdentityUser { UserName = request.Username, Email = request.Email };
    var identityResult = await _userManager.CreateAsync(identityUser, request.Password);

    if (!identityResult.Succeeded)
    {
      var errors = identityResult.Errors.Select(e => e.Description).ToList();
      return BadRequest(new { error = "Registration failed.", details = errors });
    }

    var identityId = await _userManager.GetUserIdAsync(identityUser);
    var code = await _userManager.GenerateEmailConfirmationTokenAsync(identityUser);

    var appUser = AppUser.Create();
    appUser.SetIdentityId(identityId);

    await _appUsersService.AddAsync(appUser);
    await _unitOfWork.SaveChangesAsync();

    code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
    await _accountEmailService.SendConfirmationEmailAsync(
        request.Email,
        identityId,
        code,
        request.Username);

    return Ok(new { message = "User registered successfully. Please confirm your email." });
  }

  // Helper methods for 2FA
  private string FormatKey(string unformattedKey)
  {
    var result = new StringBuilder();
    int currentPosition = 0;
    while (currentPosition + 4 < unformattedKey.Length)
    {
      result.Append(unformattedKey.AsSpan(currentPosition, 4)).Append(' ');
      currentPosition += 4;
    }
    if (currentPosition < unformattedKey.Length)
    {
      result.Append(unformattedKey.AsSpan(currentPosition));
    }

    return result.ToString().ToLowerInvariant();
  }

  private string GenerateQrCodeUri(string email, string unformattedKey)
  {
    const string AuthenticatorUriFormat = "otpauth://totp/{0}:{1}?secret={2}&issuer={0}&digits=6";
    return string.Format(
        System.Globalization.CultureInfo.InvariantCulture,
        AuthenticatorUriFormat,
        System.Web.HttpUtility.UrlEncode("AppTemplate"),
        System.Web.HttpUtility.UrlEncode(email),
        unformattedKey);
  }

  private DeviceInfo CreateDeviceInfo(
      string userAgent,
      string forwardedFor,
      string realIp,
      string browserInfo,
      HttpContext context)
  {
    var ipAddress = GetClientIpAddress(forwardedFor, realIp, context);
    var (platform, browser, deviceName) = ParseUserAgent(userAgent ?? "Unknown", browserInfo);

    return new DeviceInfo(
        UserAgent: userAgent ?? "Unknown",
        IpAddress: ipAddress,
        DeviceName: deviceName,
        Platform: platform,
        Browser: browser);
  }

  private string GetClientIpAddress(string forwardedFor, string realIp, HttpContext context)
  {
    if (!string.IsNullOrEmpty(forwardedFor))
    {
      return forwardedFor.Split(',')[0].Trim();
    }

    if (!string.IsNullOrEmpty(realIp))
    {
      return realIp;
    }

    return context.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
  }

  private (string platform, string browser, string deviceName) ParseUserAgent(string userAgent, string browserInfo)
  {
    if (string.IsNullOrEmpty(userAgent))
    {
      return ("Unknown", "Unknown", "Unknown Device");
    }

    var parser = Parser.GetDefault();
    var clientInfo = parser.Parse(userAgent);

    var platform = clientInfo.OS.Family ?? "Unknown";
    var browser = clientInfo.Browser.Family ?? "Unknown";

    // Check for custom browser info header first (for Brave detection)
    if (!string.IsNullOrEmpty(browserInfo) && browserInfo.Equals("Brave", StringComparison.OrdinalIgnoreCase))
    {
      browser = "Brave";
    }
    else
    {
      // Enhanced browser detection - order matters!
      var userAgentLower = userAgent.ToLowerInvariant();

      // Check for specific Chromium-based browsers first (before checking for Chrome)
      if (userAgentLower.Contains("samsungbrowser"))
      {
        browser = "Samsung Browser";
      }
      else if (userAgentLower.Contains("vivaldi"))
      {
        browser = "Vivaldi";
      }
      else if (userAgentLower.Contains("edg/") || userAgentLower.Contains("edge/"))
      {
        browser = "Edge";
      }
      else if (userAgentLower.Contains("opr/") || userAgentLower.Contains("opera"))
      {
        browser = "Opera";
      }
      else if (userAgentLower.Contains("yabrowser") || userAgentLower.Contains("yandex"))
      {
        browser = "Yandex";
      }
      else if (userAgentLower.Contains("brave"))
      {
        browser = "Brave";
      }
      else if (browser.Contains("Chrome"))
      {
        browser = "Chrome";
      }
      else if (browser.Contains("Firefox"))
      {
        browser = "Firefox";
      }
      else if (browser.Contains("Safari") && !browser.Contains("Chrome"))
      {
        browser = "Safari";
      }
    }

    // Clean up platform names
    if (platform.Contains("Windows"))
    {
      platform = "Windows";
    }
    else if (platform.Contains("Mac") || platform.Contains("macOS"))
    {
      platform = "macOS";
    }
    else if (platform.Contains("Linux"))
    {
      platform = "Linux";
    }
    else if (platform.Contains("Android"))
    {
      platform = "Android";
    }
    else if (platform.Contains("iOS"))
    {
      platform = "iOS";
    }

    var deviceName = $"{platform} - {browser}";

    return (platform, browser, deviceName);
  }

  private void SetRefreshTokenCookie(string refreshToken, bool rememberMe = false)
  {
    var sameSiteMode = Enum.Parse<SameSiteMode>(
      _configuration["Authentication:Cookie:SameSite"] ?? "Strict"
    );

    var cookieOptions = new CookieOptions
    {
      HttpOnly = true,
      Secure = _configuration.GetValue("Authentication:Cookie:Secure", true),
      SameSite = sameSiteMode,
      Path = "/"
    };

    if (rememberMe)
    {
      // Set expiry for RememberMe
      var expiryDays = int.Parse(_configuration["Jwt:RememberMeTokenExpiryInDays"] ?? "30");
      cookieOptions.Expires = DateTime.UtcNow.AddDays(expiryDays);
    }

    Response.Cookies.Append("session", refreshToken, cookieOptions);
  }
}

// Request DTOs
public sealed record ChangePasswordRequest
{
  [Required]
  [DataType(DataType.Password)]
  public required string OldPassword { get; set; }

  [Required]
  [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
  [DataType(DataType.Password)]
  public required string NewPassword { get; set; }

  [DataType(DataType.Password)]
  [Compare("NewPassword", ErrorMessage = "The new password and confirmation password do not match.")]
  public required string ConfirmPassword { get; set; }
}

public sealed record ChangeEmailRequest
{
  [Required]
  [EmailAddress]
  public required string NewEmail { get; set; }
}

public sealed record EnableAuthenticatorRequest
{
  [Required]
  [StringLength(7, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
  [DataType(DataType.Text)]
  public required string Code { get; set; }
}

public sealed record ForgotPasswordRequest
{
  [Required, EmailAddress]
  public required string Email { get; set; }
}

public sealed record LoginRequest
{
  [Required]
  public required string LoginIdentifier { get; set; }
  [Required]
  public required string Password { get; set; }
  public bool RememberMe { get; set; }
}

public sealed record LoginWith2faRequest
{
  [Required]
  public required string UserId { get; set; }
  [Required]
  public required string TwoFactorCode { get; set; }
  public bool RememberMe { get; set; }
  public bool RememberMachine { get; set; }
}

public sealed record LoginWithRecoveryCodeRequest
{
  [Required]
  public required string UserId { get; set; }
  [Required]
  public required string RecoveryCode { get; set; }
}

public sealed record RegisterRequest
{
  [Required]
  public required string Username { get; set; }
  [Required, EmailAddress]
  public required string Email { get; set; }
  [Required, StringLength(100, MinimumLength = 6)]
  public required string Password { get; set; }
}

public sealed record ResendEmailConfirmationRequest
{
  [Required, EmailAddress]
  public required string Email { get; set; }
}

public sealed record ResetPasswordRequest
{
  [Required, EmailAddress]
  public required string Email { get; set; }
  [Required]
  public required string Code { get; set; }
  [Required, StringLength(100, MinimumLength = 6)]
  public required string Password { get; set; }
}

public sealed record UpdateUserNotificationsRequest(
    bool InAppNotification,
    bool EmailNotification,
    bool PushNotification);

public sealed record JwtLoginRequest
{
  [Required]
  public required string LoginIdentifier { get; set; }
  [Required]
  public required string Password { get; set; }
  public bool RememberMe { get; set; }
}

public sealed record RefreshTokenRequest
{
  [Required]
  public required string RefreshToken { get; set; }
}

public sealed record LogoutRequest
{
  [Required]
  public required string RefreshToken { get; set; }
}

public sealed record RevokeDeviceRequest
{
  [Required]
  public required string RefreshToken { get; set; }
}
