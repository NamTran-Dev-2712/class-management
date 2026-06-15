using ClassManagement.Application.Modules.Classroom.DTOs;

// Student views the approved roster of a class they are an approved member of (S2-04).
public record GetStudentClassMembersQuery
    : BaseFilterQuery,
        IRequest<PaginatedResult<ClassMemberDto>>
{
    public Guid ClassPublicId { get; init; }
}
