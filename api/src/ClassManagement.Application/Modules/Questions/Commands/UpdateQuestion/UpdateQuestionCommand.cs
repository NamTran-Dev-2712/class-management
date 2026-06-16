using ClassManagement.Application.Modules.Questions.DTOs;
using ClassManagement.Domain.Modules.Questions.Enums;

// Teacher edits one of their own questions. PublicId is bound from the route. Options/Tags replace
// the existing sets wholesale. Editing is allowed even while the question is used in an Exam — the
// Exam snapshot (MVP-4/5) protects past attempts.
public record UpdateQuestionCommand(
    Guid PublicId,
    Guid? SubjectId,
    QuestionType Type,
    string Content,
    QuestionDifficulty Difficulty,
    decimal SuggestedPoint,
    QuestionVisibility Visibility,
    string? Explanation,
    List<QuestionOptionInput>? Options,
    List<string>? Tags
) : IRequest, IQuestionWriteCommand;
