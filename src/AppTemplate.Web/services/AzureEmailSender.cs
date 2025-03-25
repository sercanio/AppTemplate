using Azure.Communication.Email;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;

namespace AppTemplate.Web.services;

public class AzureEmailSender : IEmailSender
{
    private readonly EmailClient _emailClient;
    private readonly string _fromEmail;

    public AzureEmailSender(IConfiguration configuration)
    {
        var connectionString = configuration["AzureCommunicationService:ConnectionString"];
        _emailClient = new EmailClient(connectionString);
        _fromEmail = configuration["AzureCommunicationService:FromEmail"];
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
}