public sealed class CreateAssignmentValidator : AssignmentWriteValidator<CreateAssignmentCommand>
{
    public CreateAssignmentValidator(IAssignmentPolicy policy)
        : base(policy)
    {
        RuleFor(x => x.ExamId).NotEmpty().WithMessage("Validation.Assignment.ExamRequired");
        RuleFor(x => x.ClassId).NotEmpty().WithMessage("Validation.Assignment.ClassRequired");
    }
}
