using System.Linq.Expressions;
using ClassManagement.Application.Modules.Assignments.DTOs;

// Shared search → count → sort → project → page pipeline for attempt-list views (teacher roster +
// student history). Both need a per-request access guard, so they use this instead of
// BaseGetQueryHandler.
internal static class AttemptPaging
{
    public static readonly Expression<Func<AttemptView, AttemptListDto>> ToDto =
        a => new AttemptListDto
        {
            PublicId = a.PublicId,
            Status = a.Status,
            AttemptNumber = a.AttemptNumber,
            StartedAt = a.StartedAt,
            SubmittedAt = a.SubmittedAt,
            DeadlineAt = a.DeadlineAt,
            AutoSubmitted = a.AutoSubmitted,
            TotalAutoScore = a.TotalAutoScore,
            TotalManualScore = a.TotalManualScore,
            TotalScore = a.TotalScore,
            TotalPoint = a.TotalPoint,
            AssignmentPublicId = a.AssignmentPublicId,
            AssignmentTitle = a.AssignmentTitle,
            StudentPublicId = a.StudentPublicId,
            StudentName = a.StudentName,
            CreatedAt = a.CreatedAt,
        };

    public static async Task<PaginatedResult<AttemptListDto>> RunAsync(
        IGenericRepository<AttemptView> repo,
        IQueryable<AttemptView> query,
        BaseFilterQuery request,
        CancellationToken ct
    )
    {
        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var keyword = request.SearchTerm.Trim().ToLower();
            query = query.Where(a => a.StudentName.ToLower().Contains(keyword));
        }

        var total = await repo.CountAsync(query, ct);

        var pageQuery = query
            .OrderByDescending(a => a.StartedAt)
            .Select(ToDto)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize);

        var items = await repo.ToListAsync(pageQuery, ct);
        return new PaginatedResult<AttemptListDto>(
            items,
            total,
            request.PageNumber,
            request.PageSize
        );
    }
}
