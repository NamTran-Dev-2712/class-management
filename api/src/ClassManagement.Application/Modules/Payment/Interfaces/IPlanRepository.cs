// Thin data-access for the Plan aggregate (MVP-8). Stages writes only — the handler commits via
// IUnitOfWork.SaveChangesAsync(). Business rules live in the handlers.
public interface IPlanRepository
{
    Task<Plan?> GetByIdAsync(long id, CancellationToken cancellationToken = default);
    Task<Plan?> GetByPublicIdAsync(Guid publicId, CancellationToken cancellationToken = default);

    // Active plans ordered by display_order, for the public pricing page.
    Task<List<Plan>> GetActiveOrderedAsync(CancellationToken cancellationToken = default);

    void Update(Plan plan);
}
