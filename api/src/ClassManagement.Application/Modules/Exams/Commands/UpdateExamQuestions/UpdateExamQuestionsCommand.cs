using ClassManagement.Application.Modules.Exams.DTOs;

// Teacher saves the full ordered question list of an exam (add/remove/reorder/repoint in one atomic
// save). PublicId is bound from the route. The list replaces exam_questions wholesale; display order
// is taken from list position. Recomputes total point/questions and bumps the exam version by one.
public record UpdateExamQuestionsCommand(Guid PublicId, List<ExamQuestionInput> Questions)
    : IRequest;
