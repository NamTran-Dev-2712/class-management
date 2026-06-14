using System.Net;

namespace ClassManagement.Infrastructure.Services.Email.Templates;

// Builds the welcome email an admin-created user receives: their temporary password + a sign-in
// link. Kept separate so content/markup can change without touching transport or scheduling.
public static class WelcomeEmailTemplate
{
    public static EmailMessage Build(
        string toEmail,
        string displayName,
        string temporaryPassword,
        string loginLink
    )
    {
        var safeName = WebUtility.HtmlEncode(
            string.IsNullOrWhiteSpace(displayName) ? "there" : displayName
        );
        var safePassword = WebUtility.HtmlEncode(temporaryPassword);
        var safeLink = WebUtility.HtmlEncode(loginLink);

        const string subject = "Your Class Management account is ready";

        var htmlBody =
            $@"
<div style=""font-family:Segoe UI,Arial,sans-serif;max-width:480px;margin:0 auto;color:#1f2937"">
  <h2 style=""color:#111827"">Welcome to Class Management</h2>
  <p>Hi {safeName},</p>
  <p>An administrator created an account for you. Use the temporary password below to sign in,
     then change it from your profile.</p>
  <div style=""font-size:20px;font-weight:700;letter-spacing:2px;text-align:center;
              background:#f3f4f6;border-radius:8px;padding:16px;margin:16px 0;color:#111827"">
    {safePassword}
  </div>
  <p style=""text-align:center;margin:24px 0"">
    <a href=""{safeLink}""
       style=""background:#6d28d9;color:#ffffff;text-decoration:none;
              padding:12px 24px;border-radius:6px;display:inline-block"">
      Sign in
    </a>
  </p>
  <p style=""color:#6b7280;font-size:14px"">
    For your security, please change this password right after your first sign-in. If you did not
    expect this email, you can ignore it.
  </p>
</div>";

        var textBody =
            $"Hi {(string.IsNullOrWhiteSpace(displayName) ? "there" : displayName)},\n\n"
            + "An administrator created an account for you.\n"
            + $"Temporary password: {temporaryPassword}\n"
            + $"Sign in here: {loginLink}\n\n"
            + "Please change this password right after your first sign-in.";

        return new EmailMessage(toEmail, subject, htmlBody.Trim(), textBody);
    }
}
