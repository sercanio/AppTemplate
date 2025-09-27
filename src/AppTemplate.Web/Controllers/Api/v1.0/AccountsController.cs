using AppTemplate.Application.Authentication;
using AppTemplate.Application.Authorization;
using AppTemplate.Application.Features.Accounts.UpdateNotificationPreferences;
using AppTemplate.Application.Services.AppUsers;
using AppTemplate.Application.Services.EmailSenders;
using AppTemplate.Application.Services.ErrorHandling;
using AppTemplate.Domain;
using AppTemplate.Domain.AppUsers;
using AppTemplate.Domain.AppUsers.ValueObjects;
using AppTemplate.Web.Controllers.Api;
using Ardalis.Result;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.IdentityModel.Tokens;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace AppTemplate.Web.Controllers;

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
  private readonly IEmailSender _emailSender;
  private readonly IJwtTokenService _jwtTokenService;

  public AccountsController(
      UserManager<IdentityUser> userManager,
      SignInManager<IdentityUser> signInManager,
      IAppUsersService appUsersService,
      IUnitOfWork unitOfWork,
      IEmailSender emailSender,
      IJwtTokenService jwtTokenService,
      ISender sender,
      IErrorHandlingService errorHandlingService
      ) : base(sender, errorHandlingService)
  {
    _userManager = userManager;
    _signInManager = signInManager;
    _emailSender = emailSender;
    _appUsersService = appUsersService;
    _unitOfWork = unitOfWork;
    _jwtTokenService = jwtTokenService;
  }

  private Result<T> ConvertIdentityResult<T>(IdentityResult identityResult, T value = default, string defaultErrorMessage = "Operation failed.")
  {
    if (identityResult.Succeeded)
    {
      return Result.Success(value);
    }

    var errors = identityResult.Errors.Select(e => e.Description).ToList();
    var errorList = new Ardalis.Result.ErrorList(errors);
    return Result.Error(errors.Count > 0 ? errorList : new Ardalis.Result.ErrorList(new[] { defaultErrorMessage }));
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

    code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(code));
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

    await ((AzureEmailSender)_emailSender).SendEmailChangeConfirmationAsync(
        request.NewEmail,
        userId,
        code,
        user.UserName);

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

    await ((AzureEmailSender)_emailSender).SendConfirmationEmailAsync(
        email,
        userId,
        code,
        user.UserName);

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

    await ((AzureEmailSender)_emailSender).SendConfirmationEmailAsync(
        request.Email,
        userId,
        code,
        user.UserName);

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

    code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(code));
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
    if (user == null || !(await _userManager.IsEmailConfirmedAsync(user)))
    {
      // To prevent user enumeration, always return success.
      return Ok(new { message = "Verification email sent. Please check your email." });
    }

    var code = await _userManager.GeneratePasswordResetTokenAsync(user);
    code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));

    await ((AzureEmailSender)_emailSender).SendPasswordResetAsync(
        request.Email,
        code,
        user.UserName);

    return Ok(new { message = "Verification email sent. Please check your email." });
  }

  // POST: /api/v1.0/account/login
  [HttpPost("login")]
  public async Task<IActionResult> LoginWithJwt([FromBody] JwtLoginRequest request)
  {
    if (!ModelState.IsValid)
      return BadRequest(ModelState);

    var user = await _userManager.FindByEmailAsync(request.LoginIdentifier)
               ?? await _userManager.FindByNameAsync(request.LoginIdentifier);

    if (user == null)
      return BadRequest(new { error = "Invalid credentials" });

    var result = await _signInManager.CheckPasswordSignInAsync(user, request.Password, lockoutOnFailure: false);
    if (!await _userManager.CheckPasswordAsync(user, request.Password))
      return BadRequest(new { error = "Invalid credentials" });

    if (result.Succeeded)
    {
      if (!await _userManager.IsEmailConfirmedAsync(user))
        return BadRequest(new { error = "Email not confirmed" });

      var appUser = await _appUsersService.GetByIdentityIdAsync(user.Id);
      if (appUser == null)
        return BadRequest(new { error = "App user not found" });

      // Check if 2FA is required
      if (await _userManager.GetTwoFactorEnabledAsync(user))
      {
        return Ok(new
        {
          requiresTwoFactor = true,
          userId = user.Id
        });
      }

      var tokens = await _jwtTokenService.GenerateTokensAsync(user, appUser.Value);
      return Ok(tokens);
    }

    if (result.RequiresTwoFactor)
      return Ok(new { requiresTwoFactor = true, userId = user.Id });

    if (result.IsLockedOut)
      return BadRequest(new { error = "Account locked" });

    return BadRequest(new { error = "Invalid credentials" });
  }

  [HttpPost("refresh-token")]
  public async Task<IActionResult> RefreshJwtToken([FromBody] RefreshTokenRequest request)
  {
    try
    {
      var tokens = await _jwtTokenService.RefreshTokensAsync(request.RefreshToken);
      return Ok(tokens);
    }
    catch (SecurityTokenValidationException ex)
    {
      return BadRequest(new { error = ex.Message });
    }
  }

  [HttpPost("logout")]
  [Authorize(AuthenticationSchemes = "Bearer")]
  public async Task<IActionResult> LogoutJwt([FromBody] LogoutRequest request)
  {
    await _jwtTokenService.RevokeRefreshTokenAsync(request.RefreshToken);
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

  [HttpPost("register")]
  public async Task<IActionResult> Register([FromBody] RegisterRequest request)
  {
    if (!ModelState.IsValid)
      return BadRequest(ModelState);

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
    await ((AzureEmailSender)_emailSender).SendConfirmationEmailAsync(
        request.Email,
        identityId,
        code,
        request.Username);

    return Ok(new { message = "User registered successfully. Please confirm your email." });
  }
}

