using ClassManagement.Application.Modules.Questions.DTOs;
using ClassManagement.Domain.Modules.Questions.Enums;

// Teacher creates a question in their bank. SubjectId = a catalog subject's PublicId (optional; when
// provided it must be active). Options apply to choice types only; Tags are free-form (normalized).
// Returns the new question's PublicId.
public record CreateQuestionCommand(
    Guid? SubjectId,
    QuestionType Type,
    string Content,
    QuestionDifficulty Difficulty,
    decimal SuggestedPoint,
    QuestionVisibility Visibility,
    string? Explanation,
    List<QuestionOptionInput>? Options,
    List<string>? Tags,
    List<QuestionMediaInput>? Attachments = null
) : IRequest<Guid>, IQuestionWriteCommand;
