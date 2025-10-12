using AppTemplate.Domain.Mailing;
using MimeKit;

namespace AppTemplate.Domain.Tests.Unit.Mailing;

[Trait("Category", "Unit")]
public class MailUnitTests
{
  [Fact]
  public void Constructor_WithSubjectOnly_ShouldInitializeWithDefaultValues()
  {
    // Arrange
    var subject = "Test Subject";

    // Act
    var mail = new Mail(subject);

    // Assert
    Assert.Equal(string.Empty, mail.Subject);
    Assert.Equal(string.Empty, mail.TextBody);
    Assert.Equal(string.Empty, mail.HtmlBody);
    Assert.Empty(mail.ToList);
    Assert.Null(mail.CcList);
    Assert.Null(mail.BccList);
    Assert.Null(mail.Attachments);
    Assert.Null(mail.UnsubscribeLink);
  }

  [Fact]
  public void Constructor_WithAllParameters_ShouldSetAllProperties()
  {
    // Arrange
    var subject = "Test Subject";
    var textBody = "Plain text body";
    var htmlBody = "<p>HTML body</p>";
    var toList = new List<MailboxAddress>
    {
      new MailboxAddress("John Doe", "john@example.com"),
      new MailboxAddress("Jane Doe", "jane@example.com")
    };

    // Act
    var mail = new Mail(subject, textBody, htmlBody, toList);

    // Assert
    Assert.Equal(subject, mail.Subject);
    Assert.Equal(textBody, mail.TextBody);
    Assert.Equal(htmlBody, mail.HtmlBody);
    Assert.Equal(toList, mail.ToList);
    Assert.Equal(2, mail.ToList.Count);
  }

  [Fact]
  public void Constructor_WithEmptySubject_ShouldSetEmptySubject()
  {
    // Arrange
    var subject = string.Empty;

    // Act
    var mail = new Mail(subject);

    // Assert
    Assert.Equal(string.Empty, mail.Subject);
  }

  [Fact]
  public void Constructor_WithNullSubject_ShouldHandleGracefully()
  {
    // Arrange
    string? subject = null;

    // Act
    var mail = new Mail(subject!);

    // Assert
    Assert.Equal(string.Empty, mail.Subject);
  }

  [Fact]
  public void Properties_CanBeSetAndRetrieved()
  {
    // Arrange
    var mail = new Mail("Test");
    var subject = "Updated Subject";
    var textBody = "Updated text body";
    var htmlBody = "<p>Updated HTML body</p>";
    var unsubscribeLink = "https://example.com/unsubscribe";

    // Act
    mail.Subject = subject;
    mail.TextBody = textBody;
    mail.HtmlBody = htmlBody;
    mail.UnsubscribeLink = unsubscribeLink;

    // Assert
    Assert.Equal(subject, mail.Subject);
    Assert.Equal(textBody, mail.TextBody);
    Assert.Equal(htmlBody, mail.HtmlBody);
    Assert.Equal(unsubscribeLink, mail.UnsubscribeLink);
  }

  [Fact]
  public void ToList_CanAddRecipients()
  {
    // Arrange
    var mail = new Mail("Test");
    var recipient1 = new MailboxAddress("User 1", "user1@example.com");
    var recipient2 = new MailboxAddress("User 2", "user2@example.com");

    // Act
    mail.ToList.Add(recipient1);
    mail.ToList.Add(recipient2);

    // Assert
    Assert.Equal(2, mail.ToList.Count);
    Assert.Contains(recipient1, mail.ToList);
    Assert.Contains(recipient2, mail.ToList);
  }

  [Fact]
  public void CcList_CanBeSetWithRecipients()
  {
    // Arrange
    var mail = new Mail("Test");
    var ccList = new List<MailboxAddress>
    {
      new MailboxAddress("CC User 1", "cc1@example.com"),
      new MailboxAddress("CC User 2", "cc2@example.com")
    };

    // Act
    mail.CcList = ccList;

    // Assert
    Assert.NotNull(mail.CcList);
    Assert.Equal(2, mail.CcList.Count);
  }

  [Fact]
  public void BccList_CanBeSetWithRecipients()
  {
    // Arrange
    var mail = new Mail("Test");
    var bccList = new List<MailboxAddress>
    {
      new MailboxAddress("BCC User 1", "bcc1@example.com"),
      new MailboxAddress("BCC User 2", "bcc2@example.com")
    };

    // Act
    mail.BccList = bccList;

    // Assert
    Assert.NotNull(mail.BccList);
    Assert.Equal(2, mail.BccList.Count);
  }

  [Fact]
  public void Attachments_CanBeSetAndRetrieved()
  {
    // Arrange
    var mail = new Mail("Test");
    var attachments = new AttachmentCollection();

    // Act
    mail.Attachments = attachments;

    // Assert
    Assert.NotNull(mail.Attachments);
    Assert.Same(attachments, mail.Attachments);
  }

