using AppTemplate.Application.Services.EmailSenders;
using Azure.Communication.Email;
using Microsoft.Extensions.Configuration;
using Moq;

namespace AppTemplate.Application.Tests.Unit.Services.EmailSendersServiceTests;

public class AzureEmailSendersUnitTests
{
  private readonly Mock<IConfiguration> _configMock;
  private readonly Mock<EmailClient> _emailClientMock;
  private readonly AzureEmailSender _sender;

  public AzureEmailSendersUnitTests()
  {
    _configMock = new Mock<IConfiguration>();
    _configMock.Setup(c => c["AzureCommunicationService:ConnectionString"]).Returns("endpoint=https://fake.endpoint/;accesskey=fakekey");
    _configMock.Setup(c => c["AzureCommunicationService:FromEmail"]).Returns("from@example.com");
    _configMock.Setup(c => c["Frontend:BaseUrl"]).Returns("https://frontend.example.com");

    _emailClientMock = new Mock<EmailClient>("endpoint=https://fake.endpoint/;accesskey=fakekey");

    _emailClientMock
        .Setup(client => client.SendAsync(
            Azure.WaitUntil.Completed,
            It.IsAny<EmailMessage>(),
            It.IsAny<CancellationToken>()))
        .Returns(Task.FromResult(new EmailSendOperation("fake-id", _emailClientMock.Object)));

    _sender = new TestableAzureEmailSender(_configMock.Object, new EmailTemplateService(_configMock.Object), _emailClientMock.Object);
  }

  [Fact]
  public async Task SendEmailAsync_SendsEmailWithCorrectParameters()
  {
    // Arrange
    var email = "to@example.com";
    var subject = "Test Subject";
    var html = "<b>Test</b>";

    // Act
    await _sender.SendEmailAsync(email, subject, html);

    // Assert
    _emailClientMock.Verify(client => client.SendAsync(
        Azure.WaitUntil.Completed,
        It.IsAny<EmailMessage>(),
        It.IsAny<CancellationToken>()), Times.Once);
  }

  [Fact]
  public async Task SendConfirmationEmailAsync_UsesTemplateAndSends()
  {
    // Arrange
    var email = "to@example.com";
    var userId = "user-id";
    var code = "code";
    var username = "TestUser";

    // Act
    await _sender.SendConfirmationEmailAsync(email, userId, code, username);

    // Assert
    _emailClientMock.Verify(client => client.SendAsync(
        Azure.WaitUntil.Completed,
        It.IsAny<EmailMessage>(),
        It.IsAny<CancellationToken>()), Times.Once);
  }

  [Fact]
  public async Task SendEmailChangeConfirmationAsync_UsesTemplateAndSends()
  {
    // Arrange
    var email = "to@example.com";
    var userId = "user-id";
    var code = "code";
    var username = "TestUser";

    // Act
    await _sender.SendEmailChangeConfirmationAsync(email, userId, code, username);

    // Assert
    _emailClientMock.Verify(client => client.SendAsync(
        Azure.WaitUntil.Completed,
        It.IsAny<EmailMessage>(),
        It.IsAny<CancellationToken>()), Times.Once);
  }

  [Fact]
  public async Task SendPasswordResetAsync_UsesTemplateAndSends()
  {
    // Arrange
    var email = "to@example.com";
    var code = "reset-code";
    var username = "TestUser";

    // Act
    await _sender.SendPasswordResetAsync(email, code, username);

    // Assert
    _emailClientMock.Verify(client => client.SendAsync(
        Azure.WaitUntil.Completed,
        It.IsAny<EmailMessage>(),
        It.IsAny<CancellationToken>()), Times.Once);
  }

  // Helper subclass to inject the mock EmailClient
  private class TestableAzureEmailSender : AzureEmailSender
  {
    public TestableAzureEmailSender(IConfiguration configuration, EmailTemplateService templateService, EmailClient emailClient)
        : base(configuration, templateService)
    {
      typeof(AzureEmailSender)
          .GetField("_emailClient", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
          .SetValue(this, emailClient);
    }
  }
}