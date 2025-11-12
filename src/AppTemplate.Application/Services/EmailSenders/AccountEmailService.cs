using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Configuration;

namespace AppTemplate.Application.Services.EmailSenders;

public class AccountEmailService : IAccountEmailService
{
  private readonly IEmailSender _emailSender;
  private readonly IConfiguration _configuration;

  public AccountEmailService(IEmailSender emailSender, IConfiguration configuration)
  {
    _emailSender = emailSender;
    _configuration = configuration;
  }

  public async Task SendConfirmationEmailAsync(string email, string userId, string code, string username)
  {
    if (_emailSender is AzureEmailSender azureEmailSender)
    {
      await azureEmailSender.SendConfirmationEmailAsync(email, userId, code, username);
    }
    else
    {
      // Fallback for other email senders
      var callbackUrl = GenerateConfirmationUrl(userId, code);
      var htmlMessage = EmailTemplateService.GetEmailConfirmationTemplate(callbackUrl, username);
      await _emailSender.SendEmailAsync(email, "Confirm your email", htmlMessage);
    }
  }

  public async Task SendEmailChangeConfirmationAsync(string newEmail, string userId, string code, string username)
  {
    if (_emailSender is AzureEmailSender azureEmailSender)
    {
      await azureEmailSender.SendEmailChangeConfirmationAsync(newEmail, userId, code, username);
    }
    else
    {
      var callbackUrl = GenerateEmailChangeConfirmationUrl(userId, newEmail, code);
      var htmlMessage = EmailTemplateService.GetEmailChangeConfirmationTemplate(callbackUrl, username, newEmail);
      await _emailSender.SendEmailAsync(newEmail, "Confirm email change", htmlMessage);
    }
  }

  public async Task SendPasswordResetAsync(string email, string code, string username)
  {
    if (_emailSender is AzureEmailSender azureEmailSender)
    {
      await azureEmailSender.SendPasswordResetAsync(email, code, username);
    }
    else
    {
      var callbackUrl = GeneratePasswordResetUrl(email, code);
      var htmlMessage = EmailTemplateService.GetPasswordResetTemplate(callbackUrl, code, username);
      await _emailSender.SendEmailAsync(email, "Reset Password", htmlMessage);
    }
  }

  private string GenerateConfirmationUrl(string userId, string code)
  {
    var baseUrl = _configuration["AppSettings:BaseUrl"] ?? "https://localhost:5001";
    return $"{baseUrl}/Account/ConfirmEmail?userId={userId}&code={code}";
  }

  private string GenerateEmailChangeConfirmationUrl(string userId, string email, string code)
  {
    var baseUrl = _configuration["AppSettings:BaseUrl"] ?? "https://localhost:5001";
    return $"{baseUrl}/Account/ConfirmEmailChange?userId={userId}&email={email}&code={code}";
  }

  private string GeneratePasswordResetUrl(string email, string code)
  {
    var baseUrl = _configuration["AppSettings:BaseUrl"] ?? "https://localhost:5001";
    return $"{baseUrl}/Account/ResetPassword?email={email}&code={code}";
  }
}
