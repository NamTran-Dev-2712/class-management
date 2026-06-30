using System.Text;
using System.Text.Json;
using ClassManagement.Infrastructure.Configuration;
using Microsoft.Extensions.Options;

namespace ClassManagement.Infrastructure.Services.Payment;

// Momo Business API v2 adapter. CreatePayment calls Momo server-to-server for a payUrl/QR; the webhook
// verifies the HMAC-SHA256 signature over the documented field set before trusting resultCode. Real
// adapter — exercised in production once credentials are configured (UseFakeProvider = false).
public sealed class MomoPaymentProvider : IPaymentProvider
{
    private readonly HttpClient _http;
    private readonly PaymentOptions _payment;
    private readonly PaymentOptions.MomoOptions _options;

    public MomoPaymentProvider(HttpClient http, IOptions<PaymentOptions> options)
    {
        _http = http;
        _payment = options.Value;
        _options = options.Value.Momo;
    }

    public PaymentProvider Provider => PaymentProvider.Momo;

    public async Task<CreatePaymentResult> CreatePaymentAsync(
        CreatePaymentRequest request,
        CancellationToken cancellationToken = default
    )
    {
        var requestId = Guid.NewGuid().ToString("N");
        var orderId = request.OrderRef;
        const string requestType = "captureWallet";
        var extraData = string.Empty;
        var redirectUrl = string.IsNullOrEmpty(_options.RedirectUrl)
            ? $"{_payment.ReturnUrlBase}?paymentId={request.PaymentPublicId}"
            : _options.RedirectUrl;
        var ipnUrl = _options.IpnUrl;

        // Raw signature string — field order is fixed by Momo's spec.
        var raw =
            $"accessKey={_options.AccessKey}&amount={request.AmountVnd}&extraData={extraData}"
            + $"&ipnUrl={ipnUrl}&orderId={orderId}&orderInfo={request.OrderInfo}"
            + $"&partnerCode={_options.PartnerCode}&redirectUrl={redirectUrl}"
            + $"&requestId={requestId}&requestType={requestType}";
        var signature = PaymentSignature.HmacSha256(_options.SecretKey, raw);

        var payload = new Dictionary<string, object?>
        {
            ["partnerCode"] = _options.PartnerCode,
            ["accessKey"] = _options.AccessKey,
            ["requestId"] = requestId,
            ["amount"] = request.AmountVnd.ToString(),
            ["orderId"] = orderId,
            ["orderInfo"] = request.OrderInfo,
            ["redirectUrl"] = redirectUrl,
            ["ipnUrl"] = ipnUrl,
            ["extraData"] = extraData,
            ["requestType"] = requestType,
            ["signature"] = signature,
            ["lang"] = "vi",
        };

        using var content = new StringContent(
            JsonSerializer.Serialize(payload),
            Encoding.UTF8,
            "application/json"
        );
        using var response = await _http.PostAsync(_options.Endpoint, content, cancellationToken);
        var body = await response.Content.ReadAsStringAsync(cancellationToken);

        using var doc = JsonDocument.Parse(body);
        var root = doc.RootElement;
        var payUrl = root.TryGetProperty("payUrl", out var p) ? p.GetString() : null;
        var qrUrl = root.TryGetProperty("qrCodeUrl", out var q) ? q.GetString() : null;
        return new CreatePaymentResult(orderId, payUrl, qrUrl);
    }

    public PaymentWebhookResult? VerifyAndParseWebhook(PaymentWebhookContext context)
    {
        using var doc = JsonDocument.Parse(context.RawBody);
        var root = doc.RootElement;
        string Get(string k) =>
            root.TryGetProperty(k, out var v)
                ? (v.ValueKind == JsonValueKind.String ? v.GetString() ?? "" : v.ToString())
                : "";

        var providedSignature = Get("signature");
        if (string.IsNullOrEmpty(providedSignature))
            return null;

        var raw =
            $"accessKey={_options.AccessKey}&amount={Get("amount")}&extraData={Get("extraData")}"
            + $"&message={Get("message")}&orderId={Get("orderId")}&orderInfo={Get("orderInfo")}"
            + $"&orderType={Get("orderType")}&partnerCode={Get("partnerCode")}&payType={Get("payType")}"
            + $"&requestId={Get("requestId")}&responseTime={Get("responseTime")}"
            + $"&resultCode={Get("resultCode")}&transId={Get("transId")}";
        var expected = PaymentSignature.HmacSha256(_options.SecretKey, raw);
        if (!PaymentSignature.FixedTimeEquals(expected, providedSignature))
            return null;

        var resultCode = Get("resultCode");
        var outcome =
            resultCode == "0" ? PaymentWebhookOutcome.Succeeded : PaymentWebhookOutcome.Failed;
        return new PaymentWebhookResult(
            Get("orderId"),
            Get("transId"),
            outcome,
            new Dictionary<string, object?>
            {
                ["resultCode"] = resultCode,
                ["message"] = Get("message"),
            }
        );
    }
}
