using ClassManagement.Domain.Modules.Exams.Enums;
using ClassManagement.Infrastructure.Persistence.DbContext;
using Microsoft.Extensions.Logging;

namespace ClassManagement.Infrastructure.Persistence.Seeds.Demo;

// Each teacher gets 2 exams built from their own questions: one all-objective (auto-graded end to end),
// one mixing writing questions (drives the manual-grading demo). Totals/version computed in-code exactly
// like UpdateExamQuestionsCommandHandler.
internal static class DemoExamSeeder
{
    public static async Task SeedAsync(
        ApplicationDbContext context,
        DemoSeedState state,
        ILogger logger
    )
    {
        var now = DateTime.UtcNow;

        foreach (var teacher in state.Teachers)
        {
            var questions = state.QuestionsOf(teacher.Id).ToList();
            if (questions.Count < 5)
                continue;

            state.Exams.Add(
                BuildExam(
                    teacher,
                    "Đề kiểm tra 15 phút (trắc nghiệm)",
                    "Đề toàn câu trắc nghiệm, chấm tự động.",
                    ExamVisibility.Public,
                    questions.Take(5).ToList(),
                    ["kiem-tra-15-phut", "trac-nghiem"],
                    now
                )
            );

            state.Exams.Add(
                BuildExam(
                    teacher,
                    "Đề kiểm tra 1 tiết (tổng hợp)",
                    "Đề có cả câu tự luận, cần chấm tay.",
                    ExamVisibility.Private,
                    questions.Skip(3).Take(5).ToList(),
                    ["kiem-tra-1-tiet", "tong-hop"],
                    now
                )
            );
        }

        await context.Exams.AddRangeAsync(state.Exams);
        await context.SaveChangesAsync();

        logger.LogInformation("Seeded {Count} demo exams", state.Exams.Count);
    }

    private static Exam BuildExam(
        ApplicationUser teacher,
        string title,
        string description,
        ExamVisibility visibility,
        List<Question> questions,
        string[] tags,
        DateTime now
    )
    {
        var exam = new Exam
        {
            TeacherId = teacher.Id,
            SubjectId = questions[0].SubjectId,
            Title = title,
            Description = description,
            Visibility = visibility,
            Version = 1,
            CreatedAt = now,
            UpdatedAt = now,
        };

        var order = 1;
        foreach (var question in questions)
        {
            exam.Questions.Add(
                new ExamQuestion
                {
                    QuestionId = question.Id,
                    DisplayOrder = order++,
                    Point = question.SuggestedPoint,
                    CreatedAt = now,
                }
            );
        }

        foreach (var tag in tags)
            exam.Tags.Add(new ExamTag { Tag = tag, CreatedAt = now });

        exam.TotalQuestions = exam.Questions.Count;
        exam.TotalPoint = exam.Questions.Sum(q => q.Point);
        return exam;
    }
}
