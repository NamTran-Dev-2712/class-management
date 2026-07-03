using Microsoft.Extensions.Logging;

// Deletes the physical storage object for each orphaned asset, then hard-removes its row. A soft-deleted
// asset still pinned by a snapshot is excluded by the repository query, so old attempts keep working
// (BR-9-06). Storage deletion failures are logged and left for the next run (the row is not removed), so
// we never orphan the object.
public sealed class RunMediaCleanupCommandHandler : IRequestHandler<RunMediaCleanupCommand, int>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IStorageProviderResolver _storage;
    private readonly ILogger<RunMediaCleanupCommandHandler> _logger;

    public RunMediaCleanupCommandHandler(
        IUnitOfWork unitOfWork,
        IStorageProviderResolver storage,
        ILogger<RunMediaCleanupCommandHandler> logger
    )
    {
        _unitOfWork = unitOfWork;
        _storage = storage;
        _logger = logger;
    }

    public async Task<int> Handle(RunMediaCleanupCommand request, CancellationToken ct)
    {
        var cutoff = DateTime.UtcNow.AddMinutes(-request.PendingConfirmTtlMinutes);

        var expiredPending = await _unitOfWork.Media.GetExpiredPendingAsync(
            cutoff,
            request.BatchSize,
            ct
        );
        var softDeletedUnpinned = await _unitOfWork.Media.GetSoftDeletedUnpinnedAsync(
            request.BatchSize,
            ct
        );

        var candidates = expiredPending.Concat(softDeletedUnpinned).ToList();
        if (candidates.Count == 0)
            return 0;

        var deletedIds = new List<long>();
        foreach (var asset in candidates)
        {
            try
            {
                await _storage.Resolve(asset.Provider).DeleteAsync(asset.StorageKey, ct);
                deletedIds.Add(asset.Id);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(
                    ex,
                    "Media cleanup: failed to delete storage object for asset {AssetId}; retrying next run",
                    asset.Id
                );
            }
        }

        if (deletedIds.Count > 0)
            await _unitOfWork.Media.HardDeleteAsync(deletedIds, ct);

        _logger.LogInformation("Media cleanup removed {Count} orphaned asset(s)", deletedIds.Count);
        return deletedIds.Count;
    }
}
