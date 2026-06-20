namespace ClassManagement.Domain.Modules.Assignments.Entities;

/// <summary>
/// A teacher's manual score + feedback for one writing snapshot question within an
/// <see cref="Attempt"/> (MVP-6). One row per (attempt, snapshot_question) — upserted when the teacher
/// grades or edits a writing answer. The auditable columns carry the grading audit:
/// <see cref="AuditableEntity.CreatedAt"/>/<see cref="AuditableEntity.CreatedBy"/> = first grade
/// (graded_at/graded_by), <see cref="AuditableEntity.UpdatedAt"/>/<see cref="AuditableEntity.UpdatedBy"/>
/// = last edit. <see cref="Score"/> is validated app-side to be within [0, snapshot_question.point].
/// </summary>
public sealed class ManualGrade : AuditableEntity
{
    public long AttemptId { get; set; }
    public long SnapshotQuestionId { get; set; }
    public decimal Score { get; set; }
    public string? Feedback { get; set; }
}
