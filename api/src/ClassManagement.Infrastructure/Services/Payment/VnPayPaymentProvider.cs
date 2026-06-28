using System.Globalization;
using System.Net;
using ClassManagement.Infrastructure.Configuration;
using Microsoft.Extensions.Options;

namespace ClassManagement.Infrastructure.Services.Payment;

// VnPay adapter. CreatePayment builds a signed redirect URL (no server call). The webhook (IPN) rebuilds
// the hash from the sorted vnp_* params and verifies HMAC-SHA512 before trusting the response code. Real
// adapter — exercised in production once credentials are configured (UseFakeProvider = false).
public sealed class VnPayPaymentProvider : IPaymentProvider
{
    private readonly PaymentOptions _payment;
    private readonly PaymentOptions.VnPayOptions _options;

    public VnPayPaymentProvider(IOptions<PaymentOptions> options)
    {
        _payment = options.Value;
        _options = options.Value.VnPay;
    }

    public PaymentProvider Provider => PaymentProvider.VnPay;

    public Task<CreatePaymentResult> CreatePaymentAsync(
        CreatePaymentRequest request,
        CancellationToken cancellationToken = default
    )
    {
        var now = DateTime.UtcNow.AddHours(7); // VnPay timestamps are GMT+7.
        var fields = new SortedDictionary<string, string>(StringComparer.Ordinal)
        {
            ["vnp_Version"] = "2.1.0",
            ["vnp_Command"] = "pay",
            ["vnp_TmnCode"] = _options.TmnCode,
            ["vnp_Amount"] = (request.AmountVnd * 100).ToString(CultureInfo.InvariantCulture),
            ["vnp_CurrCode"] = "VND",
            ["vnp_TxnRef"] = request.OrderRef,
            ["vnp_OrderInfo"] = request.OrderInfo,
            ["vnp_OrderType"] = "other",
            ["vnp_Locale"] = "vn",
            ["vnp_ReturnUrl"] = $"{_payment.ReturnUrlBase}?paymentId={request.PaymentPublicId}",
            ["vnp_IpAddr"] = string.IsNullOrEmpty(request.ClientIpAddress)
                ? "127.0.0.1"
                : request.ClientIpAddress,
            ["vnp_CreateDate"] = now.ToString("yyyyMMddHHmmss"),
        };

        var hashData = BuildHashData(fields);
        var secureHash = PaymentSignature.HmacSha512(_options.HashSecret, hashData);
        var redirect = $"{_options.Endpoint}?{hashData}&vnp_SecureHash={secureHash}";
        return Task.FromResult(new CreatePaymentResult(request.OrderRef, redirect, null));
    }

    public PaymentWebhookResult? VerifyAndParseWebhook(PaymentWebhookContext context)
    {
        if (
            !context.Query.TryGetValue("vnp_SecureHash", out var providedHash)
            || string.IsNullOrEmpty(providedHash)
        )
            return null;

        var signed = new SortedDictionary<string, string>(StringComparer.Ordinal);
        foreach (var kv in context.Query)
        {
            if (
                kv.Key.StartsWith("vnp_", StringComparison.Ordinal)
                && kv.Key != "vnp_SecureHash"
                && kv.Key != "vnp_SecureHashType"
            )
                signed[kv.Key] = kv.Value;
        }

        var expected = PaymentSignature.HmacSha512(_options.HashSecret, BuildHashData(signed));
        if (!PaymentSignature.FixedTimeEquals(expected, providedHash))
            return null;

        var orderRef = context.Query.GetValueOrDefault("vnp_TxnRef") ?? "";
        var responseCode = context.Query.GetValueOrDefault("vnp_ResponseCode") ?? "";
        var transactionStatus = context.Query.GetValueOrDefault("vnp_TransactionStatus") ?? "";
        var transactionNo = context.Query.GetValueOrDefault("vnp_TransactionNo");
        var outcome =
            responseCode == "00" && transactionStatus == "00"
                ? PaymentWebhookOutcome.Succeeded
                : PaymentWebhookOutcome.Failed;

        return new PaymentWebhookResult(
            orderRef,
            transactionNo,
            outcome,
            new Dictionary<string, object?> { ["vnp_ResponseCode"] = responseCode }
        );
    }

    private static string BuildHashData(SortedDictionary<string, string> fields) =>
        string.Join("&", fields.Select(kv => $"{kv.Key}={WebUtility.UrlEncode(kv.Value)}"));
}