  [Fact]
  public void Constructor_WithEmptyToList_ShouldSetEmptyList()
  {
    // Arrange
    var subject = "Test";
    var textBody = "Body";
    var htmlBody = "<p>Body</p>";
    var emptyToList = new List<MailboxAddress>();

    // Act
    var mail = new Mail(subject, textBody, htmlBody, emptyToList);

    // Assert
    Assert.Empty(mail.ToList);
  }

  [Fact]
  public void Constructor_WithLargeToList_ShouldHandleAllRecipients()
  {
    // Arrange
    var subject = "Bulk Email";
    var textBody = "Message";
    var htmlBody = "<p>Message</p>";
    var toList = new List<MailboxAddress>();
    for (int i = 0; i < 100; i++)
    {
      toList.Add(new MailboxAddress($"User {i}", $"user{i}@example.com"));
    }

    // Act
    var mail = new Mail(subject, textBody, htmlBody, toList);

    // Assert
    Assert.Equal(100, mail.ToList.Count);
  }

  [Fact]
  public void Subject_WithSpecialCharacters_ShouldPreserveContent()
  {
    // Arrange
    var subject = "Test Subject: <Special> & \"Quoted\" 'Text' @#$%";
    var mail = new Mail(subject);

    // Act
    mail.Subject = subject;

    // Assert
    Assert.Equal(subject, mail.Subject);
  }

  [Fact]
  public void TextBody_WithMultilineContent_ShouldPreserveNewlines()
  {
    // Arrange
    var mail = new Mail("Test");
    var textBody = "Line 1\nLine 2\nLine 3\n";

    // Act
    mail.TextBody = textBody;

    // Assert
    Assert.Equal(textBody, mail.TextBody);
    Assert.Contains("\n", mail.TextBody);
  }

  [Fact]
  public void HtmlBody_WithComplexHtml_ShouldPreserveMarkup()
  {
    // Arrange
    var mail = new Mail("Test");
    var htmlBody = @"<!DOCTYPE html>
<html>
<head><title>Test</title></head>
<body>
  <h1>Hello</h1>
  <p style=""color: red;"">This is a test</p>
  <a href=""https://example.com"">Link</a>
</body>
</html>";

    // Act
    mail.HtmlBody = htmlBody;

    // Assert
    Assert.Equal(htmlBody, mail.HtmlBody);
    Assert.Contains("<!DOCTYPE html>", mail.HtmlBody);
  }

  [Fact]
  public void UnsubscribeLink_WithValidUrl_ShouldSetCorrectly()
  {
    // Arrange
    var mail = new Mail("Test");
    var unsubscribeLink = "https://example.com/unsubscribe?token=abc123";

    // Act
    mail.UnsubscribeLink = unsubscribeLink;

    // Assert
    Assert.Equal(unsubscribeLink, mail.UnsubscribeLink);
  }

  [Fact]
  public void UnsubscribeLink_WhenSetToNull_ShouldBeNull()
  {
    // Arrange
    var mail = new Mail("Test")
    {
      UnsubscribeLink = "https://example.com/unsubscribe"
    };

    // Act
    mail.UnsubscribeLink = null;

    // Assert
    Assert.Null(mail.UnsubscribeLink);
  }

  [Theory]
  [InlineData("Welcome to our service")]
  [InlineData("Password Reset Request")]
  [InlineData("Your order confirmation #12345")]
  [InlineData("")]
  public void Constructor_WithVariousSubjects_ShouldInitializeCorrectly(string subject)
  {
    // Act
    var mail = new Mail(subject);

    // Assert
    Assert.NotNull(mail);
    Assert.Empty(mail.ToList);
  }

  [Fact]
  public void ToList_InitializedAsEmptyCollection_ShouldNotBeNull()
  {
    // Arrange
    var mail = new Mail("Test");

    // Act & Assert
    Assert.NotNull(mail.ToList);
    Assert.IsType<List<MailboxAddress>>(mail.ToList);
  }

  [Fact]
  public void CcList_DefaultValue_ShouldBeNull()
  {
    // Arrange & Act
    var mail = new Mail("Test");

    // Assert
    Assert.Null(mail.CcList);
  }

  [Fact]
  public void BccList_DefaultValue_ShouldBeNull()
  {
    // Arrange & Act
    var mail = new Mail("Test");

    // Assert
    Assert.Null(mail.BccList);
  }

  [Fact]
  public void Attachments_DefaultValue_ShouldBeNull()
  {
    // Arrange & Act
    var mail = new Mail("Test");

    // Assert
    Assert.Null(mail.Attachments);
  }

