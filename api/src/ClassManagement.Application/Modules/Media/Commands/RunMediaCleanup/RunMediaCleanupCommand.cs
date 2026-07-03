// Sweeps orphaned media (MVP-9, BR-9-08): Pending assets never confirmed past the TTL, and soft-deleted
// assets no longer pinned by any published snapshot. Idempotent — a missed run is caught next tick. The
// tunables are passed in by the thin Hangfire trigger (appsettings), so Application stays Options-free.
// Returns the number of assets whose storage object + row were removed.
public sealed record RunMediaCleanupCommand(int PendingConfirmTtlMinutes = 60, int BatchSize = 200)
    : IRequest<int>;
