using ClassManagement.Application.Exceptions;

// Resolves a set of media public ids referenced by a question (option images + attachments) to internal
// ids, enforcing that every one is a Confirmed asset owned by the teacher (BR-9-01/07). Throws if any
// referenced media is missing, not owned, or still Pending.
internal static class QuestionMediaResolver
{
    public static async Task<IReadOnlyDictionary<Guid, long>> ResolveAsync(
        IUnitOfWork unitOfWork,
        long teacherId,
        IReadOnlyList<Guid> publicIds,
        CancellationToken ct
    )
    {
        if (publicIds.Count == 0)
            return new Dictionary<Guid, long>();

        var assets = await unitOfWork.Media.GetConfirmedOwnedAsync(publicIds, teacherId, ct);
        var map = assets.ToDictionary(a => a.PublicId, a => a.Id);

        if (map.Count != publicIds.Count)
            throw new BadException("Media.InvalidReference");

        return map;
    }
}
