using System.Collections.Concurrent;
using ClassManagement.Application.Interfaces.Messaging;

namespace ClassManagement.IntegrationTests.Infrastructure;

// Captures password-reset emails in-memory instead of enqueuing a Hangfire job,
// so tests can read back the OTP/link and assert delivery without a real provider.
public sealed class FakeEmailQueueService : IEmailQueueService
{
    private readonly ConcurrentDictionary<string, (string Otp, string ResetLink)> _sent = new(
        StringComparer.OrdinalIgnoreCase
    );

    public void EnqueuePasswordResetEmail(
        string email,
        string displayName,
        string otp,
        string resetLink
    ) => _sent[email] = (otp, resetLink);

    public string? GetLastOtp(string email) =>
        _sent.TryGetValue(email, out var entry) ? entry.Otp : null;

    public bool WasSentTo(string email) => _sent.ContainsKey(email);
}
