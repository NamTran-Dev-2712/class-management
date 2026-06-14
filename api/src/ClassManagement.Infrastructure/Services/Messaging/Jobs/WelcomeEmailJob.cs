using ClassManagement.Infrastructure.Services.Email.Templates;
using Hangfire;

namespace ClassManagement.Infrastructure.Services.Messaging.Jobs;

// Invoked by Hangfire on a background worker. Delivers the welcome email containing the
// admin-generated temporary password and a sign-in link.
public sealed class WelcomeEmailJob
{
    private readonly IEmailService _email;

    public WelcomeEmailJob(IEmailService email)
    {
        _email = email;
    }

    [AutomaticRetry(Attempts = 3)]
    public Task ExecuteAsync(
        string email,
        string displayName,
        string temporaryPassword,
        string loginLink
    )
    {
        var message = WelcomeEmailTemplate.Build(email, displayName, temporaryPassword, loginLink);
        return _email.SendAsync(message);
    }
}
