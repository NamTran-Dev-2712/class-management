using ClassManagement.Domain.Modules.Media.Enums;

namespace ClassManagement.Domain.Modules.Questions.Entities;

/// <summary>
/// Links a <see cref="MediaAsset"/> to a <see cref="Question"/> as a first-class attachment (MVP-9).
/// Append-only child of <see cref="Question"/> (cascade-deleted with its parent); on edit the parent
/// replaces the whole set rather than mutating rows, so no UpdatedAt is needed — same pattern as
/// <see cref="QuestionOption"/> / <see cref="QuestionTag"/>. The linked asset is RESTRICT so a media row
/// referenced here cannot be hard-deleted out from under it (soft-delete + cleanup handles removal).
/// </summary>
public sealed class QuestionMedia : BaseEntity
{
    public long QuestionId { get; set; }
    public long MediaId { get; set; }
    public MediaRole Role { get; set; } = MediaRole.Attachment;
    public int DisplayOrder { get; set; }
}
