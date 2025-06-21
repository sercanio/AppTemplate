using AppTemplate.Application.Features.Accounts.UpdateNotificationPreferences;
using AppTemplate.Application.Features.AppUsers.Queries.GetLoggedInUser;
using AppTemplate.Domain.AppUsers.ValueObjects;
using AppTemplate.Web.Attributes;
using AppTemplate.Web.Controllers.Api;
using Ardalis.Result;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.WebUtilities;
using Myrtus.Clarity.Core.Infrastructure.Authorization;
using Myrtus.Clarity.Core.WebAPI;
using Myrtus.Clarity.Core.WebAPI.Controllers;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.Encodings.Web;

namespace AppTemplate.Web.Controllers;

[ApiController]
[ApiVersion(ApiVersions.V1)]
[Route("api/v{version:apiVersion}/[controller]")]
[EnableRateLimiting("Fixed")]
public class AccountController : BaseController
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly SignInManager<IdentityUser> _signInManager;
    private readonly IEmailSender _emailSender;

    public AccountController(
        UserManager<IdentityUser> userManager,
        SignInManager<IdentityUser> signInManager,
        IEmailSender emailSender,
        ISender sender, IErrorHandlingService errorHandlingService
        ) : base(sender, errorHandlingService)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _emailSender = emailSender;
    }

    private Result<T> ConvertIdentityResult<T>(IdentityResult identityResult, T value = default, string defaultErrorMessage = "Operation failed.")
    {
        if (identityResult.Succeeded)
        {
            return Result.Success(value);
        }

        var errors = identityResult.Errors.Select(e => e.Description).ToList();
        // Fix for CS1503: Convert List<string> to ErrorList using the appropriate constructor or method
        var errorList = new Ardalis.Result.ErrorList(errors);
        // Fix for CA1860: Use Count == 0 instead of Any()
        return Result.Error(errors.Count > 0 ? errorList : new Ardalis.Result.ErrorList(new[] { defaultErrorMessage }));
    }

    // GET: /api/v1.0/account/confirmemail?userId=...&code=...
    [HttpGet("confirmemail")]
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

    // GET: /api/v1.0/account/confirmemailchange?userId=...&email=...&code=...
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

        var setNameResult = await _userManager.SetUserNameAsync(user, email);
        if (!setNameResult.Succeeded)
        {
            return BadRequest(new { error = "Error updating username.", details = setNameResult.Errors });
        }

        await _signInManager.RefreshSignInAsync(user);
        return Ok(new { message = "Email change confirmed." });
    }

    [HttpPost("changepassword")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return _errorHandlingService.HandleErrorResponse(Result.NotFound("Unable to load user."));
        }

        var identityResult = await _userManager.ChangePasswordAsync(user, request.OldPassword, request.NewPassword);
        var result = ConvertIdentityResult(identityResult, "Your password has been changed successfully.", "Password change failed.");

        if (result.IsSuccess)
        {
            await _signInManager.RefreshSignInAsync(user);
        }

        return result.IsSuccess
            ? Ok(new { message = result.Value })
            : _errorHandlingService.HandleErrorResponse(result);
    }

    // POST: /api/v1.0/account/forgotpassword
    [HttpPost("forgotpassword")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
    {
        if (string.IsNullOrEmpty(request.Email))
        {
            return _errorHandlingService.HandleErrorResponse(Result.Invalid());
        }

        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user == null || !(await _userManager.IsEmailConfirmedAsync(user)))
        {
            // To prevent user enumeration, always return success.
            return Ok(new { message = "Verification email sent. Please check your email." });
        }

        var code = await _userManager.GeneratePasswordResetTokenAsync(user);
        code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
        var callbackUrl = Url.Action("ResetPassword", "Account", new { code }, Request.Scheme);

        await _emailSender.SendEmailAsync(request.Email, "Reset Password",
            $"Please reset your password by clicking here: {HtmlEncoder.Default.Encode(callbackUrl)}");

        return Ok(new { message = "Verification email sent. Please check your email." });
    }

    // POST: /api/v1.0/account/login
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        IdentityUser user = await _userManager.FindByEmailAsync(request.LoginIdentifier)
                              ?? await _userManager.FindByNameAsync(request.LoginIdentifier);

        if (user == null)
        {
            return BadRequest(new { error = "User not found." });
        }

        var result = await _signInManager.PasswordSignInAsync(user, request.Password, request.RememberMe, lockoutOnFailure: false);
        if (result.Succeeded)
        {
            return Ok(new { message = "Login successful." });
        }
        if (result.RequiresTwoFactor)
        {
            return Ok(new { message = "Two-factor authentication required." });
        }
        if (result.IsLockedOut)
        {
            return BadRequest(new { error = "User account locked out." });
        }
        return BadRequest(new { error = "Invalid password." });
    }

    // GET: /api/v1.0/account/2fa/status
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
            hasAuthenticator = hasAuthenticator,
            recoveryCodesLeft = recoveryCodesLeft,
            isMachineRemembered = isMachineRemembered
        });
    }

    // POST: /api/v1.0/account/2fa/disable
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

    // POST: /api/v1.0/account/2fa/forget-browser
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

    // GET: /api/v1.0/account/2fa/authenticator
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

        var sharedKey = FormatKey(unformattedKey);
        var email = await _userManager.GetEmailAsync(user);
        var authenticatorUri = GenerateQrCodeUri(email, unformattedKey);

        return Ok(new
        {
            sharedKey,
            authenticatorUri
        });
    }

    // POST: /api/v1.0/account/2fa/authenticator
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
            recoveryCodes.AddRange(newRecoveryCodes);
        }

        return Ok(new
        {
            message = "Your authenticator app has been verified.",
            recoveryCodes = recoveryCodes.Count > 0 ? recoveryCodes : null
        });
    }

    // POST: /api/v1.0/account/2fa/authenticator/reset
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
        await _signInManager.RefreshSignInAsync(user);

        return Ok(new { message = "Your authenticator app key has been reset." });
    }

    // POST: /api/v1.0/account/2fa/recovery-codes/generate
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

        return Ok(new
        {
            recoveryCodes = recoveryCodes.ToArray(),
            message = "You have generated new recovery codes."
        });
    }

    // POST: /api/v1.0/account/2fa/login
    [HttpPost("2fa/login")]
    public async Task<IActionResult> LoginWith2fa([FromBody] LoginWith2faRequest request)
    {
        var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();
        if (user == null)
        {
            return BadRequest(new { error = "Unable to load two-factor authentication user." });
        }

        var code = request.TwoFactorCode.Replace(" ", string.Empty).Replace("-", string.Empty);
        var result = await _signInManager.TwoFactorAuthenticatorSignInAsync(code, request.RememberMe, request.RememberMachine);

        if (result.Succeeded)
        {
            return Ok(new { message = "Two-factor authentication login successful." });
        }
        if (result.IsLockedOut)
        {
            return BadRequest(new { error = "User account locked out." });
        }
        return BadRequest(new { error = "Invalid authenticator code." });
    }

    // POST: /api/v1.0/account/2fa/login-recovery
    [HttpPost("2fa/login-recovery")]
    public async Task<IActionResult> LoginWithRecoveryCode([FromBody] LoginWithRecoveryCodeRequest request)
    {
        var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();
        if (user == null)
        {
            return BadRequest(new { error = "Unable to load two-factor authentication user." });
        }

        var recoveryCode = request.RecoveryCode.Replace(" ", string.Empty);
        var result = await _signInManager.TwoFactorRecoveryCodeSignInAsync(recoveryCode);

        if (result.Succeeded)
        {
            return Ok(new { message = "Logged in with recovery code." });
        }
        if (result.IsLockedOut)
        {
            return BadRequest(new { error = "User account locked out." });
        }
        return BadRequest(new { error = "Invalid recovery code." });
    }
    [IgnoreAntiforgeryToken]
    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        return Ok(new { message = "User logged out." });
    }

    // POST: /api/v1.0/account/register
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var user = new IdentityUser { UserName = request.Username, Email = request.Email };
        var result = await _userManager.CreateAsync(user, request.Password);

        if (result.Succeeded)
        {
            var userId = await _userManager.GetUserIdAsync(user);
            var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
            var callbackUrl = Url.Action("ConfirmEmail", "Account", new { userId, code }, Request.Scheme);

            await _emailSender.SendEmailAsync(request.Email, "Confirm your email",
                $"Confirm your account by clicking here: {HtmlEncoder.Default.Encode(callbackUrl)}");

            return Ok(new { message = "User registered successfully. Please confirm your email." });
        }
        return BadRequest(new { error = "Registration failed.", details = result.Errors });
    }

    // POST: /api/v1.0/account/resendemailconfirmation
    [HttpPost("resendemailconfirmation")]
    [Authorize]
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
        var callbackUrl = Url.Action("ConfirmEmail", "Account", new { userId, code }, Request.Scheme);

        await _emailSender.SendEmailAsync(request.Email, "Confirm your email",
            $"Confirm your account by clicking here: {HtmlEncoder.Default.Encode(callbackUrl)}");

        return Ok(new { message = "Verification email sent. Please check your email." });
    }

    // POST: /api/v1.0/account/resetpassword
    [HttpPost("resetpassword")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user == null)
        {
            // Do not reveal that the user does not exist.
            return Ok(new { message = "Password reset successful. Please check your email for further instructions." });
        }

        var result = await _userManager.ResetPasswordAsync(user, request.Code, request.Password);
        return result.Succeeded
            ? Ok(new { message = "Password reset successful." })
            : BadRequest(new { error = "Password reset failed.", details = result.Errors });
    }

    [IgnoreAntiforgeryToken]
    [HttpGet("me")]
    public async Task<IActionResult> GetCurrentUser([FromQuery] Guid? id)
    {
        var query = new GetLoggedInUserQuery();
        var result = await _sender.Send(query);
        if (result.IsSuccess)
        {
            return Ok(result.Value);
        }
        return _errorHandlingService.HandleErrorResponse(result);
    }

    [HttpPatch("me/notifications")]
    [HasPermission(Permissions.NotificationsUpdate)]
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
    public string TwoFactorCode { get; set; }
    public bool RememberMe { get; set; }
    public bool RememberMachine { get; set; }
}

public class LoginWithRecoveryCodeRequest
{
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
