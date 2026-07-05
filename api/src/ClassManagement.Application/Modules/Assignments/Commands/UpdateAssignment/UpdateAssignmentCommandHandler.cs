using ClassManagement.Application.Exceptions;
using ClassManagement.Domain.Modules.Assignments.Enums;

public class UpdateAssignmentCommandHandler : IRequestHandler<UpdateAssignmentCommand>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;

    public UpdateAssignmentCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUser)
    {
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
    }

    public async Task Handle(UpdateAssignmentCommand request, CancellationToken ct)
    {
        var assignment = AssignmentGuard.EnsureOwned(
            await _unitOfWork.Assignments.GetByPublicIdAsync(request.PublicId, false, ct),
            _currentUser.UserId
        );

        // Config is only editable before publish; once published the snapshot is immutable.
        if (assignment.Status != AssignmentStatus.Draft)
            throw new BadException("Assignment.NotDraft");

        assignment.Title = request.Title.Trim();
        assignment.Description = string.IsNullOrWhiteSpace(request.Description)
            ? null
            : request.Description.Trim();
        assignment.OpensAt = request.OpensAt;
        assignment.ClosesAt = request.ClosesAt;
        assignment.TimeLimitMinutes = request.TimeLimitMinutes;
        assignment.MaxAttempts = request.MaxAttempts;
        assignment.ScorePolicy = request.ScorePolicy;
        assignment.AllowLate = request.AllowLate;
        assignment.GradePublishPolicy = request.GradePublishPolicy;
        assignment.ShuffleQuestions = request.ShuffleQuestions;
        assignment.ShuffleOptions = request.ShuffleOptions;
        assignment.ShowAnswersAfterGrade = request.ShowAnswersAfterGrade;
        assignment.RequireFullscreen = request.RequireFullscreen;
        assignment.DetectTabSwitch = request.DetectTabSwitch;
        assignment.BlockCopyPaste = request.BlockCopyPaste;
        assignment.MaxViolations = request.MaxViolations;
        assignment.ViolationAction = request.ViolationAction;

        _unitOfWork.Assignments.Update(assignment);
        await _unitOfWork.SaveChangesAsync(ct);
    }
}
