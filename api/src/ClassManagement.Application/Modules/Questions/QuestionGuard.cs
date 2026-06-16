using ClassManagement.Application.Exceptions;
using ClassManagement.Domain.Modules.Questions.Enums;

// Ownership/visibility checks shared by question command/query handlers. A teacher may only manage
// questions they own (BR-3-07); a public question (or one they own) may be read/duplicated by anyone.
internal static class QuestionGuard
{
    public static Question EnsureOwned(Question? question, long? currentUserId)
    {
        if (question is null)
            throw new NotFoundException("Question.NotFound");

        if (currentUserId is null || question.TeacherId != currentUserId.Value)
            throw new ForbiddenException("Question.NotOwner");

        return question;
    }

    // Readable when the question is Public OR owned by the caller; otherwise hidden (404 — don't leak
    // existence of someone else's private question).
    public static Question EnsureReadable(Question? question, long? currentUserId)
    {
        if (question is null)
            throw new NotFoundException("Question.NotFound");

        if (question.Visibility != QuestionVisibility.Public && question.TeacherId != currentUserId)
            throw new NotFoundException("Question.NotFound");

        return question;
    }
}
