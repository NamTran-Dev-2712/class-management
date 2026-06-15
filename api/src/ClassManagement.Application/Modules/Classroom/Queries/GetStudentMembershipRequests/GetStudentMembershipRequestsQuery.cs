using ClassManagement.Application.Modules.Classroom.DTOs;
using ClassManagement.Domain.Modules.Classroom.Enums;

// A student's join requests and their statuses (S2-02), optionally filtered by status.
public record GetStudentMembershipRequestsQuery
    : BaseFilterQuery,
        IRequest<PaginatedResult<MembershipRequestDto>>
{
    public MembershipStatus? Status { get; init; }
}
