using ClassManagement.Domain.Modules.Questions.Enums;

namespace ClassManagement.Domain.Modules.Questions.Entities;

/// <summary>
/// A single reusable question in a teacher's question bank (MVP-3). Owns its options and tags as
/// child collections (replaced wholesale on update). <see cref="SubjectId"/> is required at create
/// time but the column is nullable so a subject deletion sets it null rather than deleting the
/// question. <see cref="Content"/> holds Markdown (rendered safely on the client).
/// </summary>
public sealed class Question : AuditableEntity, IHasPublicId, ISoftDeletable
{
    public Guid PublicId { get; set; }
    public long TeacherId { get; set; }
    public long? SubjectId { get; set; }
    public QuestionType Type { get; set; }
    public string Content { get; set; } = string.Empty;
    public QuestionDifficulty Difficulty { get; set; } = QuestionDifficulty.Medium;
    public decimal SuggestedPoint { get; set; } = 1.00m;
    public QuestionVisibility Visibility { get; set; } = QuestionVisibility.Private;
    public string? Explanation { get; set; }
    public DateTime? DeletedAt { get; set; }

    public ICollection<QuestionOption> Options { get; set; } = [];
    public ICollection<QuestionTag> Tags { get; set; } = [];
}
