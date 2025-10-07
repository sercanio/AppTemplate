namespace AppTemplate.Application.Services.EmailSenders;

public interface IAccountEmailService
{
    Task SendConfirmationEmailAsync(string email, string userId, string code, string username);
    Task SendEmailChangeConfirmationAsync(string newEmail, string userId, string code, string username);
    Task SendPasswordResetAsync(string email, string code, string username);
}
