using ClassManagement.Application.Modules.Classroom.DTOs;

// Shared search → count → sort → project → page pipeline for member-list views. Member lists need a
// per-request access guard (owner / approved member), so they use this instead of BaseGetQueryHandler.
internal static class ClassMemberPaging
{
    public static async Task<PaginatedResult<ClassMemberDto>> RunAsync(
        IGenericRepository<ClassMemberView> repo,
        IQueryable<ClassMemberView> query,
        BaseFilterQuery request,
        CancellationToken ct
    )
    {
        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var keyword = request.SearchTerm.Trim().ToLower();
            query = query.Where(m =>
                m.StudentName.ToLower().Contains(keyword)
                || m.StudentEmail.ToLower().Contains(keyword)
            );
        }

        var total = await repo.CountAsync(query, ct);

        var pageQuery = query
            .OrderByDescending(m => m.JoinedAt)
            .Select(ClassProjections.ToMemberDto)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize);

        var items = await repo.ToListAsync(pageQuery, ct);
        return new PaginatedResult<ClassMemberDto>(
            items,
            total,
            request.PageNumber,
            request.PageSize
        );
    }
}
