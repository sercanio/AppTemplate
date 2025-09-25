using AppTemplate.Application.Authentication;
using AppTemplate.Application.Authentication.Jwt;
using AppTemplate.Application.Services.AppUsers;
using AppTemplate.Application.Services.ErrorHandling;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.IdentityModel.Tokens;

namespace AppTemplate.Web.Controllers.Api;

[ApiController]
[IgnoreAntiforgeryToken]
[ApiVersion(ApiVersions.V1)]
[Route("api/v{version:apiVersion}/[controller]")]
[EnableRateLimiting("Fixed")]
public class AuthController : BaseController
{
  private readonly UserManager<IdentityUser> _userManager;
  private readonly SignInManager<IdentityUser> _signInManager;
  private readonly IJwtTokenService _jwtTokenService;
  private readonly IAppUsersService _appUsersService;

  public AuthController(
      UserManager<IdentityUser> userManager,
      SignInManager<IdentityUser> signInManager,
      IJwtTokenService jwtTokenService,
      IAppUsersService appUsersService,
      ISender sender,
      IErrorHandlingService errorHandlingService)
      : base(sender, errorHandlingService)
  {
    _userManager = userManager;
    _signInManager = signInManager;
    _jwtTokenService = jwtTokenService;
    _appUsersService = appUsersService;
  }

  [HttpPost("login")]
  public async Task<IActionResult> Login([FromBody] JwtLoginRequest request)
  {
    if (!ModelState.IsValid)
      return BadRequest(ModelState);

    var user = await _userManager.FindByEmailAsync(request.LoginIdentifier)
               ?? await _userManager.FindByNameAsync(request.LoginIdentifier);

    if (user == null)
      return BadRequest(new { error = "Invalid credentials" });

    var result = await _signInManager.CheckPasswordSignInAsync(user, request.Password, lockoutOnFailure: false);

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
        // For JWT, we need to handle 2FA differently
        // You might want to return a temporary token that requires 2FA completion
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

  [HttpPost("refresh")]
  public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
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
  public async Task<IActionResult> Logout([FromBody] LogoutRequest request)
  {
    await _jwtTokenService.RevokeRefreshTokenAsync(request.RefreshToken);
    return Ok(new { message = "Logged out successfully" });
  }

  [HttpPost("revoke-all")]
  [Authorize(AuthenticationSchemes = "Bearer")]
  public async Task<IActionResult> RevokeAllTokens()
  {
    var userId = User.GetIdentityId();
    await _jwtTokenService.RevokeAllUserRefreshTokensAsync(userId);
    return Ok(new { message = "All tokens revoked successfully" });
  }
}

public sealed record JwtLoginRequest(
    string LoginIdentifier,
    string Password);

public sealed record RefreshTokenRequest(
    string RefreshToken);

public sealed record LogoutRequest(
    string RefreshToken);