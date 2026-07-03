namespace ClassManagement.Application.Modules.Questions.DTOs;

/// <summary>
/// An answer choice supplied by the client when creating/updating a choice-type question. The
/// handler assigns <c>DisplayOrder</c> from list position. For TrueFalse the server seeds the two
/// fixed options, so the client sends only the two with its chosen correct flag.
/// </summary>
public sealed record QuestionOptionInput(
    string Content,
    bool IsCorrect,
    Guid? MediaPublicId = null
);
