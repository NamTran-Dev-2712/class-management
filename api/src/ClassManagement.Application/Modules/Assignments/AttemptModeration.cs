using ClassManagement.Application.Exceptions;
using ClassManagement.Domain.Modules.Assignments.Entities;

// Shared resolve-and-guard for the teacher/admin attempt moderation commands (MVP-10 force-submit /
// flag / unlock). Loads the attempt (with answers, for a possible finalize) and, when ownerScoped,
// verifies the current user owns the assignment. Admin path (ownerScoped = false) skips the owner check.
internal static class AttemptModeration
{
    public static async Task<Attempt> ResolveAsync(
        IUnitOfWork unitOfWork,
        long? currentUserId,
        Guid attemptPublicId,
        bool ownerScoped,
        CancellationToken ct
    )
    {
        var attempt =
            await unitOfWork.Attempts.GetByPublicIdAsync(attemptPublicId, true, ct)
            ?? throw new NotFoundException("Attempt.NotFound");

        if (ownerScoped)
        {
            var avRepo = unitOfWork.Repository<AssignmentView>();
            var ownerId = (
                await avRepo.ToListAsync(
                    avRepo
                        .Query()
                        .Where(a => a.Id == attempt.AssignmentId)
                        .Select(a => a.TeacherId)
                        .Take(1),
                    ct
                )
            ).FirstOrDefault();

            if (ownerId != currentUserId)
                throw new ForbiddenException("Assignment.NotOwner");
        }

        return attempt;
    }
}
