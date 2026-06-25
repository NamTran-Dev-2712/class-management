using System.Linq.Expressions;
using ClassManagement.Domain.Modules.Admin.Enums;

// Resolves a report's polymorphic target (type + public id) to the internal id, or null if it does not
// exist / is not visible. Content lookups honour the global soft-delete filter; users resolve via the
// directory. Shared by submit (validate target) and review (apply hide/delete).
internal static class ReportTargetResolver
{
    public static async Task<long?> ResolveAsync(
        IUnitOfWork unitOfWork,
        IUserDirectory userDirectory,
        ReportTargetType type,
        Guid publicId,
        CancellationToken ct
    )
    {
        return type switch
        {
            ReportTargetType.Question => await FirstIdAsync<Question>(q => q.PublicId == publicId),
            ReportTargetType.Exam => await FirstIdAsync<Exam>(e => e.PublicId == publicId),
            ReportTargetType.Assignment => await FirstIdAsync<Assignment>(a =>
                a.PublicId == publicId
            ),
            ReportTargetType.Class => await FirstIdAsync<Class>(c => c.PublicId == publicId),
            ReportTargetType.User => await userDirectory.GetUserIdByPublicIdAsync(publicId, ct),
            _ => null,
        };

        async Task<long?> FirstIdAsync<TEntity>(
            System.Linq.Expressions.Expression<Func<TEntity, bool>> predicate
        )
            where TEntity : BaseEntity
        {
            var repo = unitOfWork.Repository<TEntity>();
            var rows = await repo.ToListAsync(
                repo.Query().Where(predicate).Select(e => e.Id).Take(1),
                ct
            );
            return rows.Count > 0 ? rows[0] : null;
        }
    }

    // The user a moderation action concerns: the owner of reported content, or the reported user itself.
    public static async Task<long?> ResolveSubjectUserIdAsync(
        IUnitOfWork unitOfWork,
        ReportTargetType type,
        long targetId,
        CancellationToken ct
    )
    {
        return type switch
        {
            ReportTargetType.Question => await OwnerAsync<Question>(targetId, q => q.TeacherId),
            ReportTargetType.Exam => await OwnerAsync<Exam>(targetId, e => e.TeacherId),
            ReportTargetType.Assignment => await OwnerAsync<Assignment>(targetId, a => a.TeacherId),
            ReportTargetType.Class => await OwnerAsync<Class>(targetId, c => c.OwnerId),
            ReportTargetType.User => targetId,
            _ => null,
        };

        async Task<long?> OwnerAsync<TEntity>(
            long id,
            Expression<Func<TEntity, long>> ownerSelector
        )
            where TEntity : BaseEntity
        {
            var repo = unitOfWork.Repository<TEntity>();
            var idParam = ownerSelector.Parameters[0];
            var predicate = Expression.Lambda<Func<TEntity, bool>>(
                Expression.Equal(
                    Expression.Property(idParam, nameof(BaseEntity.Id)),
                    Expression.Constant(id)
                ),
                idParam
            );
            var rows = await repo.ToListAsync(
                repo.Query().Where(predicate).Select(ownerSelector).Take(1),
                ct
            );
            return rows.Count > 0 ? rows[0] : null;
        }
    }
}
