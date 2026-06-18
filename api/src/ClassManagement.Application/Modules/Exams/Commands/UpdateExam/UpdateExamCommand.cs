using ClassManagement.Domain.Modules.Exams.Enums;

// Teacher edits an exam's metadata (title/description/subject/visibility). PublicId is bound from the
// route. This does NOT change the question list, so it does NOT bump the exam version.
public record UpdateExamCommand(
    Guid PublicId,
    Guid? SubjectId,
    string Title,
    string? Description,
    ExamVisibility Visibility,
    List<string>? Tags
) : IRequest, IExamWriteCommand;