  [Fact]
  public void Constructor_WithUnicodeCharacters_ShouldPreserveContent()
  {
    // Arrange
    var subject = "テスト件名 测试主题 тестовая тема";
    var textBody = "Unicode content: こんにちは 你好 Привет";
    var htmlBody = "<p>Unicode: こんにちは 你好 Привет</p>";
    var toList = new List<MailboxAddress>
    {
      new MailboxAddress("テストユーザー", "test@example.com")
    };

    // Act
    var mail = new Mail(subject, textBody, htmlBody, toList);

    // Assert
    Assert.Equal(subject, mail.Subject);
    Assert.Equal(textBody, mail.TextBody);
    Assert.Equal(htmlBody, mail.HtmlBody);
  }

  [Fact]
  public void Mail_MultipleInstances_ShouldBeIndependent()
  {
    // Arrange & Act
    var mail1 = new Mail("Subject 1");
    var mail2 = new Mail("Subject 2");

    mail1.ToList.Add(new MailboxAddress("User 1", "user1@example.com"));
    mail2.ToList.Add(new MailboxAddress("User 2", "user2@example.com"));

    // Assert
    Assert.NotSame(mail1, mail2);
    Assert.Single(mail1.ToList);
    Assert.Single(mail2.ToList);
    Assert.NotEqual(mail1.ToList[0].Address, mail2.ToList[0].Address);
  }

  [Fact]
  public void ToList_ModifyingAfterConstruction_ShouldReflectChanges()
  {
    // Arrange
    var initialList = new List<MailboxAddress>
    {
      new MailboxAddress("User 1", "user1@example.com")
    };
    var mail = new Mail("Test", "Body", "<p>Body</p>", initialList);

    // Act
    mail.ToList.Add(new MailboxAddress("User 2", "user2@example.com"));

    // Assert
    Assert.Equal(2, mail.ToList.Count);
  }

  [Fact]
  public void Mail_WithAllOptionalPropertiesSet_ShouldRetainAllValues()
  {
    // Arrange
    var mail = new Mail("Test")
    {
      Subject = "Full Test",
      TextBody = "Text content",
      HtmlBody = "<p>HTML content</p>",
      UnsubscribeLink = "https://example.com/unsubscribe",
      CcList = new List<MailboxAddress>
      {
        new MailboxAddress("CC", "cc@example.com")
      },
      BccList = new List<MailboxAddress>
      {
        new MailboxAddress("BCC", "bcc@example.com")
      },
      Attachments = new AttachmentCollection()
    };

    mail.ToList.Add(new MailboxAddress("To", "to@example.com"));

    // Assert
    Assert.Equal("Full Test", mail.Subject);
    Assert.Equal("Text content", mail.TextBody);
    Assert.Equal("<p>HTML content</p>", mail.HtmlBody);
    Assert.Equal("https://example.com/unsubscribe", mail.UnsubscribeLink);
    Assert.NotNull(mail.CcList);
    Assert.Single(mail.CcList);
    Assert.NotNull(mail.BccList);
    Assert.Single(mail.BccList);
    Assert.NotNull(mail.Attachments);
    Assert.Single(mail.ToList);
  }

  [Fact]
  public void Subject_CanBeUpdatedMultipleTimes()
  {
    // Arrange
    var mail = new Mail("Initial Subject");

    // Act
    mail.Subject = "Updated Subject 1";
    mail.Subject = "Updated Subject 2";
    mail.Subject = "Final Subject";

    // Assert
    Assert.Equal("Final Subject", mail.Subject);
  }

  [Fact]
  public void Constructor_PreservesListReference()
  {
    // Arrange
    var toList = new List<MailboxAddress>
    {
      new MailboxAddress("User", "user@example.com")
    };

    // Act
    var mail = new Mail("Test", "Body", "<p>Body</p>", toList);

    // Assert
    Assert.Same(toList, mail.ToList);
  }

  [Fact]
  public void TextBody_WithEmptyString_ShouldSetEmpty()
  {
    // Arrange
    var mail = new Mail("Test")
    {
      TextBody = string.Empty
    };

    // Assert
    Assert.Equal(string.Empty, mail.TextBody);
  }

  [Fact]
  public void HtmlBody_WithEmptyString_ShouldSetEmpty()
  {
    // Arrange
    var mail = new Mail("Test")
    {
      HtmlBody = string.Empty
    };

    // Assert
    Assert.Equal(string.Empty, mail.HtmlBody);
  }

  [Fact]
  public void Mail_WithLongContent_ShouldHandleCorrectly()
  {
    // Arrange
    var longSubject = new string('A', 1000);
    var longTextBody = new string('B', 10000);
    var longHtmlBody = $"<p>{new string('C', 10000)}</p>";
    var toList = new List<MailboxAddress>
    {
      new MailboxAddress("User", "user@example.com")
    };

    // Act
    var mail = new Mail(longSubject, longTextBody, longHtmlBody, toList);

    // Assert
    Assert.Equal(1000, mail.Subject.Length);
    Assert.Equal(10000, mail.TextBody.Length);
    Assert.True(mail.HtmlBody.Length > 10000);
  }
}