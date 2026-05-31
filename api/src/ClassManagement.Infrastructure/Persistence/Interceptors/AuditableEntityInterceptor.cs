using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using EfDbContext = Microsoft.EntityFrameworkCore.DbContext;

namespace ClassManagement.Infrastructure.Persistence.Interceptors;

// Registered as Singleton — inject IHttpContextAccessor (also Singleton), NOT ICurrentUserService (Scoped)
public sealed class AuditableEntityInterceptor : SaveChangesInterceptor
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public AuditableEntityInterceptor(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData,
        InterceptionResult<int> result
    )
    {
        UpdateEntities(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken ct = default
    )
    {
        UpdateEntities(eventData.Context);
        return base.SavingChangesAsync(eventData, result, ct);
    }

    private long? CurrentUserId
    {
        get
        {
            var value = _httpContextAccessor.HttpContext?.User?.FindFirstValue(
                ClaimTypes.NameIdentifier
            );
            return long.TryParse(value, out var id) ? id : null;
        }
    }

    private void UpdateEntities(EfDbContext? context)
    {
        if (context is null)
            return;

        var now = DateTime.UtcNow;
        var userId = CurrentUserId;

        foreach (var entry in context.ChangeTracker.Entries())
        {
            // Soft delete: intercept Delete → Modified + set DeletedAt
            if (entry.Entity is ISoftDeletable softDeletable && entry.State == EntityState.Deleted)
            {
                entry.State = EntityState.Modified;
                softDeletable.DeletedAt = now;
            }

            // Full audit for AuditableEntity (domain) and ApplicationUser (Identity)
            if (entry.Entity is IFullyAuditable auditable)
            {
                switch (entry.State)
                {
                    case EntityState.Added:
                        auditable.CreatedAt = now;
                        auditable.UpdatedAt = now;
                        auditable.CreatedBy = userId;
                        auditable.UpdatedBy = userId;
                        break;
                    case EntityState.Modified:
                        auditable.UpdatedAt = now;
                        auditable.UpdatedBy = userId;
                        break;
                }
            }
            // Append-only: just set CreatedAt if not already set
            else if (
                entry.Entity is BaseEntity baseEntity
                && entry.State == EntityState.Added
                && baseEntity.CreatedAt == default
            )
            {
                baseEntity.CreatedAt = now;
            }
        }
    }
}
