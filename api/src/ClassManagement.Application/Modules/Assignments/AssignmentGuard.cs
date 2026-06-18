using ClassManagement.Application.Exceptions;

// Ownership/access checks shared by assignment + attempt handlers (MVP-5 permission matrix). A teacher
// may only manage assignments they own; a student may only act on their own attempts.
internal static class AssignmentGuard
{
    public static Assignment EnsureOwned(Assignment? assignment, long? currentUserId)
    {
        if (assignment is null)
            throw new NotFoundException("Assignment.NotFound");

        if (currentUserId is null || assignment.TeacherId != currentUserId.Value)
            throw new ForbiddenException("Assignment.NotOwner");

        return assignment;
    }

    public static Attempt EnsureAttemptOwner(Attempt? attempt, long? currentUserId)
    {
        if (attempt is null)
            throw new NotFoundException("Attempt.NotFound");

        if (currentUserId is null || attempt.StudentId != currentUserId.Value)
            throw new ForbiddenException("Attempt.NotOwner");

        return attempt;
    }
}
