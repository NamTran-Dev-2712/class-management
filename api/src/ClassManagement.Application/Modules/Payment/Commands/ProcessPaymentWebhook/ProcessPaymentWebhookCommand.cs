// A provider → server payment notification (MVP-8). Carries the raw body + query so the provider adapter
// can verify the signature; the handler then applies it exactly once (idempotency key).
public record ProcessPaymentWebhookCommand(
    PaymentProvider Provider,
    string RawBody,
    IReadOnlyDictionary<string, string> Query
) : IRequest;
