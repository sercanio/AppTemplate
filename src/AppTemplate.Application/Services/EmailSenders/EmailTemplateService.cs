using Microsoft.Extensions.Configuration;

namespace AppTemplate.Application.Services.EmailSenders;

public class EmailTemplateService
{
  public EmailTemplateService(IConfiguration configuration)
  {
  }

  public static string GetEmailConfirmationTemplate(string callbackUrl, string username)
  {
    return @$"<!DOCTYPE html>
<html lang=""en"">
<head>
  <meta http-equiv=""Content-Type"" content=""text/html; charset=UTF-8"" />
  <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"" />
  <title>Confirm Your Email - AppTemplate</title>
</head>
<body style=""Margin:0;padding:0;background-color:#f4f4f4;font-family:Arial,Helvetica,sans-serif;"">
  <table width=""100%"" cellpadding=""0"" cellspacing=""0"" border=""0"" style=""background-color:#f4f4f4;"">
    <tr>
      <td align=""center"" style=""padding:20px 0;"">
        <!--[if (gte mso 9)|(IE)]>
        <table width=""600"" align=""center"" cellpadding=""0"" cellspacing=""0"" border=""0"">
        <tr>
        <td>
        <![endif]-->
        <table width=""100%"" max-width=""600"" cellpadding=""0"" cellspacing=""0"" border=""0"" style=""background-color:#ffffff;border:1px solid #dddddd;border-radius:4px;overflow:hidden;"">
          <!-- Header -->
          <tr>
            <td align=""center"" bgcolor=""#8ac926"" style=""padding: 30px 20px; color:#ffffff; font-size:24px; font-weight:bold;"">
              AppTemplate
            </td>
          </tr>
          <!-- Body -->
          <tr>
            <td style=""padding: 40px 30px; color:#333333; font-size:16px; line-height:1.5;"">
              <p style=""Margin:0 0 20px 0;"">
                Hello{(string.IsNullOrEmpty(username) ? "" : $" {username}")},
              </p>
              <p style=""Margin:0 0 20px 0;"">
                Thank you for registering with AppTemplate. To complete your registration and activate your account, please confirm your email address by clicking the button below:
              </p>
              <!-- Button : begin -->
              <table cellpadding=""0"" cellspacing=""0"" border=""0"" width=""100%"" style=""Margin:30px 0;"">
                <tr>
                  <td align=""center"">
                    <!--[if mso]>
                    <v:roundrect xmlns:v=""urn:schemas-microsoft-com:vml"" xmlns:w=""urn:schemas-microsoft-com:office:word"" href=""{callbackUrl}"" style=""height:48px;v-text-anchor:middle;width:260px;"" arcsize=""8%"" stroke=""f"" fillcolor=""#8ac926"">
                      <w:anchorlock/>
                      <center style=""color:#ffffff;font-family:Arial,Helvetica,sans-serif;font-size:16px;font-weight:bold;"">
                        Confirm Email Address
                      </center>
                    </v:roundrect>
                    <![endif]-->
                    <![if !mso]>
                    <a href=""{callbackUrl}"" target=""_blank""
                       style=""background-color:#8ac926;color:#ffffff;text-decoration:none;padding:14px 26px;font-size:16px;font-weight:bold;border-radius:4px;display:inline-block;font-family:Arial,Helvetica,sans-serif;"">
                      Confirm Email Address
                    </a>
                    <![endif]>
                  </td>
                </tr>
              </table>
              <!-- Button : end -->
              <p style=""Margin:0 0 20px 0;font-size:14px;color:#555555;line-height:1.4;"">
                If you're having trouble with the button above, copy and paste the URL below into your web browser:
              </p>
              <p style=""Margin:0 0 20px 0;font-size:14px;color:#555555;line-height:1.4;word-break:break-all;"">
                <a href=""{callbackUrl}"" target=""_blank"" style=""color:#8ac926;text-decoration:none;word-break:break-all;font-family:Courier New,monospace;"">
                  {callbackUrl}
                </a>
              </p>
              <p style=""Margin:0;font-size:14px;color:#555555;line-height:1.4;"">
                If you did not create an account, you can safely ignore this email.
              </p>
            </td>
          </tr>
          <!-- Footer -->
          <tr>
            <td align=""center"" bgcolor=""#f0f0f0"" style=""padding:20px 30px;font-size:12px;color:#999999;line-height:1.4;"">
              © 2025 AppTemplate. All rights reserved.<br />
              This email was sent to confirm your email address. If you didn't register for AppTemplate, please ignore this email.
            </td>
          </tr>
        </table>
        <!--[if (gte mso 9)|(IE)]>
        </td>
        </tr>
        </table>
        <![endif]-->
      </td>
    </tr>
  </table>
</body>
</html>";
  }

