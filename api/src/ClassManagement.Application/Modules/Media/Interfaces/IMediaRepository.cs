using ClassManagement.Application.Modules.Media.DTOs;

// Media data-access over the shared scoped DbContext (MVP-9). Mutation methods only STAGE — the handler
// commits through IUnitOfWork.SaveChangesAsync(). Reads return tracked entities for confirm/delete.
public interface IMediaRepository
{
    Task<MediaAsset?> GetByPublicIdAsync(
        Guid publicId,
        CancellationToken cancellationToken = default
    );

    Task AddAsync(MediaAsset entity, CancellationToken cancellationToken = default);
    void Update(MediaAsset entity);
    void Remove(MediaAsset entity);

    /// <summary>Total bytes owned by a teacher (Pending + Confirmed, soft-deleted excluded) — for quota.</summary>
    Task<long> SumBytesByOwnerAsync(long ownerId, CancellationToken cancellationToken = default);

    /// <summary>Confirmed assets owned by <paramref name="ownerId"/> among the given public ids — validates
    /// question media references belong to the owner and are usable (BR-9-01/07).</summary>
    Task<IReadOnlyList<MediaAsset>> GetConfirmedOwnedAsync(
        IEnumerable<Guid> publicIds,
        long ownerId,
        CancellationToken cancellationToken = default
    );

    /// <summary>Pending assets created before <paramref name="cutoff"/> (never confirmed) — cleanup sweep.</summary>
    Task<IReadOnlyList<MediaAsset>> GetExpiredPendingAsync(
        DateTime cutoff,
        int take,
        CancellationToken cancellationToken = default
    );

    /// <summary>Soft-deleted assets NOT pinned by any snapshot — safe to remove physically (BR-9-08).</summary>
    Task<IReadOnlyList<MediaAsset>> GetSoftDeletedUnpinnedAsync(
        int take,
        CancellationToken cancellationToken = default
    );

    /// <summary>Physically delete asset rows by internal id (cleanup only, after their storage objects are
    /// removed). Bypasses the soft-delete interceptor + query filter; commits immediately.</summary>
    Task HardDeleteAsync(
        IEnumerable<long> internalIds,
        CancellationToken cancellationToken = default
    );

    /// <summary>Per-teacher storage aggregate for the admin overview, paged by usage descending.</summary>
    Task<(IReadOnlyList<StorageOverviewDto> Rows, int Total)> GetStorageOverviewAsync(
        int skip,
        int take,
        CancellationToken cancellationToken = default
    );
}
