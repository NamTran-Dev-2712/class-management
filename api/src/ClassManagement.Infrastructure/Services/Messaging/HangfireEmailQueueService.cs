using ClassManagement.Infrastructure.Configuration;
using ClassManagement.Infrastructure.Services.Messaging.Jobs;
using Hangfire;
using Microsoft.Extensions.Options;

namespace ClassManagement.Infrastructure.Services.Messaging;

// Enqueues email delivery as a durable Hangfire background job so the HTTP request
// returns immediately and delivery survives process restarts (PostgreSQL storage).
public sealed class HangfireEmailQueueService : IEmailQueueService
{
    private readonly IBackgroundJobClient _jobs;
    private readonly ClientAppOptions _clientApp;

    public HangfireEmailQueueService(
        IBackgroundJobClient jobs,
        IOptions<ClientAppOptions> clientApp
    )
    {
        _jobs = jobs;
        _clientApp = clientApp.Value;
    }

    public void EnqueuePasswordResetEmail(
        string email,
        string displayName,
        string otp,
        string resetLink
    ) =>
        _jobs.Enqueue<PasswordResetEmailJob>(j =>
            j.ExecuteAsync(email, displayName, otp, resetLink)
        );

    public void EnqueueWelcomeEmail(string email, string displayName, string temporaryPassword) =>
        _jobs.Enqueue<WelcomeEmailJob>(j =>
            j.ExecuteAsync(email, displayName, temporaryPassword, BuildLoginLink())
        );

    private string BuildLoginLink()
    {
        var baseUrl = _clientApp.BaseUrl.TrimEnd('/');
        var path = _clientApp.LoginPath;
        if (!path.StartsWith('/'))
            path = "/" + path;

        return $"{baseUrl}{path}";
    }
}
