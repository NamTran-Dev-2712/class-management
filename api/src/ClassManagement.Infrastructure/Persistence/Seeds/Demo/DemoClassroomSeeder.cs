using ClassManagement.Domain.Modules.Classroom.Enums;
using ClassManagement.Infrastructure.Persistence.DbContext;
using Microsoft.Extensions.Logging;

namespace ClassManagement.Infrastructure.Persistence.Seeds.Demo;

// Each teacher owns 2 classes (one linked to a catalog subject + snapshot name, one free-text). Each
// class gets a realistic mix of Approved / Pending / Rejected memberships drawn from the student pool.
internal static class DemoClassroomSeeder
{
    public static async Task SeedAsync(
        ApplicationDbContext context,
        DemoSeedState state,
        ILogger logger
    )
    {
        var now = DateTime.UtcNow;
        var inviteSeq = 1;
        var classDefs = new List<(ApplicationUser Teacher, string Name, bool UseCatalogSubject)>();

        foreach (var teacher in state.Teachers)
        {
            classDefs.Add((teacher, $"Lớp 10A - {teacher.DisplayName}", true));
            classDefs.Add((teacher, $"Lớp Ôn Tập - {teacher.DisplayName}", false));
        }

        var subjectIndex = 0;
        foreach (var (teacher, name, useCatalog) in classDefs)
        {
            var subject =
                state.Subjects.Count > 0
                    ? state.Subjects[subjectIndex++ % state.Subjects.Count]
                    : null;

            var @class = new Class
            {
                Name = name,
                Description = "Lớp học demo phục vụ kiểm thử.",
                OwnerId = teacher.Id,
                SubjectId = useCatalog ? subject?.Id : null,
                SubjectName = useCatalog ? subject?.Name : "Luyện thi tổng hợp",
                InviteCode = $"DEMO{inviteSeq++:D4}",
                Status = ClassStatus.Active,
                CreatedAt = now,
                UpdatedAt = now,
            };
            state.Classes.Add(@class);
        }

        await context.Classes.AddRangeAsync(state.Classes);
        await context.SaveChangesAsync();

        // Memberships — rotate a window over the student pool so classes differ, then approve most,
        // leave a couple pending, reject one.
        var offset = 0;
        foreach (var @class in state.Classes)
        {
            var approved = PickStudents(state.Students, offset, 8);
            var pending = PickStudents(state.Students, offset + 8, 2);
            var rejected = PickStudents(state.Students, offset + 10, 1);
            offset += 3;

            foreach (var student in approved)
                AddMembership(context, @class, student, MembershipStatus.Approved, now);
            foreach (var student in pending)
                AddMembership(context, @class, student, MembershipStatus.Pending, now);
            foreach (var student in rejected)
                AddMembership(context, @class, student, MembershipStatus.Rejected, now);
        }

        await context.SaveChangesAsync();

        logger.LogInformation(
            "Seeded {ClassCount} demo classes with memberships",
            state.Classes.Count
        );
    }

    private static void AddMembership(
        ApplicationDbContext context,
        Class @class,
        ApplicationUser student,
        MembershipStatus status,
        DateTime now
    )
    {
        var joinedAt = now.AddDays(-14);
        var membership = new ClassMembership
        {
            ClassId = @class.Id,
            StudentId = student.Id,
            Status = status,
            JoinedAt = joinedAt,
            ProcessedAt = status == MembershipStatus.Pending ? null : joinedAt.AddHours(6),
            ProcessedBy = status == MembershipStatus.Pending ? null : @class.OwnerId,
            RejectionReason =
                status == MembershipStatus.Rejected ? "Không thuộc danh sách lớp." : null,
            CreatedAt = joinedAt,
        };
        @class.Memberships.Add(membership);
        context.ClassMemberships.Add(membership);
    }

    // Picks `count` students starting at `start`, wrapping around the pool (students may belong to
    // more than one class — realistic and harmless since membership is per (class, student)).
    private static List<ApplicationUser> PickStudents(
        List<ApplicationUser> pool,
        int start,
        int count
    )
    {
        if (pool.Count == 0)
            return [];

        var picked = new List<ApplicationUser>(count);
        for (var i = 0; i < count; i++)
            picked.Add(pool[(start + i) % pool.Count]);
        return picked;
    }
}
