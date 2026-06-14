using ClassManagement.Infrastructure.Configuration;
using Microsoft.Extensions.Options;
using Resend;
using ResendMessage = Resend.EmailMessage;

namespace ClassManagement.Infrastructure.Services.Email;

// IEmailService implementation backed by the Resend transactional email API.
public sealed class ResendEmailService : IEmailService
{
    private readonly IResend _resend;
    private readonly ResendOptions _options;

    public ResendEmailService(IResend resend, IOptions<ResendOptions> options)
    {
        _resend = resend;
        _options = options.Value;
    }

    public async Task SendAsync(
        Application.Interfaces.Email.EmailMessage message,
        CancellationToken cancellationToken = default
    )
    {
        var from = string.IsNullOrWhiteSpace(_options.FromName)
            ? _options.FromEmail
            : $"{_options.FromName} <{_options.FromEmail}>";

        var payload = new ResendMessage
        {
            From = from,
            Subject = message.Subject,
            HtmlBody = message.HtmlBody,
            TextBody = message.TextBody,
        };
        payload.To.Add(message.To);

        await _resend.EmailSendAsync(payload, cancellationToken);
    }
}
