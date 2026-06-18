namespace ClassManagement.Domain.Modules.Assignments.Enums;

/// <summary>
/// Lifecycle of an <see cref="Entities.Assignment"/> (MVP-5). <c>Draft</c> is editable and has no
/// snapshot; publishing builds the immutable snapshot and moves to <c>Scheduled</c> (opens later) or
/// <c>Open</c> (open now); <c>Closed</c> stops new attempts; <c>Archived</c> hides it.
/// </summary>
public enum AssignmentStatus
{
    Draft,
    Scheduled,
    Open,
    Closed,
    Archived,
}