// Request DTOs
public class ChangePasswordRequest
{
  [Required]
  [DataType(DataType.Password)]
  public string OldPassword { get; set; }

  [Required]
  [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
  [DataType(DataType.Password)]
  public string NewPassword { get; set; }

  [DataType(DataType.Password)]
  [Compare("NewPassword", ErrorMessage = "The new password and confirmation password do not match.")]
  public string ConfirmPassword { get; set; }
}

public class ChangeEmailRequest
{
  [Required]
  [EmailAddress]
  public string NewEmail { get; set; }
}

public class EnableAuthenticatorRequest
{
  [Required]
  [StringLength(7, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
  [DataType(DataType.Text)]
  public string Code { get; set; }
}

public class ForgotPasswordRequest
{
  [Required, EmailAddress]
  public string Email { get; set; }
}

public class LoginRequest
{
  [Required]
  public string LoginIdentifier { get; set; }
  [Required]
  public string Password { get; set; }
  public bool RememberMe { get; set; }
}

public class LoginWith2faRequest
{
  [Required]
  public string UserId { get; set; }
  [Required]
  public string TwoFactorCode { get; set; }
  public bool RememberMe { get; set; }
  public bool RememberMachine { get; set; }
}

public class LoginWithRecoveryCodeRequest
{
  [Required]
  public string UserId { get; set; }
  [Required]
  public string RecoveryCode { get; set; }
}

public class RegisterRequest
{
  [Required]
  public string Username { get; set; }
  [Required, EmailAddress]
  public string Email { get; set; }
  [Required, StringLength(100, MinimumLength = 6)]
  public string Password { get; set; }
}

public class ResendEmailConfirmationRequest
{
  [Required, EmailAddress]
  public string Email { get; set; }
}

public class ResetPasswordRequest
{
  [Required, EmailAddress]
  public string Email { get; set; }
  [Required]
  public string Code { get; set; }
  [Required, StringLength(100, MinimumLength = 6)]
  public string Password { get; set; }
}

public sealed record UpdateUserNotificationsRequest(
    bool InAppNotification,
    bool EmailNotification,
    bool PushNotification);

public class JwtLoginRequest
{
  [Required]
  public string LoginIdentifier { get; set; }
  [Required]
  public string Password { get; set; }
}

public class RefreshTokenRequest
{
  [Required]
  public string RefreshToken { get; set; }
}

public class LogoutRequest
{
  [Required]
  public string RefreshToken { get; set; }
}
