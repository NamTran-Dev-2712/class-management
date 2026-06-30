using ClassManagement.Domain.Modules.Admin.Constants;
using ClassManagement.Domain.Modules.Assignments.Enums;
using ClassManagement.Domain.Modules.Questions.Enums;
using ClassManagement.Infrastructure.Persistence.DbContext;
using Microsoft.Extensions.Logging;

namespace ClassManagement.Infrastructure.Persistence.Seeds.Demo;

// A few notifications, one pending report, and a handful of audit-log rows so the admin/student surfaces
// aren't empty on a fresh demo DB.
internal static class DemoModerationSeeder
{
    public static async Task SeedAsync(
        ApplicationDbContext context,
        DemoSeedState state,
        ILogger logger
    )
    {
        var now = DateTime.UtcNow;
        var teacher = state.Teachers.FirstOrDefault();
        var openAssignment = state.Assignments.FirstOrDefault(a =>
            a.Status == AssignmentStatus.Open
        );
        var publicQuestion = state.Questions.FirstOrDefault(q =>
            q.Visibility == QuestionVisibility.Public
        );

        // Notifications for the first couple of students.
        foreach (var student in state.Students.Take(2))
        {
            if (openAssignment is not null)
            {
                context.Notifications.Add(
                    new Notification
                    {
                        UserId = student.Id,
                        EventType = NotificationEventType.AssignmentCreated,
                        Title = "Bài tập mới",
                        Body = $"Bài tập \"{openAssignment.Title}\" đã được giao.",
                        Link = $"/student/assignments/{openAssignment.PublicId}",
                        ReferenceId = openAssignment.PublicId.ToString(),
                        Payload = new Dictionary<string, object?>
                        {
                            ["assignmentTitle"] = openAssignment.Title,
                        },
                        Status = NotificationStatus.Unread,
                        CreatedAt = now,
                    }
                );
            }

            context.Notifications.Add(
                new Notification
                {
                    UserId = student.Id,
                    EventType = NotificationEventType.GradePublished,
                    Title = "Điểm đã được công bố",
                    Body = "Giáo viên đã công bố điểm cho một bài kiểm tra của bạn.",
                    Status = NotificationStatus.Read,
                    ReadAt = now,
                    CreatedAt = now.AddDays(-1),
                }
            );
        }

        // One pending report on a public question.
        if (publicQuestion is not null && state.Students.Count > 0)
        {
            context.Reports.Add(
                new Report
                {
                    ReporterId = state.Students[0].Id,
                    TargetType = ReportTargetType.Question,
                    TargetId = publicQuestion.Id,
                    TargetPublicId = publicQuestion.PublicId,
                    Reason = ReportReason.IncorrectAnswer,
                    Description = "Mình nghĩ đáp án của câu này chưa chính xác.",
                    Status = ReportStatus.Pending,
                    CreatedAt = now,
                    UpdatedAt = now,
                }
            );
        }

        // A handful of audit-log rows (append-only, polymorphic, no FK).
        if (teacher is not null)
        {
            if (openAssignment is not null)
            {
                context.AuditLogs.Add(
                    BuildAudit(
                        AuditActions.AssignmentPublished,
                        teacher.Id,
                        AuditTargetTypes.Assignment,
                        openAssignment.Id,
                        openAssignment.PublicId,
                        now.AddDays(-2)
                    )
                );
            }

            context.AuditLogs.Add(
                BuildAudit(
                    AuditActions.PaymentCompleted,
                    teacher.Id,
                    AuditTargetTypes.Payment,
                    teacher.Id,
                    null,
                    now.AddDays(-5)
                )
            );
            context.AuditLogs.Add(
                BuildAudit(
                    AuditActions.InvoiceIssued,
                    teacher.Id,
                    AuditTargetTypes.Payment,
                    teacher.Id,
                    null,
                    now.AddDays(-5)
                )
            );
        }

        await context.SaveChangesAsync();
        logger.LogInformation("Seeded demo notifications, report, and audit logs");
    }

    private static AuditLog BuildAudit(
        string action,
        long actorId,
        string targetType,
        long targetId,
        Guid? targetPublicId,
        DateTime createdAt
    ) =>
        new()
        {
            Action = action,
            ActorId = actorId,
            ActorRole = AuditActorRoles.Teacher,
            TargetType = targetType,
            TargetId = targetId,
            TargetPublicId = targetPublicId,
            IpAddress = "127.0.0.1",
            UserAgent = "DemoSeeder",
            CreatedAt = createdAt,
        };
}
