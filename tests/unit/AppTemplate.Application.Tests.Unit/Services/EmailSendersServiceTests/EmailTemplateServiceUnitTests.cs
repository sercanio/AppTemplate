using AppTemplate.Application.Services.EmailSenders;

namespace AppTemplate.Application.Tests.Unit.Services.EmailSendersServiceTests;

[Trait("Category", "Unit")]
public class EmailTemplateServiceUnitTests
{
  [Fact]
  public void GetEmailConfirmationTemplate_IncludesCallbackUrlAndUsername()
  {
    var url = "https://example.com/confirm";
    var username = "TestUser";
    var html = EmailTemplateService.GetEmailConfirmationTemplate(url, username);

    Assert.Contains(url, html);
    Assert.Contains(username, html);
    Assert.Contains("Confirm Your Email", html);
  }

  [Fact]
  public void GetEmailChangeConfirmationTemplate_IncludesCallbackUrlUsernameAndNewEmail()
  {
    var url = "https://example.com/change";
    var username = "TestUser";
    var newEmail = "new@example.com";
    var html = EmailTemplateService.GetEmailChangeConfirmationTemplate(url, username, newEmail);

    Assert.Contains(url, html);
    Assert.Contains(username, html);
    Assert.Contains(newEmail, html);
    Assert.Contains("Confirm Email Change", html);
  }

  [Fact]
  public void GetPasswordResetTemplate_IncludesCallbackUrlCodeAndUsername()
  {
    var url = "https://example.com/reset";
    var code = "123456";
    var username = "TestUser";
    var html = EmailTemplateService.GetPasswordResetTemplate(url, code, username);

    Assert.Contains(url, html);
    Assert.Contains(code, html);
    Assert.Contains(username, html);
    Assert.Contains("Reset Your Password", html);
  }
}
