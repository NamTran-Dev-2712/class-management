// Resolves a teacher's EFFECTIVE resource caps (MVP-8). A teacher with an Active subscription gets that
// plan's limits (a null plan limit = unlimited = 0); otherwise (Free / no active sub) it falls back to
// the live system_settings caps via the existing per-resource policies — preserving MVP-7 behavior where
// an admin tunes Free caps at runtime. 0 always means unlimited.
public interface IResourceLimitService
{
    Task<EffectiveResourceLimits> GetEffectiveLimitsAsync(
        long teacherId,
        CancellationToken cancellationToken = default
    );

    Task<int> GetMaxClassesAsync(long teacherId, CancellationToken cancellationToken = default);
    Task<int> GetMaxQuestionsAsync(long teacherId, CancellationToken cancellationToken = default);
    Task<int> GetMaxExamsAsync(long teacherId, CancellationToken cancellationToken = default);

    /// <summary>Total media storage cap in bytes for a teacher; <c>0</c> = unlimited (MVP-9).</summary>
    Task<long> GetMaxStorageBytesAsync(
        long teacherId,
        CancellationToken cancellationToken = default
    );
}

/// <summary>The caps in force for a teacher right now. 0 = unlimited. <c>IsPro</c> drives upgrade UI.</summary>
public sealed record EffectiveResourceLimits(
    string PlanName,
    bool IsPro,
    int MaxClasses,
    int MaxQuestions,
    int MaxExams,
    long MaxStorageBytes
);
