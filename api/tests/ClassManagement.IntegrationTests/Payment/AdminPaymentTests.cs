using ClassManagement.Infrastructure.Persistence.DbContext;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace ClassManagement.IntegrationTests.Payment;

// MVP-8 Admin surface — manual-set Pro (A8-02), subscription list (A8-01), payment history + revenue
// (A8-03/A8-04), and the teacher invoice download (T8-03/T8-07). Uses the Fake provider.
public class AdminPaymentTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    private readonly ApiFactory _factory = factory;

    private static async Task<(Guid planId, long price)> GetProMonthlyAsync(HttpClient client)
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

    private Task<Guid> TeacherPublicIdAsync(string email) =>
        _factory.WithScopeAsync(async sp =>
        {
            var db = sp.GetRequiredService<ApplicationDbContext>();
            return await db.Users.Where(u => u.Email == email).Select(u => u.PublicId).FirstAsync();
        });

    private static async Task CompletePurchaseAsync(
        HttpClient teacher,
        Guid planId,
        string provider
    )
    {
        var checkout = await teacher.PostAsJsonAsync(
            "/api/teacher/subscription/checkout",
            new { planPublicId = planId, provider }
        );
        var checkoutData = await AuthHelper.ReadDataAsync(checkout);
        var orderRef = ExtractOrderRef(checkoutData.GetProperty("redirectUrl").GetString()!);
        var webhook = await teacher.PostAsJsonAsync(
            $"/api/payments/webhook/{provider.ToLowerInvariant()}",
            new { orderRef, outcome = "Succeeded" }
        );
        webhook.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task AdminManualSet_GrantsProToTeacher()
    {
        var email = TestConstants.UniqueEmail();
        var teacher = await AuthHelper.CreateRoleClientAsync(_factory, "Teacher", email);
        var admin = await AuthHelper.CreateAdminClientAsync(_factory);
        var (planId, _) = await GetProMonthlyAsync(teacher);
        var teacherPublicId = await TeacherPublicIdAsync(email);

        var set = await admin.PostAsJsonAsync(
            "/api/admin/subscriptions/manual-set",
            new
            {
                teacherPublicId,
                planPublicId = planId,
                adminNote = "Partnership",
            }
        );
        set.StatusCode.Should().Be(HttpStatusCode.OK);

        // Teacher is now Pro.
        var usage = await teacher.GetAsync("/api/teacher/subscription/usage");
        var usageData = await AuthHelper.ReadDataAsync(usage);
        usageData.GetProperty("isPro").GetBoolean().Should().BeTrue();

        // Admin list shows the manual subscription.
        var list = await admin.GetAsync($"/api/admin/subscriptions?searchTerm={email}");
        var listData = await AuthHelper.ReadDataAsync(list);
        var items = listData.GetProperty("items");
        items.GetArrayLength().Should().Be(1);
        items[0].GetProperty("paymentType").GetString().Should().Be("Manual");
        items[0].GetProperty("adminNote").GetString().Should().Be("Partnership");
    }

    [Fact]
    public async Task AdminPaymentsAndRevenue_ReflectCompletedPurchase()
    {
        var teacher = await AuthHelper.CreateRoleClientAsync(_factory, "Teacher");
        var admin = await AuthHelper.CreateAdminClientAsync(_factory);
        var (planId, price) = await GetProMonthlyAsync(teacher);

        await CompletePurchaseAsync(teacher, planId, "Momo");

        var payments = await admin.GetAsync("/api/admin/payments?status=Completed");
        var paymentsData = await AuthHelper.ReadDataAsync(payments);
        paymentsData.GetProperty("totalCount").GetInt32().Should().BeGreaterThanOrEqualTo(1);

        var revenue = await admin.GetAsync("/api/admin/payments/revenue");
        var revenueData = await AuthHelper.ReadDataAsync(revenue);
        revenueData
            .GetProperty("totalRevenueVnd")
            .GetInt64()
            .Should()
            .BeGreaterThanOrEqualTo(price);
        revenueData.GetProperty("byMonth").GetArrayLength().Should().BeGreaterThanOrEqualTo(1);
    }

    [Fact]
    public async Task TeacherInvoice_ListAndDownloadPdf()
    {
        var teacher = await AuthHelper.CreateRoleClientAsync(_factory, "Teacher");
        var (planId, _) = await GetProMonthlyAsync(teacher);

        await CompletePurchaseAsync(teacher, planId, "VnPay");

        var list = await teacher.GetAsync("/api/teacher/subscription/invoices");
        var listData = await AuthHelper.ReadDataAsync(list);
        listData.GetArrayLength().Should().Be(1);
        var invoiceId = listData[0].GetProperty("publicId").GetGuid();
        listData[0].GetProperty("invoiceNumber").GetString().Should().StartWith("INV-");

        var pdf = await teacher.GetAsync($"/api/teacher/subscription/invoices/{invoiceId}/pdf");
        pdf.StatusCode.Should().Be(HttpStatusCode.OK);
        pdf.Content.Headers.ContentType!.MediaType.Should().Be("application/pdf");
        var bytes = await pdf.Content.ReadAsByteArrayAsync();
        bytes.Length.Should().BeGreaterThan(4);
        System.Text.Encoding.ASCII.GetString(bytes, 0, 4).Should().Be("%PDF");
    }

    [Fact]
    public async Task AdminEndpoints_ForbiddenForTeacher()
    {
        var teacher = await AuthHelper.CreateRoleClientAsync(_factory, "Teacher");
        var subs = await teacher.GetAsync("/api/admin/subscriptions");
        subs.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        var payments = await teacher.GetAsync("/api/admin/payments");
        payments.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