  public static string GetEmailChangeConfirmationTemplate(string callbackUrl, string username, string newEmail)
  {
    return @$"<!DOCTYPE html>
<html lang=""en"">
<head>
  <meta http-equiv=""Content-Type"" content=""text/html; charset=UTF-8"" />
  <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"" />
  <title>Confirm Email Change - AppTemplate</title>
</head>
<body style=""Margin:0;padding:0;background-color:#f4f4f4;font-family:Arial,Helvetica,sans-serif;"">
  <table width=""100%"" cellpadding=""0"" cellspacing=""0"" border=""0"" style=""background-color:#f4f4f4;"">
    <tr>
      <td align=""center"" style=""padding:20px 0;"">
        <!--[if (gte mso 9)|(IE)]>
        <table width=""600"" align=""center"" cellpadding=""0"" cellspacing=""0"" border=""0"">
        <tr>
        <td>
        <![endif]-->
        <table width=""100%"" max-width=""600"" cellpadding=""0"" cellspacing=""0"" border=""0"" style=""background-color:#ffffff;border:1px solid #dddddd;border-radius:4px;overflow:hidden;"">
          <!-- Header -->
          <tr>
            <td align=""center"" bgcolor=""#8ac926"" style=""padding: 30px 20px; color:#ffffff; font-size:24px; font-weight:bold;"">
              AppTemplate
            </td>
          </tr>
          <!-- Body -->
          <tr>
            <td style=""padding: 40px 30px; color:#333333; font-size:16px; line-height:1.5;"">
              <p style=""Margin:0 0 20px 0;"">
                Hello{(string.IsNullOrEmpty(username) ? "" : $" {username}")},
              </p>
              <p style=""Margin:0 0 20px 0;"">
                We received a request to change your email address to <strong style=""color:#000000;"">{newEmail}</strong>. To confirm this change, please click the button below:
              </p>
              <!-- Button : begin -->
              <table cellpadding=""0"" cellspacing=""0"" border=""0"" width=""100%"" style=""Margin:30px 0;"">
                <tr>
                  <td align=""center"">
                    <!--[if mso]>
                    <v:roundrect xmlns:v=""urn:schemas-microsoft-com:vml"" xmlns:w=""urn:schemas-microsoft-com:office:word"" href=""{callbackUrl}"" style=""height:48px;v-text-anchor:middle;width:260px;"" arcsize=""8%"" stroke=""f"" fillcolor=""#8ac926"">
                      <w:anchorlock/>
                      <center style=""color:#ffffff;font-family:Arial,Helvetica,sans-serif;font-size:16px;font-weight:bold;"">
                        Confirm Email Change
                      </center>
                    </v:roundrect>
                    <![endif]-->
                    <![if !mso]>
                    <a href=""{callbackUrl}"" target=""_blank""
                       style=""background-color:#8ac926;color:#ffffff;text-decoration:none;padding:14px 26px;font-size:16px;font-weight:bold;border-radius:4px;display:inline-block;font-family:Arial,Helvetica,sans-serif;"">
                      Confirm Email Change
                    </a>
                    <![endif]>
                  </td>
                </tr>
              </table>
              <!-- Button : end -->
              <p style=""Margin:0 0 20px 0;font-size:14px;color:#555555;line-height:1.4;"">
                If you're having trouble with the button above, copy and paste the URL below into your web browser:
              </p>
              <p style=""Margin:0 0 20px 0;font-size:14px;color:#555555;line-height:1.4;word-break:break-all;"">
                <a href=""{callbackUrl}"" target=""_blank"" style=""color:#8ac926;text-decoration:none;word-break:break-all;font-family:Courier New,monospace;"">
                  {callbackUrl}
                </a>
              </p>
              <p style=""Margin:0;font-size:14px;color:#555555;line-height:1.4;"">
                If you did not request this change, you can safely ignore this email and contact support immediately.
              </p>
            </td>
          </tr>
          <!-- Footer -->
          <tr>
            <td align=""center"" bgcolor=""#f0f0f0"" style=""padding:20px 30px;font-size:12px;color:#999999;line-height:1.4;"">
              © 2025 AppTemplate. All rights reserved.<br />
              This email was sent to confirm your email address change. If you didn't request this change, please contact support.
            </td>
          </tr>
        </table>
        <!--[if (gte mso 9)|(IE)]>
        </td>
        </tr>
        </table>
        <![endif]-->
      </td>
    </tr>
  </table>
</body>
</html>";
  }

