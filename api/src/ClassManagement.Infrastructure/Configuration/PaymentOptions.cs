namespace ClassManagement.Infrastructure.Configuration;

// Payment-gateway configuration (MVP-8). Secrets come from config/env — never hard-coded, never in the
// DB. When UseFakeProvider is true (dev/test) the FakePaymentProvider stands in for Momo/VnPay so the
// whole flow is exercisable offline; production sets it false and fills the provider credentials.
public sealed class PaymentOptions
{
    public const string SectionName = "Payment";

    /// <summary>Use the in-process simulator instead of real gateways (dev/test). Production: false.</summary>
    public bool UseFakeProvider { get; init; } = true;

    /// <summary>Fallback order timeout (minutes); the live value is system_settings:payment_order_timeout_minutes.</summary>
    public int OrderTimeoutMinutes { get; init; } = 30;

    /// <summary>Frontend URL the gateway redirects the payer back to (a payment public id is appended).</summary>
    public string ReturnUrlBase { get; init; } =
        "http://localhost:5173/teacher/subscription/return";

    public MomoOptions Momo { get; init; } = new();
    public VnPayOptions VnPay { get; init; } = new();

    public sealed class MomoOptions
    {
        public string Endpoint { get; init; } = string.Empty;
        public string PartnerCode { get; init; } = string.Empty;
        public string AccessKey { get; init; } = string.Empty;
        public string SecretKey { get; init; } = string.Empty;

        /// <summary>Server-to-server webhook (IPN) URL registered with Momo.</summary>
        public string IpnUrl { get; init; } = string.Empty;

        /// <summary>Where Momo redirects the payer after payment.</summary>
        public string RedirectUrl { get; init; } = string.Empty;
    }

    public sealed class VnPayOptions
    {
        public string Endpoint { get; init; } = string.Empty;
        public string TmnCode { get; init; } = string.Empty;
        public string HashSecret { get; init; } = string.Empty;
        public string IpnUrl { get; init; } = string.Empty;
    }
}
