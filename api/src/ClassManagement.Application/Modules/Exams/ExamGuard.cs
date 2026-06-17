using ClassManagement.Application.Exceptions;
using ClassManagement.Domain.Modules.Exams.Enums;

// Ownership/visibility checks shared by exam command/query handlers. A teacher may only manage exams
// they own (BR-4-04); a public exam (or one they own) may be read/duplicated by anyone.
internal static class ExamGuard
{
    public static Exam EnsureOwned(Exam? exam, long? currentUserId)
    {
        if (exam is null)
            throw new NotFoundException("Exam.NotFound");

        if (currentUserId is null || exam.TeacherId != currentUserId.Value)
            throw new ForbiddenException("Exam.NotOwner");

        return exam;
    }

    // Readable when the exam is Public OR owned by the caller; otherwise hidden (404 — don't leak the
    // existence of someone else's private exam).
    public static Exam EnsureReadable(Exam? exam, long? currentUserId)
    {
        if (exam is null)
            throw new NotFoundException("Exam.NotFound");

        if (exam.Visibility != ExamVisibility.Public && exam.TeacherId != currentUserId)
            throw new NotFoundException("Exam.NotFound");

        return exam;
    }
}
