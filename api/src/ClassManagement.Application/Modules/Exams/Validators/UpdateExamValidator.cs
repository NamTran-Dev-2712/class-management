public sealed class UpdateExamValidator : ExamWriteValidator<UpdateExamCommand>
{
    public UpdateExamValidator()
    {
        RuleFor(x => x.PublicId).NotEmpty().WithMessage("Validation.Exam.NotFound");
    }
}
