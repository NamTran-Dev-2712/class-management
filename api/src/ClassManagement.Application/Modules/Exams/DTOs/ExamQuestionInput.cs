namespace ClassManagement.Application.Modules.Exams.DTOs;

/// <summary>
/// One entry in the ordered question list the teacher saves for an exam. <c>QuestionId</c> is a
/// question's PublicId; <c>Point</c> overrides its suggested point. The handler assigns the 1-based
/// display order from list position, so the client controls ordering purely by array order.
/// </summary>
public sealed record ExamQuestionInput(Guid QuestionId, decimal Point);
