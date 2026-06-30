using ClassManagement.Domain.Modules.Assignments.Enums;
using ClassManagement.Infrastructure.Persistence.DbContext;
using Microsoft.Extensions.Logging;

namespace ClassManagement.Infrastructure.Persistence.Seeds.Demo;

// Per teacher: the objective exam → an Open assignment (Immediate grade policy), the mixed exam → a
// Closed assignment (Manual grade policy, drives the grading + release demo), plus one Draft. Published
// assignments get an immutable snapshot built exactly like PublishAssignmentCommandHandler.
internal static class DemoAssignmentSeeder
{
    public static async Task SeedAsync(
        ApplicationDbContext context,
        DemoSeedState state,
        ILogger logger
    )
    {
        var now = DateTime.UtcNow;
        var questionsById = state.Questions.ToDictionary(q => q.Id);

        foreach (var teacher in state.Teachers)
        {
            var exams = state.ExamsOf(teacher.Id).ToList();
            var classes = state.ClassesOf(teacher.Id).ToList();
            if (exams.Count < 2 || classes.Count < 2)
                continue;

            // Open assignment (objective exam) — Immediate publishing, currently accepting attempts.
            var open = new Assignment
            {
                ExamId = exams[0].Id,
                ClassId = classes[0].Id,
                TeacherId = teacher.Id,
                Title = "Bài kiểm tra 15 phút - Tuần này",
                Description = "Làm bài trực tuyến, chấm điểm ngay.",
                OpensAt = now.AddDays(-2),
                ClosesAt = now.AddDays(7),
                TimeLimitMinutes = 30,
                MaxAttempts = 2,
                ScorePolicy = ScorePolicy.Highest,
                GradePublishPolicy = GradePublishPolicy.Immediate,
                ShowAnswersAfterGrade = true,
                Status = AssignmentStatus.Open,
                PublishedAt = now.AddDays(-2),
                ExamVersionAtPublish = exams[0].Version,
                CreatedAt = now,
                UpdatedAt = now,
            };
            open.Snapshot = BuildSnapshot(exams[0], questionsById, now);
            state.Assignments.Add(open);

            // Closed assignment (mixed exam) — Manual publishing, window already passed.
            var closed = new Assignment
            {
                ExamId = exams[1].Id,
                ClassId = classes[1].Id,
                TeacherId = teacher.Id,
                Title = "Bài kiểm tra 1 tiết - Đã kết thúc",
                Description = "Đã đóng; chờ giáo viên chấm tự luận và công bố điểm.",
                OpensAt = now.AddDays(-10),
                ClosesAt = now.AddDays(-1),
                TimeLimitMinutes = 45,
                MaxAttempts = 1,
                ScorePolicy = ScorePolicy.Highest,
                GradePublishPolicy = GradePublishPolicy.Manual,
                ShowAnswersAfterGrade = true,
                Status = AssignmentStatus.Closed,
                PublishedAt = now.AddDays(-10),
                ClosedAt = now.AddDays(-1),
                ExamVersionAtPublish = exams[1].Version,
                CreatedAt = now,
                UpdatedAt = now,
            };
            closed.Snapshot = BuildSnapshot(exams[1], questionsById, now);
            state.Assignments.Add(closed);

            // Draft assignment — editable, no snapshot yet.
            state.Assignments.Add(
                new Assignment
                {
                    ExamId = exams[0].Id,
                    ClassId = classes[1].Id,
                    TeacherId = teacher.Id,
                    Title = "Bài kiểm tra sắp tới (nháp)",
                    Description = "Chưa phát hành.",
                    TimeLimitMinutes = 30,
                    MaxAttempts = 1,
                    ScorePolicy = ScorePolicy.Highest,
                    GradePublishPolicy = GradePublishPolicy.AfterDeadline,
                    Status = AssignmentStatus.Draft,
                    CreatedAt = now,
                    UpdatedAt = now,
                }
            );
        }

        await context.Assignments.AddRangeAsync(state.Assignments);
        await context.SaveChangesAsync();

        logger.LogInformation("Seeded {Count} demo assignments", state.Assignments.Count);
    }

    private static AssignmentSnapshot BuildSnapshot(
        Exam exam,
        IReadOnlyDictionary<long, Question> questionsById,
        DateTime now
    )
    {
        var examQuestions = exam.Questions.OrderBy(eq => eq.DisplayOrder).ToList();

        var snapshot = new AssignmentSnapshot
        {
            TotalQuestions = examQuestions.Count,
            TotalPoint = examQuestions.Sum(eq => eq.Point),
            SnapshotCreatedAt = now,
        };

        var displayOrder = 1;
        foreach (var eq in examQuestions)
        {
            var question = questionsById[eq.QuestionId];
            var snapshotQuestion = new SnapshotQuestion
            {
                OriginalQuestionId = question.Id,
                Type = question.Type,
                Content = question.Content,
                Point = eq.Point,
                DisplayOrder = displayOrder++,
                Explanation = question.Explanation,
                SnapshotCreatedAt = now,
            };

            foreach (var opt in question.Options.OrderBy(o => o.DisplayOrder))
            {
                snapshotQuestion.Options.Add(
                    new SnapshotOption
                    {
                        OriginalOptionId = opt.Id,
                        Content = opt.Content,
                        IsCorrect = opt.IsCorrect,
                        DisplayOrder = opt.DisplayOrder,
                        SnapshotCreatedAt = now,
                    }
                );
            }

            snapshot.Questions.Add(snapshotQuestion);
        }

        return snapshot;
    }
}
