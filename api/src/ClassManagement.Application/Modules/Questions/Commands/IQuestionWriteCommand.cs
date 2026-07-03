using ClassManagement.Application.Modules.Questions.DTOs;
using ClassManagement.Domain.Modules.Questions.Enums;

// Shared shape of the create/update question commands so a single validator covers both. Records
// with matching positional parameters implement this implicitly.
public interface IQuestionWriteCommand
{
    Guid? SubjectId { get; }
    QuestionType Type { get; }
    string Content { get; }
    QuestionDifficulty Difficulty { get; }
    decimal SuggestedPoint { get; }
    QuestionVisibility Visibility { get; }
    string? Explanation { get; }
    List<QuestionOptionInput>? Options { get; }
    List<string>? Tags { get; }

    // Question-level media attachments (MVP-9).
    List<QuestionMediaInput>? Attachments { get; }
}
