using ClassManagement.Domain.Modules.Exams.Enums;

// Teacher creates an exam template. SubjectId = a catalog subject's PublicId (optional; when provided
// it must be active). The exam starts empty (questions are saved separately); BR-4-02's "at least one
// question" rule is enforced only when publishing to an Assignment (MVP-5). Returns the new PublicId.
public record CreateExamCommand(
    Guid? SubjectId,
    string Title,
    string? Description,
    ExamVisibility Visibility
) : IRequest<Guid>, IExamWriteCommand;
