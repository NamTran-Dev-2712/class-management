using ClassManagement.Application.Modules.Classroom.DTOs;
using ClassManagement.Domain.Modules.Classroom.Enums;

// Teacher views the roster/pending list of an owned class, optionally filtered by membership status.
public record GetClassMembersQuery : BaseFilterQuery, IRequest<PaginatedResult<ClassMemberDto>>
{
    public Guid ClassPublicId { get; init; }
    public MembershipStatus? Status { get; init; }
}
