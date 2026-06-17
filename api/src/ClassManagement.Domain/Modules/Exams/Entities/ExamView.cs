namespace ClassManagement.Domain.Modules.Exams.Entities;

/// <summary>
/// Read-only projection of an exam for list screens. Mapped to the <c>vw_exams</c> view (exams +
/// owner display name + live subject name/public id, excluding soft-deleted rows) so list queries
/// reuse <c>BaseGetQueryHandler</c> without joining the Identity <c>ApplicationUser</c> type. The
/// <see cref="Visibility"/> enum is exposed as plain <c>string</c> (the column is text); totals come
/// straight from the denormalized cache columns. Never written through — mutations go via
/// <c>IExamRepository</c>.
/// </summary>
public sealed class ExamView : BaseEntity, IHasPublicId
{
    public Guid PublicId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Visibility { get; set; } = string.Empty;
    public int Version { get; set; }
    public decimal TotalPoint { get; set; }
    public int TotalQuestions { get; set; }
    public DateTime UpdatedAt { get; set; }

    public long? SubjectId { get; set; }
    public Guid? SubjectPublicId { get; set; }
    public string? SubjectName { get; set; }

    public long TeacherId { get; set; }
    public Guid TeacherPublicId { get; set; }
    public string TeacherName { get; set; } = string.Empty;
}
