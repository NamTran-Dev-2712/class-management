using ClassManagement.Infrastructure.Persistence.DbContext;
using Microsoft.Extensions.Logging;

namespace ClassManagement.Infrastructure.Persistence.Seeds.Demo;

// Puts the first demo teacher on an Active Pro (Monthly) subscription with a completed payment and an
// immutable invoice (number from the payment_invoice_number_seq sequence, like InvoiceRepository).
internal static class DemoPaymentSeeder
{
    public static async Task SeedAsync(
        ApplicationDbContext context,
        DemoSeedState state,
        ILogger logger
    )
    {
        var teacher = state.Teachers.FirstOrDefault();
        if (teacher is null)
            return;

        var plan = await context.Plans.FirstOrDefaultAsync(p =>
            p.Name == "Pro" && p.BillingCycle == BillingCycle.Monthly
        );
        if (plan is null)
        {
            logger.LogWarning("Pro Monthly plan not found — skipping demo payment seed.");
            return;
        }

        var startedAt = DateTime.UtcNow.AddDays(-5);

        var subscription = new Subscription
        {
            TeacherId = teacher.Id,
            PlanId = plan.Id,
            Status = SubscriptionStatus.Active,
            StartedAt = startedAt,
            ExpiresAt = startedAt.AddMonths(1),
            PaymentType = SubscriptionPaymentType.Auto,
            CreatedAt = startedAt,
            UpdatedAt = startedAt,
        };
        await context.Subscriptions.AddAsync(subscription);
        await context.SaveChangesAsync();

        var payment = new Payment
        {
            SubscriptionId = subscription.Id,
            TeacherId = teacher.Id,
            PlanId = plan.Id,
            Provider = PaymentProvider.Momo,
            BillingCycle = BillingCycle.Monthly,
            AmountVnd = plan.PriceVnd,
            Status = PaymentStatus.Completed,
            IdempotencyKey = $"momo_demo-{teacher.Id}",
            ProviderOrderId = $"DEMO-ORDER-{teacher.Id}",
            ProviderTransactionId = $"DEMO-TXN-{teacher.Id}",
            CompletedAt = startedAt,
            CreatedAt = startedAt,
        };
        await context.Payments.AddAsync(payment);
        await context.SaveChangesAsync();

        var sequence = await NextInvoiceNumberAsync(context);
        var invoice = new Invoice
        {
            PaymentId = payment.Id,
            SubscriptionId = subscription.Id,
            TeacherId = teacher.Id,
            InvoiceNumber = $"INV-{startedAt:yyyyMM}-{sequence:D6}",
            AmountVnd = payment.AmountVnd,
            PlanName = plan.Name,
            BillingCycle = BillingCycle.Monthly,
            CreatedAt = startedAt,
        };
        await context.Invoices.AddAsync(invoice);
        await context.SaveChangesAsync();

        logger.LogInformation(
            "Seeded demo Pro subscription + payment + invoice for {Email}",
            teacher.Email
        );
    }

    private static async Task<long> NextInvoiceNumberAsync(ApplicationDbContext context)
    {
        var values = await context
            .Database.SqlQueryRaw<long>("SELECT nextval('payment_invoice_number_seq') AS \"Value\"")
            .ToListAsync();
        return values[0];
    }
}
