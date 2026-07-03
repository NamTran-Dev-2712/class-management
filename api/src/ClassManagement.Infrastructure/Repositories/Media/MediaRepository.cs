using ClassManagement.Application.Modules.Media.DTOs;
using ClassManagement.Infrastructure.Persistence.DbContext;

// Media data-access over the shared scoped DbContext (MVP-9). Mutations only STAGE (except HardDeleteAsync,
// which is a direct SQL delete used by the cleanup job). Reads return tracked entities for confirm/delete.
public sealed class MediaRepository : IMediaRepository
{
    private readonly ApplicationDbContext _context;

    public MediaRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public Task<MediaAsset?> GetByPublicIdAsync(
        Guid publicId,
        CancellationToken cancellationToken = default
    ) => _context.MediaAssets.FirstOrDefaultAsync(m => m.PublicId == publicId, cancellationToken);

    public async Task AddAsync(MediaAsset entity, CancellationToken cancellationToken = default) =>
        await _context.MediaAssets.AddAsync(entity, cancellationToken);

    public void Update(MediaAsset entity) => _context.MediaAssets.Update(entity);

    public void Remove(MediaAsset entity) => _context.MediaAssets.Remove(entity);

    public Task<long> SumBytesByOwnerAsync(
        long ownerId,
        CancellationToken cancellationToken = default
    ) =>
        _context
            .MediaAssets.Where(m => m.OwnerId == ownerId)
            .SumAsync(m => m.ByteSize, cancellationToken);

    public async Task<IReadOnlyList<MediaAsset>> GetConfirmedOwnedAsync(
        IEnumerable<Guid> publicIds,
        long ownerId,
        CancellationToken cancellationToken = default
    )
    {
        var ids = publicIds.Distinct().ToList();
        if (ids.Count == 0)
            return [];

        return await _context
            .MediaAssets.Where(m =>
                m.OwnerId == ownerId
                && m.Status == MediaStatus.Confirmed
                && ids.Contains(m.PublicId)
            )
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<MediaAsset>> GetExpiredPendingAsync(
        DateTime cutoff,
        int take,
        CancellationToken cancellationToken = default
    ) =>
        await _context
            .MediaAssets.Where(m => m.Status == MediaStatus.Pending && m.CreatedAt < cutoff)
            .OrderBy(m => m.CreatedAt)
            .Take(take)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<MediaAsset>> GetSoftDeletedUnpinnedAsync(
        int take,
        CancellationToken cancellationToken = default
    )
    {
        var pinned = _context.Set<SnapshotMedia>();
        return await _context
            .MediaAssets.IgnoreQueryFilters()
            .Where(m => m.DeletedAt != null && !pinned.Any(p => p.MediaPublicId == m.PublicId))
            .OrderBy(m => m.DeletedAt)
            .Take(take)
            .ToListAsync(cancellationToken);
    }

    public Task HardDeleteAsync(
        IEnumerable<long> internalIds,
        CancellationToken cancellationToken = default
    )
    {
        var ids = internalIds.Distinct().ToList();
        if (ids.Count == 0)
            return Task.CompletedTask;

        return _context
            .MediaAssets.IgnoreQueryFilters()
            .Where(m => ids.Contains(m.Id))
            .ExecuteDeleteAsync(cancellationToken);
    }

    public async Task<(IReadOnlyList<StorageOverviewDto> Rows, int Total)> GetStorageOverviewAsync(
        int skip,
        int take,
        CancellationToken cancellationToken = default
    )
    {
        var grouped = _context
            .Set<MediaView>()
            .GroupBy(m => new { m.OwnerPublicId, m.OwnerName })
            .Select(g => new
            {
                g.Key.OwnerPublicId,
                g.Key.OwnerName,
                TotalBytes = g.Sum(x => x.ByteSize),
                AssetCount = g.Count(),
            });

        var total = await grouped.CountAsync(cancellationToken);
        var rows = await grouped
            .OrderByDescending(x => x.TotalBytes)
            .Skip(skip)
            .Take(take)
            .Select(x => new StorageOverviewDto(
                x.OwnerPublicId,
                x.OwnerName,
                x.TotalBytes,
                x.AssetCount
            ))
            .ToListAsync(cancellationToken);

        return (rows, total);
    }
}
