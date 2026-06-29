using ClassManagement.Domain.Modules.Questions.Enums;
using ClassManagement.Infrastructure.Persistence.DbContext;
using Microsoft.Extensions.Logging;

namespace ClassManagement.Infrastructure.Persistence.Seeds.Demo;

// Each teacher gets a question bank spanning all 5 types + 3 difficulties, with options/tags and a mix
// of Public/Private. Content is Markdown (rendered safely on the client).
internal static class DemoQuestionSeeder
{
    public static async Task SeedAsync(
        ApplicationDbContext context,
        DemoSeedState state,
        ILogger logger
    )
    {
        var now = DateTime.UtcNow;
        var subjectId = state.Subjects.FirstOrDefault()?.Id;

        foreach (var teacher in state.Teachers)
        {
            foreach (var (q, opts) in BuildTemplates())
            {
                var question = new Question
                {
                    TeacherId = teacher.Id,
                    SubjectId = subjectId,
                    Type = q.Type,
                    Content = q.Content,
                    Difficulty = q.Difficulty,
                    SuggestedPoint = q.Point,
                    Visibility = q.Visibility,
                    Explanation = q.Explanation,
                    CreatedAt = now,
                    UpdatedAt = now,
                };

                var order = 0;
                foreach (var (content, isCorrect) in opts)
                {
                    question.Options.Add(
                        new QuestionOption
                        {
                            Content = content,
                            IsCorrect = isCorrect,
                            DisplayOrder = order++,
                            CreatedAt = now,
                        }
                    );
                }

                foreach (var tag in q.Tags)
                    question.Tags.Add(new QuestionTag { Tag = tag, CreatedAt = now });

                state.Questions.Add(question);
            }
        }

        await context.Questions.AddRangeAsync(state.Questions);
        await context.SaveChangesAsync();

        logger.LogInformation("Seeded {Count} demo questions", state.Questions.Count);
    }

    private readonly record struct QuestionTemplate(
        QuestionType Type,
        string Content,
        QuestionDifficulty Difficulty,
        decimal Point,
        QuestionVisibility Visibility,
        string? Explanation,
        string[] Tags
    );

    private static List<(
        QuestionTemplate Question,
        (string Content, bool IsCorrect)[] Options
    )> BuildTemplates() =>
        [
            (
                new QuestionTemplate(
                    QuestionType.SingleChoice,
                    "Kết quả của phép tính **2 + 3 × 4** là bao nhiêu?",
                    QuestionDifficulty.Easy,
                    1.0m,
                    QuestionVisibility.Public,
                    "Nhân chia trước, cộng trừ sau: 3 × 4 = 12, rồi 2 + 12 = 14.",
                    ["dai-so", "thu-tu-phep-tinh"]
                ),
                [("14", true), ("20", false), ("24", false), ("9", false)]
            ),
            (
                new QuestionTemplate(
                    QuestionType.SingleChoice,
                    "Đường tròn có bán kính `r`. Chu vi của nó là?",
                    QuestionDifficulty.Medium,
                    1.0m,
                    QuestionVisibility.Private,
                    "Chu vi đường tròn = 2πr.",
                    ["hinh-hoc", "duong-tron"]
                ),
                [("2πr", true), ("πr²", false), ("πr", false), ("4πr", false)]
            ),
            (
                new QuestionTemplate(
                    QuestionType.MultipleChoice,
                    "Chọn **tất cả** các số nguyên tố trong danh sách sau:",
                    QuestionDifficulty.Medium,
                    2.0m,
                    QuestionVisibility.Public,
                    "Số nguyên tố chỉ chia hết cho 1 và chính nó: 2, 7, 11.",
                    ["so-hoc", "so-nguyen-to"]
                ),
                [("2", true), ("7", true), ("9", false), ("11", true), ("15", false)]
            ),
            (
                new QuestionTemplate(
                    QuestionType.MultipleChoice,
                    "Những phát biểu nào sau đây **đúng** về hình vuông?",
                    QuestionDifficulty.Hard,
                    2.0m,
                    QuestionVisibility.Private,
                    "Hình vuông có 4 cạnh bằng nhau và 4 góc vuông; nó là trường hợp đặc biệt của hình chữ nhật.",
                    ["hinh-hoc", "tu-giac"]
                ),
                [
                    ("Có 4 cạnh bằng nhau", true),
                    ("Có 4 góc vuông", true),
                    ("Là một hình chữ nhật đặc biệt", true),
                    ("Có đúng 3 cạnh", false),
                ]
            ),
            (
                new QuestionTemplate(
                    QuestionType.TrueFalse,
                    "Phát biểu: *Tổng ba góc trong của một tam giác bằng 180°.*",
                    QuestionDifficulty.Easy,
                    1.0m,
                    QuestionVisibility.Public,
                    "Đây là định lý cơ bản của hình học phẳng.",
                    ["hinh-hoc", "tam-giac"]
                ),
                [("Đúng", true), ("Sai", false)]
            ),
            (
                new QuestionTemplate(
                    QuestionType.ShortWriting,
                    "Nêu **định nghĩa** của số chính phương và cho một ví dụ.",
                    QuestionDifficulty.Medium,
                    2.0m,
                    QuestionVisibility.Private,
                    "Số chính phương là bình phương của một số nguyên, ví dụ 9 = 3².",
                    ["so-hoc", "tu-luan"]
                ),
                []
            ),
            (
                new QuestionTemplate(
                    QuestionType.ShortWriting,
                    "Viết công thức tính diện tích hình tam giác và giải thích các đại lượng.",
                    QuestionDifficulty.Easy,
                    1.5m,
                    QuestionVisibility.Public,
                    "S = (1/2) × đáy × chiều cao.",
                    ["hinh-hoc", "dien-tich", "tu-luan"]
                ),
                []
            ),
            (
                new QuestionTemplate(
                    QuestionType.LongWriting,
                    "Trình bày các bước **giải một phương trình bậc hai** `ax² + bx + c = 0` bằng công thức nghiệm, kèm ví dụ minh hoạ.",
                    QuestionDifficulty.Hard,
                    3.0m,
                    QuestionVisibility.Private,
                    "Tính delta = b² - 4ac, biện luận theo dấu của delta, rồi áp dụng công thức nghiệm.",
                    ["dai-so", "phuong-trinh", "tu-luan"]
                ),
                []
            ),
        ];
}
