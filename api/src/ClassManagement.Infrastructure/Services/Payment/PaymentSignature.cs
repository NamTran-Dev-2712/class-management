using System.Security.Cryptography;
using System.Text;

namespace ClassManagement.Infrastructure.Services.Payment;

// HMAC helpers for the payment gateways. Momo signs with HMAC-SHA256, VnPay with HMAC-SHA512. Lower-hex
// output matches both providers' documented formats.
internal static class PaymentSignature
{
    public static string HmacSha256(string key, string data) => Hmac<HMACSHA256>(key, data);

    public static string HmacSha512(string key, string data) => Hmac<HMACSHA512>(key, data);

    private static string Hmac<T>(string key, string data)
        where T : HMAC, new()
    {
        using var hmac = new T();
        hmac.Key = Encoding.UTF8.GetBytes(key);
        var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(data));
        var sb = new StringBuilder(hash.Length * 2);
        foreach (var b in hash)
            sb.Append(b.ToString("x2"));
        return sb.ToString();
    }

    public static bool FixedTimeEquals(string a, string b) =>
        CryptographicOperations.FixedTimeEquals(
            Encoding.UTF8.GetBytes(a.ToLowerInvariant()),
            Encoding.UTF8.GetBytes(b.ToLowerInvariant())
        );
}
