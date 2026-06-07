namespace ClassManagement.Application.Interfaces.Messaging;

// Queues outbound emails for background delivery. The implementation enqueues a
// background job (Hangfire) so the request thread never blocks on the email provider.
public interface IEmailQueueService
{
    void EnqueuePasswordResetEmail(string email, string displayName, string otp, string resetLink);
}
