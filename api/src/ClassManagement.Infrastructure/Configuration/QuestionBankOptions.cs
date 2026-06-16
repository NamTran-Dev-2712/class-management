namespace ClassManagement.Infrastructure.Configuration;

public sealed class QuestionBankOptions
{
    public const string SectionName = "QuestionBank";

    /// <summary>
    /// Max number of questions a teacher may own (excluding soft-deleted). <c>0</c> = unlimited
    /// (MVP-3 default; tightened for premium tiers in MVP-8). See BR-3-11.
    /// </summary>
    public int MaxQuestionsPerTeacher { get; init; } = 0;
}
