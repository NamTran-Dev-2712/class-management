using ClassManagement.Infrastructure.Persistence.DbContext;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace ClassManagement.IntegrationTests.Payment;

// MVP-8 Slices 3+4 — checkout + webhook happy path and idempotency. Uses the Fake provider
// (Payment:UseFakeProvider = true in appsettings) so the flow runs without a real gateway.
public class CheckoutWebhookTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    private readonly ApiFactory _factory = factory;

    private async Task<(Guid planId, long price)> GetProMonthlyAsync(HttpClient client)
    {
        var response = await client.GetAsync("/api/plans");
        var data = await AuthHelper.ReadDataAsync(response);
        var pro = data.EnumerateArray()
            .First(p =>
                p.GetProperty("name").GetString() == "Pro"
                && p.GetProperty("billingCycle").GetString() == "Monthly"
            );
        return (pro.GetProperty("publicId").GetGuid(), pro.GetProperty("priceVnd").GetInt64());
    }

    private static string ExtractOrderRef(string redirectUrl)
    {
        const string marker = "orderRef=";
        var start = redirectUrl.IndexOf(marker, StringComparison.Ordinal) + marker.Length;
        var end = redirectUrl.IndexOf('&', start);
        return end < 0 ? redirectUrl[start..] : redirectUrl[start..end];
    }

    [Fact]
    public async Task Checkout_ThenWebhookSuccess_ActivatesSubscriptionAndIssuesInvoice()
    {
        var teacher = await AuthHelper.CreateRoleClientAsync(_factory, "Teacher");
        var (planId, price) = await GetProMonthlyAsync(teacher);

        // 1) Checkout → Pending payment + redirect.
        var checkout = await teacher.PostAsJsonAsync(
            "/api/teacher/subscription/checkout",
            new { planPublicId = planId, provider = "Momo" }
        );
        checkout.StatusCode.Should().Be(HttpStatusCode.OK);
        var checkoutData = await AuthHelper.ReadDataAsync(checkout);
        var paymentId = checkoutData.GetProperty("paymentPublicId").GetGuid();
        checkoutData.GetProperty("amountVnd").GetInt64().Should().Be(price);
        var redirect = checkoutData.GetProperty("redirectUrl").GetString()!;
        var orderRef = ExtractOrderRef(redirect);

        // Status is Pending before the webhook.
        var status = await teacher.GetAsync(
            $"/api/teacher/subscription/payments/{paymentId}/status"
        );
        (await AuthHelper.ReadDataAsync(status))
            .GetProperty("status")
            .GetString()
            .Should()
            .Be("Pending");

        // 2) Provider webhook → success.
        var webhook = await teacher.PostAsJsonAsync(
            "/api/payments/webhook/momo",
            new { orderRef, outcome = "Succeeded" }
        );
        webhook.StatusCode.Should().Be(HttpStatusCode.OK);

        // 3) Payment Completed; teacher now Pro.
        var after = await teacher.GetAsync(
            $"/api/teacher/subscription/payments/{paymentId}/status"
        );
        (await AuthHelper.ReadDataAsync(after))
            .GetProperty("status")
            .GetString()
            .Should()
            .Be("Completed");

        var usage = await teacher.GetAsync("/api/teacher/subscription/usage");
        var usageData = await AuthHelper.ReadDataAsync(usage);
        usageData.GetProperty("planName").GetString().Should().Be("Pro");
        usageData.GetProperty("isPro").GetBoolean().Should().BeTrue();

        // 4) Invoice + subscription exist exactly once.
        var counts = await CountForPaymentAsync(paymentId);
        counts.Subscriptions.Should().Be(1);
        counts.Invoices.Should().Be(1);
    }

    [Fact]
    public async Task Webhook_DeliveredTwice_AppliedOnce()
    {
        var teacher = await AuthHelper.CreateRoleClientAsync(_factory, "Teacher");
        var (planId, _) = await GetProMonthlyAsync(teacher);

        var checkout = await teacher.PostAsJsonAsync(
            "/api/teacher/subscription/checkout",
            new { planPublicId = planId, provider = "VnPay" }
        );
        var checkoutData = await AuthHelper.ReadDataAsync(checkout);
        var paymentId = checkoutData.GetProperty("paymentPublicId").GetGuid();
        var orderRef = ExtractOrderRef(checkoutData.GetProperty("redirectUrl").GetString()!);

        var body = new { orderRef, outcome = "Succeeded" };
        (await teacher.PostAsJsonAsync("/api/payments/webhook/vnpay", body))
            .StatusCode.Should()
            .Be(HttpStatusCode.OK);
        // Duplicate delivery (provider retry) — must be a no-op.
        (await teacher.PostAsJsonAsync("/api/payments/webhook/vnpay", body))
            .StatusCode.Should()
            .Be(HttpStatusCode.OK);

        var counts = await CountForPaymentAsync(paymentId);
        counts.Subscriptions.Should().Be(1);
        counts.Invoices.Should().Be(1);
    }

    [Fact]
    public async Task Webhook_FailedOutcome_MarksPaymentFailedNoSubscription()
    {
        var teacher = await AuthHelper.CreateRoleClientAsync(_factory, "Teacher");
        var (planId, _) = await GetProMonthlyAsync(teacher);

        var checkout = await teacher.PostAsJsonAsync(
            "/api/teacher/subscription/checkout",
            new { planPublicId = planId, provider = "Momo" }
        );
        var checkoutData = await AuthHelper.ReadDataAsync(checkout);
        var paymentId = checkoutData.GetProperty("paymentPublicId").GetGuid();
        var orderRef = ExtractOrderRef(checkoutData.GetProperty("redirectUrl").GetString()!);

        await teacher.PostAsJsonAsync(
            "/api/payments/webhook/momo",
            new { orderRef, outcome = "Failed" }
        );

        var status = await teacher.GetAsync(
            $"/api/teacher/subscription/payments/{paymentId}/status"
        );
        (await AuthHelper.ReadDataAsync(status))
            .GetProperty("status")
            .GetString()
            .Should()
            .Be("Failed");

        var counts = await CountForPaymentAsync(paymentId);
        counts.Subscriptions.Should().Be(0);
        counts.Invoices.Should().Be(0);
    }

    private async Task<(int Subscriptions, int Invoices)> CountForPaymentAsync(Guid paymentId) =>
        await _factory.WithScopeAsync(async sp =>
        {
            var db = sp.GetRequiredService<ApplicationDbContext>();
            var payment = await db.Payments.FirstAsync(p => p.PublicId == paymentId);
            var subs = await db.Subscriptions.CountAsync(s => s.TeacherId == payment.TeacherId);
            var invoices = await db.Invoices.CountAsync(i => i.TeacherId == payment.TeacherId);
            return (subs, invoices);
        });
}
