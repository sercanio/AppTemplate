using Azure.Communication.Email;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Configuration;

namespace AppTemplate.Application.Services.EmailSenders;

public class AzureEmailSender : IEmailSender
{
    private readonly EmailClient _emailClient;
    private readonly string _fromEmail;
    private readonly EmailTemplateService _templateService;
    private readonly IConfiguration _configuration;

    public AzureEmailSender(IConfiguration configuration, EmailTemplateService templateService)
    {
        _configuration = configuration;
        var connectionString = configuration["AzureCommunicationService:ConnectionString"];
        _emailClient = new EmailClient(connectionString);
        _fromEmail = configuration["AzureCommunicationService:FromEmail"];
        _templateService = templateService;
    }

    public async Task SendEmailAsync(string email, string subject, string htmlMessage)
    {
        var emailContent = new EmailContent(subject)
        {
            Html = htmlMessage
        };
        var emailMessage = new EmailMessage(_fromEmail, email, emailContent);
        await _emailClient.SendAsync(Azure.WaitUntil.Completed, emailMessage);
    }

    public async Task SendConfirmationEmailAsync(string email, string userId, string code, string username = "")
    {
        string callbackUrl = GetFrontendUrl("confirm-email", new { userId, code });
        string htmlMessage = EmailTemplateService.GetEmailConfirmationTemplate(callbackUrl, username);
        await SendEmailAsync(email, "Confirm your email", htmlMessage);
    }

    public async Task SendEmailChangeConfirmationAsync(string email, string userId, string code, string username = "")
    {
        string callbackUrl = GetFrontendUrl("confirm-email", new { userId, email, code });
        string htmlMessage = EmailTemplateService.GetEmailChangeConfirmationTemplate(callbackUrl, username, email);
        await SendEmailAsync(email, "Confirm your email change", htmlMessage);
    }

    public async Task SendPasswordResetAsync(string email, string code, string username = "")
    {
        string callbackUrl = GetFrontendUrl("auth/reset-password", new { email, code, username });
        string htmlMessage = EmailTemplateService.GetPasswordResetTemplate(callbackUrl, username);
        await SendEmailAsync(email, "Reset your password", htmlMessage);
    }

    private string GetFrontendUrl(string path, object values)
    {
        var frontendUrl = _configuration["Frontend:BaseUrl"] ?? "http://localhost:3000";
        frontendUrl = frontendUrl.TrimEnd('/');
        path = path.TrimStart('/');

        // Build the query string
        var queryParams = new List<string>();
        foreach (var prop in values.GetType().GetProperties())
        {
            var value = prop.GetValue(values);
            if (value != null)
            {
                queryParams.Add($"{prop.Name}={Uri.EscapeDataString(value.ToString())}");
            }
        }

        var queryString = queryParams.Count > 0 ? "?" + string.Join("&", queryParams) : "";

        return $"{frontendUrl}/{path}{queryString}";
    }
}