namespace ClassManagement.Domain.Modules.Assignments.Enums;

/// <summary>
/// When a student may see their grade (MVP-5 config; display logic completed in MVP-6).
/// <c>Immediate</c> shows the auto-graded score right after submit; <c>AfterDeadline</c> waits for
/// the assignment to close; <c>Manual</c> waits for the teacher to release grades.
/// </summary>
public enum GradePublishPolicy
{
    Immediate,
    AfterDeadline,
    Manual,
}
