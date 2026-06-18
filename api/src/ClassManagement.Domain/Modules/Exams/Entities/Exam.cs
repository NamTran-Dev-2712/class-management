using ClassManagement.Domain.Modules.Exams.Enums;

namespace ClassManagement.Domain.Modules.Exams.Entities;

/// <summary>
/// A reusable exam template (MVP-4): an ordered set of questions with per-question points. Owns its
/// <see cref="Questions"/> collection (replaced wholesale when the teacher saves the question list).
/// <see cref="SubjectId"/> is optional (nullable FK SET NULL). <see cref="Version"/> increments by one
/// each time the question list changes (add/remove/reorder/repoint) so MVP-5 can record which version
/// an Assignment snapshot was created from; editing title/description/visibility does not bump it.
/// <see cref="TotalPoint"/>/<see cref="TotalQuestions"/> are denormalized caches recomputed in the
/// write handler alongside the version bump.
/// </summary>
public sealed class Exam : AuditableEntity, IHasPublicId, ISoftDeletable
{
    public Guid PublicId { get; set; }
    public long TeacherId { get; set; }
    public long? SubjectId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public ExamVisibility Visibility { get; set; } = ExamVisibility.Private;
    public int Version { get; set; } = 1;
    public decimal TotalPoint { get; set; }
    public int TotalQuestions { get; set; }
    public DateTime? DeletedAt { get; set; }

    public ICollection<ExamQuestion> Questions { get; set; } = [];
    public ICollection<ExamTag> Tags { get; set; } = [];
}
