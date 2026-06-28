using System.Text.Json;
using ClassManagement.Infrastructure.Configuration;
using Microsoft.Extensions.Options;

namespace ClassManagement.Infrastructure.Services.Payment;

// In-process payment simulator for dev/test (Payment:UseFakeProvider = true). CreatePayment returns a
// deterministic local "gateway" URL that carries the order ref; the webhook parser trusts the inbound
// body/query and reads orderRef + outcome, so tests drive success/failure without a real gateway call.
public sealed class FakePaymentProvider : IPaymentProvider
{
    private readonly PaymentOptions _options;

    public FakePaymentProvider(IOptions<PaymentOptions> options)
    {
        _options = options.Value;
    }

    // The resolver returns this instance directly when UseFakeProvider is on, so this value is only a
    // label — it never participates in provider selection.
    public PaymentProvider Provider => PaymentProvider.Momo;

    public Task<CreatePaymentResult> CreatePaymentAsync(
        CreatePaymentRequest request,
        CancellationToken cancellationToken = default
    )
    {
        var returnUrl = $"{_options.ReturnUrlBase}?paymentId={request.PaymentPublicId}";
        var redirect = $"{returnUrl}&orderRef={request.OrderRef}&simulated=1";
        return Task.FromResult(
            new CreatePaymentResult(
                ProviderOrderId: request.OrderRef,
                RedirectUrl: redirect,
                QrCodeUrl: null
            )
        );
    }

    public PaymentWebhookResult? VerifyAndParseWebhook(PaymentWebhookContext context)
    {
        var orderRef = Read(context, "orderRef");
        if (string.IsNullOrEmpty(orderRef))
            return null;

        var outcome = string.Equals(
            Read(context, "outcome"),
            "Failed",
            StringComparison.OrdinalIgnoreCase
        )
            ? PaymentWebhookOutcome.Failed
            : PaymentWebhookOutcome.Succeeded;

        var transactionId = Read(context, "transactionId") ?? $"fake_{orderRef}";

        return new PaymentWebhookResult(
            orderRef,
            transactionId,
            outcome,
            new Dictionary<string, object?> { ["simulated"] = true }
        );
    }

    private static string? Read(PaymentWebhookContext context, string key)
    {
        if (context.Query.TryGetValue(key, out var value) && !string.IsNullOrEmpty(value))
            return value;

        if (!string.IsNullOrWhiteSpace(context.RawBody))
        {
            try
            {
                using var doc = JsonDocument.Parse(context.RawBody);
                if (doc.RootElement.TryGetProperty(key, out var prop))
                    return prop.ValueKind == JsonValueKind.String
                        ? prop.GetString()
                        : prop.ToString();
            }
            catch (JsonException)
            {
                // Body wasn't JSON — fall through.
            }
        }
        return null;
    }
}
