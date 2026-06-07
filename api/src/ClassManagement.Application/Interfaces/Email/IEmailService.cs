namespace ClassManagement.Application.Interfaces.Email;

// Transport abstraction — sends a single email through the configured provider (Resend).
public interface IEmailService
{
    Task SendAsync(EmailMessage message, CancellationToken cancellationToken = default);
}
