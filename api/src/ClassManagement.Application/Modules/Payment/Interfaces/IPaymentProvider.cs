// Payment-gateway abstraction (MVP-8). Keeps Application transport-agnostic; concrete adapters (Momo,
// VnPay, Fake) live in Infrastructure. The provider never touches the DB — it only builds a redirect/QR
// for an order and verifies+parses an incoming webhook callback.
public interface IPaymentProvider
{
    PaymentProvider Provider { get; }

    // Build a hosted-payment redirect / QR for an order. OrderRef is our opaque, alphanumeric order
    // reference (echoed back by the gateway in its callback).
    Task<CreatePaymentResult> CreatePaymentAsync(
        CreatePaymentRequest request,
        CancellationToken cancellationToken = default
    );

    // Verify the callback signature and parse it. Returns null when the signature is invalid (reject).
    PaymentWebhookResult? VerifyAndParseWebhook(PaymentWebhookContext context);
}

// Resolves the adapter for a provider; returns the Fake simulator when Payment:UseFakeProvider is on.
public interface IPaymentProviderResolver
{
    IPaymentProvider Resolve(PaymentProvider provider);
}

public sealed record CreatePaymentRequest(
    string OrderRef,
    long AmountVnd,
    string OrderInfo,
    Guid PaymentPublicId,
    string? ClientIpAddress
);

public sealed record CreatePaymentResult(
    string ProviderOrderId,
    string? RedirectUrl,
    string? QrCodeUrl
);

// Raw inbound webhook payload — query string (VnPay) and/or JSON body (Momo).
public sealed record PaymentWebhookContext(
    string RawBody,
    IReadOnlyDictionary<string, string> Query
);

public enum PaymentWebhookOutcome
{
    Succeeded,
    Failed,
}

public sealed record PaymentWebhookResult(
    string OrderRef,
    string? ProviderTransactionId,
    PaymentWebhookOutcome Outcome,
    Dictionary<string, object?>? Metadata
);
