using ClassManagement.Application.Modules.Assignments.Interfaces;
using ClassManagement.Domain.Modules.Assignments.Enums;
using ClassManagement.Domain.Modules.Classroom.Enums;
using ClassManagement.Domain.Modules.Payment.Enums;
using ClassManagement.Infrastructure.Identity;
using ClassManagement.Infrastructure.Persistence.DbContext;
using ClassManagement.Infrastructure.Persistence.Seeds.Demo;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;

namespace ClassManagement.IntegrationTests.Seeding;

// Directly exercises DemoDataSeeder against this class's isolated Testcontainer DB (bypassing the
// env/flag gate, which only governs the app startup path). Verifies the end-to-end graph is built
// correctly and that the seeder is idempotent. This class owns its own ApiFactory, so the demo rows it
// creates never leak into the gating tests (which use a separate, fresh container).
public class DemoDataSeederTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    private readonly ApiFactory _factory = factory;

    private async Task RunSeederAsync() =>
        await _factory.WithScopeAsync<object?>(async sp =>
        {
            var context = sp.GetRequiredService<ApplicationDbContext>();
            var userManager = sp.GetRequiredService<UserManager<ApplicationUser>>();
            var grader = sp.GetRequiredService<IAutoGradingService>();
            await DemoDataSeeder.SeedAsync(context, userManager, grader, NullLogger.Instance);
            return null;
        });

    private Task<T> QueryAsync<T>(Func<ApplicationDbContext, Task<T>> query) =>
        _factory.WithScopeAsync(sp => query(sp.GetRequiredService<ApplicationDbContext>()));

    [Fact]
    public async Task Seed_BuildsFullDemoGraph()
    {
        await RunSeederAsync();

        var teacherCount = await QueryAsync(c =>
            c.Users.CountAsync(u =>
                u.Email!.EndsWith("@demo.local") && u.Email.StartsWith("teacher")
            )
        );
        teacherCount.Should().Be(3);

        var studentCount = await QueryAsync(c =>
            c.Users.CountAsync(u =>
                u.Email!.StartsWith("student") && u.Email.EndsWith("@demo.local")
            )
        );
        studentCount.Should().Be(25);

        (await QueryAsync(c => c.Classes.CountAsync(x => x.InviteCode.StartsWith("DEMO"))))
            .Should()
            .Be(6);
        (
            await QueryAsync(c =>
                c.ClassMemberships.CountAsync(m => m.Status == MembershipStatus.Approved)
            )
        )
            .Should()
            .BeGreaterThan(0);
        (await QueryAsync(c => c.Questions.CountAsync())).Should().BeGreaterThanOrEqualTo(24);
        (await QueryAsync(c => c.Exams.CountAsync())).Should().Be(6);
        (await QueryAsync(c => c.Assignments.CountAsync())).Should().Be(9);
        (await QueryAsync(c => c.AssignmentSnapshots.CountAsync())).Should().Be(6);

        // Attempts: auto-graded, fully graded, and at least one still pending manual grading.
        (await QueryAsync(c => c.Attempts.CountAsync()))
            .Should()
            .BeGreaterThan(0);
        (await QueryAsync(c => c.Attempts.CountAsync(a => a.Status == AttemptStatus.Graded)))
            .Should()
            .BeGreaterThan(0);
        (
            await QueryAsync(c =>
                c.Attempts.CountAsync(a => a.Status == AttemptStatus.NeedManualGrading)
            )
        )
            .Should()
            .BeGreaterThan(0);
        (await QueryAsync(c => c.ManualGrades.CountAsync())).Should().BeGreaterThan(0);

        // Payment graph.
        (
            await QueryAsync(c =>
                c.Subscriptions.CountAsync(s => s.Status == SubscriptionStatus.Active)
            )
        )
            .Should()
            .Be(1);
        (await QueryAsync(c => c.Payments.CountAsync(p => p.Status == PaymentStatus.Completed)))
            .Should()
            .Be(1);
        (await QueryAsync(c => c.Invoices.CountAsync())).Should().Be(1);

        // Admin surfaces populated.
        (await QueryAsync(c => c.Notifications.CountAsync()))
            .Should()
            .BeGreaterThan(0);
        (await QueryAsync(c => c.Reports.CountAsync())).Should().BeGreaterThan(0);
        (await QueryAsync(c => c.AuditLogs.CountAsync())).Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task Seed_IsIdempotent()
    {
        await RunSeederAsync();
        var firstTeacherCount = await QueryAsync(c =>
            c.Users.CountAsync(u => u.Email!.EndsWith("@demo.local"))
        );

        await RunSeederAsync(); // second run should be a no-op (marker user already exists)

        var secondTeacherCount = await QueryAsync(c =>
            c.Users.CountAsync(u => u.Email!.EndsWith("@demo.local"))
        );
        secondTeacherCount.Should().Be(firstTeacherCount);
    }
}
