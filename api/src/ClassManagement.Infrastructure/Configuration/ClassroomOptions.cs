namespace ClassManagement.Infrastructure.Configuration;

public sealed class ClassroomOptions
{
    public const string SectionName = "Classroom";

    /// <summary>
    /// Max number of classes a teacher may own (Active + Archived, excluding soft-deleted).
    /// <c>0</c> = unlimited (MVP-2 default; tightened for premium tiers in MVP-8).
    /// </summary>
    public int MaxClassesPerTeacher { get; init; } = 0;

    /// <summary>Fallback max approved students per class; <c>0</c> = unlimited. Live value: system_settings.</summary>
    public int MaxStudentsPerClass { get; init; } = 0;

    /// <summary>Fallback invite-code length (clamped 6–12). Live value: system_settings.</summary>
    public int InviteCodeLength { get; init; } = 8;
}
