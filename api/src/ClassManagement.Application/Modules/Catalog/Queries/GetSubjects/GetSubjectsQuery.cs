public record GetSubjectsQuery : BaseFilterQuery, IRequest<PaginatedResult<SubjectDto>>
{
    public bool? IsActive { get; init; }
}
