namespace ClassManagement.Infrastructure.Configuration;

public sealed class ClassroomOptions
{
    public const string SectionName = "Classroom";

    /// <summary>
    /// Max number of classes a teacher may own (Active + Archived, excluding soft-deleted).
    /// <c>0</c> = unlimited (MVP-2 default; tightened for premium tiers in MVP-8).
    /// </summary>
    public int MaxClassesPerTeacher { get; init; } = 0;
}
