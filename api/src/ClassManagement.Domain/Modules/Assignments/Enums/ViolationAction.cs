namespace ClassManagement.Domain.Modules.Assignments.Enums;

/// <summary>
/// What the server does when an attempt's <c>violation_count</c> exceeds the assignment's
/// <c>max_violations</c> (MVP-10). <c>WarnOnly</c> just logs, <c>AutoSubmit</c> finalizes the attempt via
/// the shared grading path (<c>auto_submitted = true</c>), <c>LockAttempt</c> flips <c>is_locked</c> so the
/// student is blocked until a teacher unlocks or force-submits. Never a new attempt status (BR-10-06).
/// </summary>
public enum ViolationAction
{
    WarnOnly,
    AutoSubmit,
    LockAttempt,
}