  public static string GetPasswordResetTemplate(string callbackUrl, string code, string username = "")
  {
    return @$"<!DOCTYPE html>
<html lang=""en"">
<head>
  <meta http-equiv=""Content-Type"" content=""text/html; charset=UTF-8"" />
  <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"" />
  <title>Reset Your Password - AppTemplate</title>
</head>
<body style=""Margin:0;padding:0;background-color:#f4f4f4;font-family:Arial,Helvetica,sans-serif;"">
  <table width=""100%"" cellpadding=""0"" cellspacing=""0"" border=""0"" style=""background-color:#f4f4f4;"">
    <tr>
      <td align=""center"" style=""padding:20px 0;"">
        <!--[if (gte mso 9)|(IE)]>
        <table width=""600"" align=""center"" cellpadding=""0"" cellspacing=""0"" border=""0"">
        <tr>
        <td>
        <![endif]-->
        <table width=""100%"" max-width=""600"" cellpadding=""0"" cellspacing=""0"" border=""0"" style=""background-color:#ffffff;border:1px solid #dddddd;border-radius:4px;overflow:hidden;"">
          <!-- Header -->
          <tr>
            <td align=""center"" bgcolor=""#8ac926"" style=""padding: 30px 20px; color:#ffffff; font-size:24px; font-weight:bold;"">
              AppTemplate
            </td>
          </tr>
          <!-- Body -->
          <tr>
            <td style=""padding: 40px 30px; color:#333333; font-size:16px; line-height:1.5;"">
              <p style=""Margin:0 0 20px 0;"">
                Hello{(string.IsNullOrEmpty(username) ? "" : $" {username}")},
              </p>
              <p style=""Margin:0 0 20px 0;"">
                We received a request to reset your password for your AppTemplate account. To create a new password, please click the button below:
              </p>
              <!-- Button : begin -->
              <table cellpadding=""0"" cellspacing=""0"" border=""0"" width=""100%"" style=""Margin:30px 0;"">
                <tr>
                  <td align=""center"">
                    <!--[if mso]>
                    <v:roundrect xmlns:v=""urn:schemas-microsoft-com:vml"" xmlns:w=""urn:schemas-microsoft-com:office:word"" href=""{callbackUrl}"" style=""height:48px;v-text-anchor:middle;width:260px;"" arcsize=""8%"" stroke=""f"" fillcolor=""#8ac926"">
                      <w:anchorlock/>
                      <center style=""color:#ffffff;font-family:Arial,Helvetica,sans-serif;font-size:16px;font-weight:bold;"">
                        Reset Password
                      </center>
                    </v:roundrect>
                    <![endif]-->
                    <![if !mso]>
                    <a href=""{callbackUrl}"" target=""_blank""
                       style=""background-color:#8ac926;color:#ffffff;text-decoration:none;padding:14px 26px;font-size:16px;font-weight:bold;border-radius:4px;display:inline-block;font-family:Arial,Helvetica,sans-serif;"">
                      Reset Password
                    </a>
                    <![endif]>
                  </td>
                </tr>
              </table>
              <!-- Button : end -->
              <p style=""Margin:0 0 20px 0;font-size:14px;color:#555555;line-height:1.4;"">
                If you're having trouble with the button above, copy and paste the URL below into your web browser:
              </p>
              <p style=""Margin:0 0 20px 0;font-size:14px;color:#555555;line-height:1.4;word-break:break-all;"">
                <a href=""{callbackUrl}"" target=""_blank"" style=""color:#8ac926;text-decoration:none;word-break:break-all;font-family:Courier New,monospace;"">
                  {callbackUrl}
                </a>
              </p>
              <p style=""Margin:0;font-size:14px;color:#555555;line-height:1.4;"">
                If you did not request a password reset, you can ignore this email – your password will not be changed.
              </p>
              <!-- Optional: Show the code if needed -->
              <p style=""Margin:30px 0 0 0;font-size:14px;color:#555555;line-height:1.4;"">
                Your password reset code is:<br />
                <strong style=""font-family:Courier New,monospace;font-size:16px;color:#333333;"">{code}</strong>
              </p>
            </td>
          </tr>
          <!-- Footer -->
          <tr>
            <td align=""center"" bgcolor=""#f0f0f0"" style=""padding:20px 30px;font-size:12px;color:#999999;line-height:1.4;"">
              © 2025 AppTemplate. All rights reserved.<br />
              This email was sent to help you reset your password. If you didn't request a password reset, please ignore this email.
            </td>
          </tr>
        </table>
        <!--[if (gte mso 9)|(IE)]>
        </td>
        </tr>
        </table>
        <![endif]-->
      </td>
    </tr>
  </table>
</body>
</html>";
  }
}
