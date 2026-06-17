using ClassManagement.Domain.Modules.Exams.Enums;

// Teacher flips one of their exams Public/Private. PublicId is bound from the route. Visibility is not
// part of the versioned content, so this does not bump the exam version.
public record SetExamVisibilityCommand(Guid PublicId, ExamVisibility Visibility) : IRequest;
