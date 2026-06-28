using ClassManagement.Infrastructure.Persistence.DbContext;
using Microsoft.Extensions.Logging;

namespace ClassManagement.Infrastructure.Persistence.Seeds;

// Seeds the default plans (Free, Pro Monthly, Pro Annual) for MVP-8. Idempotent and additive: keyed on
// (name, billing_cycle) so an admin's later price/limit edits are never overwritten. Feature labels are
// i18n keys the frontend localizes (pricing namespace).
public static class PlanSeeder
{
    private static readonly (
        string Name,
        BillingCycle? Cycle,
        long Price,
        int? MaxClasses,
        int? MaxQuestions,
        int? MaxExams,
        int? MaxStudents,
        int Order,
        string[] Features
    )[] Defaults =
    [
        (
            "Free",
            null,
            0L,
            10,
            500,
            50,
            100,
            0,
            ["maxClasses", "maxQuestions", "maxExams", "communitySupport"]
        ),
        (
            "Pro",
            BillingCycle.Monthly,
            99_000L,
            null,
            null,
            null,
            null,
            1,
            ["unlimitedClasses", "unlimitedQuestions", "unlimitedExams", "prioritySupport"]
        ),
        (
            "Pro",
            BillingCycle.Annual,
            990_000L,
            null,
            null,
            null,
            null,
            2,
            [
                "unlimitedClasses",
                "unlimitedQuestions",
                "unlimitedExams",
                "prioritySupport",
                "annualDiscount",
            ]
        ),
    ];

    public static async Task SeedAsync(ApplicationDbContext context, ILogger logger)
    {
        var existing = await context
            .Plans.Select(p => new { p.Name, p.BillingCycle })
            .ToListAsync();
        var existingSet = existing.Select(e => (e.Name, e.BillingCycle)).ToHashSet();

        var toAdd = Defaults
            .Where(d => !existingSet.Contains((d.Name, d.Cycle)))
            .Select(d => new Plan
            {
                Name = d.Name,
                BillingCycle = d.Cycle,
                PriceVnd = d.Price,
                MaxClasses = d.MaxClasses,
                MaxQuestions = d.MaxQuestions,
                MaxExams = d.MaxExams,
                MaxStudentsPerClass = d.MaxStudents,
                IsActive = true,
                DisplayOrder = d.Order,
                Features = [.. d.Features],
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
            })
            .ToList();

        if (toAdd.Count == 0)
            return;

        await context.Plans.AddRangeAsync(toAdd);
        await context.SaveChangesAsync();
        logger.LogInformation("Seeded {Count} plan(s)", toAdd.Count);
    }
}
