namespace ClassManagement.Infrastructure.Configuration;

public sealed class ExamOptions
{
    public const string SectionName = "ExamBuilder";

    /// <summary>
    /// Max number of exams a teacher may own (excluding soft-deleted). <c>0</c> = unlimited (MVP-4
    /// default; tightened for premium tiers in MVP-8). See BR-4-09.
    /// </summary>
    public int MaxExamsPerTeacher { get; init; } = 0;
}
