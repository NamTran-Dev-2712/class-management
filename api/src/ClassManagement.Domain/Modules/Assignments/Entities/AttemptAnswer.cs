namespace ClassManagement.Domain.Modules.Assignments.Entities;

/// <summary>
/// A student's answer to one snapshot question within an <see cref="Attempt"/> (MVP-5). Upserted on
/// every auto-save while the attempt is InProgress (one row per question, unique on
/// (attempt, snapshot_question)). <see cref="SelectedOptionIds"/> holds chosen snapshot-option ids for
/// choice questions; <see cref="TextAnswer"/> holds writing answers. <see cref="AutoScore"/> is filled
/// by auto-grading for objective questions on submit; writing answers wait for MVP-6 manual grading.
/// </summary>
public sealed class AttemptAnswer : AuditableEntity
{
    public long AttemptId { get; set; }
    public long SnapshotQuestionId { get; set; }
    public List<long>? SelectedOptionIds { get; set; }
    public string? TextAnswer { get; set; }
    public decimal? AutoScore { get; set; }
    public bool IsAutoGraded { get; set; }
    public DateTime LastSavedAt { get; set; }
    public DateTime? SubmittedAt { get; set; }
}
