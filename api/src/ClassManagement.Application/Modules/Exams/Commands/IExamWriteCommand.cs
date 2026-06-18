using ClassManagement.Domain.Modules.Exams.Enums;

// Shared shape of the create/update exam metadata commands so a single validator covers both.
// Records with matching positional parameters implement this implicitly.
public interface IExamWriteCommand
{
    Guid? SubjectId { get; }
    string Title { get; }
    string? Description { get; }
    ExamVisibility Visibility { get; }
    List<string>? Tags { get; }
}
