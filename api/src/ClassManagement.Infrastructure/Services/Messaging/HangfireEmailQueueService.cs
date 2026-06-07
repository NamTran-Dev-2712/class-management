using ClassManagement.Infrastructure.Services.Messaging.Jobs;
using Hangfire;

namespace ClassManagement.Infrastructure.Services.Messaging;

// Enqueues email delivery as a durable Hangfire background job so the HTTP request
// returns immediately and delivery survives process restarts (PostgreSQL storage).
public sealed class HangfireEmailQueueService : IEmailQueueService
{
    private readonly IBackgroundJobClient _jobs;

    public HangfireEmailQueueService(IBackgroundJobClient jobs)
    {
        _jobs = jobs;
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
}
