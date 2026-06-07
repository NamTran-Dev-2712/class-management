using ClassManagement.Infrastructure.Configuration;
using ClassManagement.Infrastructure.Services.Email.Templates;
using Hangfire;
using Microsoft.Extensions.Options;

namespace ClassManagement.Infrastructure.Services.Messaging.Jobs;

// Invoked by Hangfire on a background worker. Dependencies are resolved per-invocation
// from a fresh DI scope, so IEmailService is safe to use here.
public sealed class PasswordResetEmailJob
{
    private readonly IEmailService _email;
    private readonly PasswordResetOptions _options;

    public PasswordResetEmailJob(IEmailService email, IOptions<PasswordResetOptions> options)
    {
        _email = email;
        _options = options.Value;
    }

    [AutomaticRetry(Attempts = 3)]
    public Task ExecuteAsync(string email, string displayName, string otp, string resetLink)
    {
        var message = PasswordResetEmailTemplate.Build(
            email,
            displayName,
            otp,
            resetLink,
            _options.ExpiryMinutes
        );

        return _email.SendAsync(message);
    }
}
