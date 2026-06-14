namespace ClassManagement.Application.Interfaces.Messaging;

// Queues outbound emails for background delivery. The implementation enqueues a
// background job (Hangfire) so the request thread never blocks on the email provider.
public interface IEmailQueueService
{
    void EnqueuePasswordResetEmail(string email, string displayName, string otp, string resetLink);

    // Sent when an admin creates an account — delivers the generated temporary password and a
    // sign-in link (built by the implementation from configuration). The password is never shown
    // in the API response.
    void EnqueueWelcomeEmail(string email, string displayName, string temporaryPassword);
}
