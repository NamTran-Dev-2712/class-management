using ClassManagement.Application.Modules.Assignments.Interfaces;
using ClassManagement.Infrastructure.Persistence.DbContext;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace ClassManagement.Infrastructure.Persistence.Seeds.Demo;

/// <summary>
/// Dev-only, end-to-end demo dataset (users → classes → questions → exams → published assignments +
/// snapshots → attempts + auto/manual grading → a Pro subscription + payment + invoice → a few
/// notifications/reports/audit logs). Gated by <c>IHostEnvironment.IsDevelopment()</c> AND the
/// <c>Seed:DemoData</c> flag in <see cref="DatabaseSeeder"/>; never runs in Production or the test host.
/// Idempotent: skips entirely when the marker user (<see cref="MarkerEmail"/>) already exists. Builds
/// rows directly through <see cref="ApplicationDbContext"/> (and <see cref="UserManager{T}"/> for users)
/// — it must not send MediatR commands, which depend on the HTTP-bound <c>ICurrentUserService</c>.
/// </summary>
public static class DemoDataSeeder
{
    public const string MarkerEmail = "teacher1@demo.local";

    public static async Task SeedAsync(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        IAutoGradingService autoGrader,
        ILogger logger
    )
    {
        if (await userManager.FindByEmailAsync(MarkerEmail) is not null)
        {
            logger.LogInformation("Demo data already present — skipping demo seed.");
            return;
        }

        logger.LogInformation("Seeding demo data (development only)...");

        var state = new DemoSeedState
        {
            Subjects = await context.Subjects.OrderBy(s => s.DisplayOrder).ToListAsync(),
        };

        await DemoUserSeeder.SeedAsync(userManager, state, logger);
        await DemoClassroomSeeder.SeedAsync(context, state, logger);
        await DemoQuestionSeeder.SeedAsync(context, state, logger);
        await DemoExamSeeder.SeedAsync(context, state, logger);
        await DemoAssignmentSeeder.SeedAsync(context, state, logger);
        await DemoAttemptSeeder.SeedAsync(context, autoGrader, state, logger);
        await DemoPaymentSeeder.SeedAsync(context, state, logger);
        await DemoModerationSeeder.SeedAsync(context, state, logger);

        logger.LogInformation("Demo data seeding complete.");
    }
}
