public sealed class UpdateAssignmentValidator : AssignmentWriteValidator<UpdateAssignmentCommand>
{
    public UpdateAssignmentValidator(IAssignmentPolicy policy)
        : base(policy) { }
}
