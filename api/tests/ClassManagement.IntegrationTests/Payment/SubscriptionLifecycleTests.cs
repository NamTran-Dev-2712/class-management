using ClassManagement.Domain.Modules.Payment.Entities;
using ClassManagement.Infrastructure.Persistence.DbContext;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace ClassManagement.IntegrationTests.Payment;

// MVP-8 Slice 5 — subscription lifecycle sweep. Buys Pro via the Fake provider, then forces expiry/grace
// timestamps into the past and runs RunSubscriptionLifecycleCommand, asserting the state machine.
public class SubscriptionLifecycleTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    private readonly ApiFactory _factory = factory;

    private async Task<long> BuyProAsync(HttpClient teacher)
    {
        var plansResponse = await teacher.GetAsync("/api/plans");
        var plans = await AuthHelper.ReadDataAsync(plansResponse);
        var planId = plans
            .EnumerateArray()
            .First(p =>
                p.GetProperty("name").GetString() == "Pro"
                && p.GetProperty("billingCycle").GetString() == "Monthly"
            )
            .GetProperty("publicId")
            .GetGuid();

        var checkout = await teacher.PostAsJsonAsync(
            "/api/teacher/subscription/checkout",
            new { planPublicId = planId, provider = "Momo" }
        );
        var checkoutData = await AuthHelper.ReadDataAsync(checkout);
        var paymentId = checkoutData.GetProperty("paymentPublicId").GetGuid();
        var redirect = checkoutData.GetProperty("redirectUrl").GetString()!;
        const string marker = "orderRef=";
        var start = redirect.IndexOf(marker, StringComparison.Ordinal) + marker.Length;
        var end = redirect.IndexOf('&', start);
        var orderRef = end < 0 ? redirect[start..] : redirect[start..end];

        await teacher.PostAsJsonAsync(
            "/api/payments/webhook/momo",
            new { orderRef, outcome = "Succeeded" }
        );

        return await _factory.WithScopeAsync(async sp =>
        {
            var db = sp.GetRequiredService<ApplicationDbContext>();
            var payment = await db.Payments.FirstAsync(p => p.PublicId == paymentId);
            return payment.TeacherId;
        });
    }

    private Task RunLifecycleAsync() =>
        _factory.WithScopeAsync(async sp =>
        {
            await sp.GetRequiredService<ISender>().Send(new RunSubscriptionLifecycleCommand());
            return 0;
        });

    private Task<string> StatusOfAsync(long teacherId) =>
        _factory.WithScopeAsync(async sp =>
        {
            var db = sp.GetRequiredService<ApplicationDbContext>();
            var sub = await db
                .Subscriptions.OrderByDescending(s => s.Id)
                .FirstAsync(s => s.TeacherId == teacherId);
            return sub.Status.ToString();
        });

    private Task MutateSubscriptionAsync(long teacherId, Action<Subscription> mutate) =>
        _factory.WithScopeAsync(async sp =>
        {
            var db = sp.GetRequiredService<ApplicationDbContext>();
            var sub = await db
                .Subscriptions.OrderByDescending(s => s.Id)
                .FirstAsync(s => s.TeacherId == teacherId);
            mutate(sub);
            await db.SaveChangesAsync();
            return 0;
        });

    [Fact]
    public async Task Lifecycle_ExpiredNotCancelled_GoesPastDueThenExpired()
    {
        var teacher = await AuthHelper.CreateRoleClientAsync(_factory, "Teacher");
        var teacherId = await BuyProAsync(teacher);
        (await StatusOfAsync(teacherId)).Should().Be("Active");

        // Past expiry, not cancelled → PastDue + grace window.
        await MutateSubscriptionAsync(teacherId, s => s.ExpiresAt = DateTime.UtcNow.AddDays(-10));
        await RunLifecycleAsync();
        (await StatusOfAsync(teacherId)).Should().Be("PastDue");

        // Grace ended → Expired.
        await MutateSubscriptionAsync(
            teacherId,
            s => s.GracePeriodEndsAt = DateTime.UtcNow.AddDays(-1)
        );
        await RunLifecycleAsync();
        (await StatusOfAsync(teacherId)).Should().Be("Expired");
    }

    [Fact]
    public async Task Lifecycle_CancelledThenExpiry_GoesCancelled()
    {
        var teacher = await AuthHelper.CreateRoleClientAsync(_factory, "Teacher");
        var teacherId = await BuyProAsync(teacher);

        var cancel = await teacher.PostAsync("/api/teacher/subscription/cancel", null);
        cancel.StatusCode.Should().Be(HttpStatusCode.OK);
        (await StatusOfAsync(teacherId)).Should().Be("Active"); // still active until expiry

        await MutateSubscriptionAsync(teacherId, s => s.ExpiresAt = DateTime.UtcNow.AddDays(-1));
        await RunLifecycleAsync();
        (await StatusOfAsync(teacherId)).Should().Be("Cancelled");
    }
}
