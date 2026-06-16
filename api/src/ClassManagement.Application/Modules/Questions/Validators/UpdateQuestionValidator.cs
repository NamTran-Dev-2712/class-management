public sealed class UpdateQuestionValidator : QuestionWriteValidator<UpdateQuestionCommand>
{
    public UpdateQuestionValidator()
    {
        RuleFor(x => x.PublicId).NotEmpty().WithMessage("Validation.Question.NotFound");
    }
}
