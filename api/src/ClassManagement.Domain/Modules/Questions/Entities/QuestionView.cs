namespace ClassManagement.Domain.Modules.Questions.Entities;

/// <summary>
/// Read-only projection of a question for list screens. Mapped to the <c>vw_questions</c> view
/// (questions + owner display name + live subject name/public id + option count + tag array,
/// excluding soft-deleted rows) so list queries reuse <c>BaseGetQueryHandler</c> without joining the
/// Identity <c>ApplicationUser</c> type. Status enums are exposed as plain <c>string</c> (the columns
/// are text). Never written through — mutations go via <c>IQuestionRepository</c>.
/// </summary>
public sealed class QuestionView : BaseEntity, IHasPublicId
{
    public Guid PublicId { get; set; }
    public string Type { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string Difficulty { get; set; } = string.Empty;
    public decimal SuggestedPoint { get; set; }
    public string Visibility { get; set; } = string.Empty;
    public string? Explanation { get; set; }
    public DateTime UpdatedAt { get; set; }

    public long? SubjectId { get; set; }
    public Guid? SubjectPublicId { get; set; }
    public string? SubjectName { get; set; }

    public long TeacherId { get; set; }
    public Guid TeacherPublicId { get; set; }
    public string TeacherName { get; set; } = string.Empty;

    public int OptionCount { get; set; }
    public List<string> Tags { get; set; } = [];
}
