﻿using Ardalis.Result;
using Microsoft.AspNetCore.WebUtilities;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.Encodings.Web;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Myrtus.Clarity.Core.Infrastructure.Authorization;
using Myrtus.Clarity.Core.WebAPI;
using Myrtus.Clarity.Core.WebAPI.Controllers;
using AppTemplate.Application.Enums;
using AppTemplate.Application.Features.Accounts.UpdateNotificationPreferences;
using AppTemplate.Application.Features.AppUsers.Commands.Update.UpdateUserRoles;
using AppTemplate.Application.Features.AppUsers.Queries.GetLoggedInUser;
using AppTemplate.Domain.AppUsers.ValueObjects;
using AppTemplate.Web.Controllers.Api;

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

    // GET: /api/v1.0/account/confirmemail?userId=...&code=...
    [HttpGet("confirmemail")]
    public async Task<IActionResult> ConfirmEmail([FromQuery] string userId, [FromQuery] string code)
    {
        if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(code))
        {
            return BadRequest(new { error = "UserId and code are required." });
        }

        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return NotFound(new { error = $"Unable to load user with ID '{userId}'." });
        }

        code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(code));
        var result = await _userManager.ConfirmEmailAsync(user, code);

        return result.Succeeded
            ? Ok(new { message = "Email confirmed successfully." })
            : BadRequest(new { error = "Error confirming email.", details = result.Errors });
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

    // POST: /api/v1.0/account/forgotpassword
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

    // POST: /api/v1.0/account/loginwith2fa
    [HttpPost("loginwith2fa")]
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

    // POST: /api/v1.0/account/loginwithrecoverycode
    [HttpPost("loginwithrecoverycode")]
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
    //[HasPermission(Permissions.NotificationsRead)]
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
}

// Request DTOs

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
