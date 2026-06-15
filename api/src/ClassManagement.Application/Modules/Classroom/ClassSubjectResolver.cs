using ClassManagement.Application.Exceptions;

// Resolves the optional subject for a class create/update. A teacher may pick a catalog subject
// (must be active — BR-2-01) or type a free-text subject; the resulting SubjectName is a snapshot.
internal static class ClassSubjectResolver
{
    public static async Task<(long? SubjectId, string? SubjectName)> ResolveAsync(
        IUnitOfWork unitOfWork,
        Guid? subjectPublicId,
        string? subjectName,
        CancellationToken ct
    )
    {
        if (subjectPublicId.HasValue)
        {
            var subject =
                await unitOfWork.Subjects.GetByPublicIdAsync(subjectPublicId.Value, ct)
                ?? throw new NotFoundException("Subject.NotFound");

            if (!subject.IsActive)
                throw new BadException("Class.SubjectInactive");

            return (subject.Id, subject.Name);
        }

        var trimmed = subjectName?.Trim();
        return string.IsNullOrEmpty(trimmed) ? (null, null) : (null, trimmed);
    }
}
