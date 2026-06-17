using ClassManagement.Application.Modules.Exams.DTOs;

// Validates the saved question list (BR-4-03: each point > 0; no duplicate question in one exam). The
// list may be empty (clearing the exam); the "at least one question" rule (BR-4-02) is enforced only
// at publish time in MVP-5. A soft cap guards against pathological sizes (Risk: too many questions).
public sealed class UpdateExamQuestionsValidator : AbstractValidator<UpdateExamQuestionsCommand>
{
    private const int MaxQuestionsPerExam = 500;

    public UpdateExamQuestionsValidator()
    {
        RuleFor(x => x.Questions)
            .NotNull()
            .WithMessage("Validation.Exam.QuestionsNull")
            .Must(q => q.Count <= MaxQuestionsPerExam)
            .WithMessage("Validation.Exam.TooManyQuestions")
            .Must(HaveNoDuplicates)
            .WithMessage("Validation.Exam.DuplicateQuestion");

        RuleForEach(x => x.Questions)
            .ChildRules(item =>
            {
                item.RuleFor(q => q.Point)
                    .GreaterThan(0)
                    .WithMessage("Validation.Exam.PointInvalid")
                    .LessThanOrEqualTo(100)
                    .WithMessage("Validation.Exam.PointInvalid");
            });
    }

    private static bool HaveNoDuplicates(List<ExamQuestionInput> questions) =>
        questions.Select(q => q.QuestionId).Distinct().Count() == questions.Count;
}
