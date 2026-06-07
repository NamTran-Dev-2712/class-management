using System.Net;

namespace ClassManagement.Infrastructure.Services.Email.Templates;

// Builds the password-reset email body. Kept separate so content/markup can change
// without touching transport (ResendEmailService) or scheduling (Hangfire job) logic.
public static class PasswordResetEmailTemplate
{
    public static EmailMessage Build(
        string toEmail,
        string displayName,
        string otp,
        string resetLink,
        int expiryMinutes
    )
    {
        var safeName = WebUtility.HtmlEncode(
            string.IsNullOrWhiteSpace(displayName) ? "there" : displayName
        );
        var safeLink = WebUtility.HtmlEncode(resetLink);

        const string subject = "Reset your password";

        var htmlBody =
            $@"
<div style=""font-family:Segoe UI,Arial,sans-serif;max-width:480px;margin:0 auto;color:#1f2937"">
  <h2 style=""color:#111827"">Reset your password</h2>
  <p>Hi {safeName},</p>
  <p>We received a request to reset your password. Use the verification code below:</p>
  <div style=""font-size:32px;font-weight:700;letter-spacing:8px;text-align:center;
              background:#f3f4f6;border-radius:8px;padding:16px;margin:16px 0;color:#111827"">
    {otp}
  </div>
  <p style=""text-align:center;margin:24px 0"">
    <a href=""{safeLink}""
       style=""background:#6d28d9;color:#ffffff;text-decoration:none;
              padding:12px 24px;border-radius:6px;display:inline-block"">
      Reset password
    </a>
  </p>
  <p style=""color:#6b7280;font-size:14px"">
    This code expires in {expiryMinutes} minutes. If you didn't request a password reset,
    you can safely ignore this email.
  </p>
</div>";

        var textBody =
            $"Hi {(string.IsNullOrWhiteSpace(displayName) ? "there" : displayName)},\n\n"
            + $"Use this verification code to reset your password: {otp}\n"
            + $"Or open this link: {resetLink}\n\n"
            + $"This code expires in {expiryMinutes} minutes. If you didn't request this, ignore this email.";

        return new EmailMessage(toEmail, subject, htmlBody.Trim(), textBody);
    }
}
