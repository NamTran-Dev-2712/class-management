using System.Text;
using ClassManagement.Application.Common.Constants;
using ClassManagement.Domain.Modules.Payment.Enums;
using ClassManagement.Infrastructure.Security;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.RateLimiting;

namespace ClassManagement.Api.Controllers;

// Provider → server payment notifications (MVP-8). Anonymous (the signature, verified in the provider
// adapter, is the trust boundary) + idempotent. Returns a generic ack both Momo and VnPay accept.
[ApiController]
[Route("api/payments/webhook")]
[AllowAnonymous]
public class PaymentWebhookController : BaseApiController
{
    private readonly ISender _mediator;

    public PaymentWebhookController(ISender mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("{provider}")]
    [EnableRateLimiting(RateLimitOptions.Policies.PaymentWebhook)]
    public async Task<IActionResult> Handle(string provider, CancellationToken cancellationToken)
    {
        if (!Enum.TryParse<PaymentProvider>(provider, ignoreCase: true, out var providerEnum))
            return ApiBadRequest("Payment.ProviderNotSupported");

        string rawBody;
        using (var reader = new StreamReader(Request.Body, Encoding.UTF8, leaveOpen: true))
            rawBody = await reader.ReadToEndAsync(cancellationToken);

        var query = Request.Query.ToDictionary(kv => kv.Key, kv => kv.Value.ToString());

        await _mediator.Send(
            new ProcessPaymentWebhookCommand(providerEnum, rawBody, query),
            cancellationToken
        );

        // A successful webhook activates the teacher's subscription — refresh subscription/usage reads.
        await EvictCacheAsync(OutputCacheTags.Subscriptions, cancellationToken);

        return Ok(new { RspCode = "00", Message = "Confirm Success" });
    }
}
