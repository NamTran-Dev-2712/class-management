using ClassManagement.Application.Modules.Classroom.DTOs;
using ClassManagement.Domain.Modules.Classroom.Enums;

// Admin read-only view of all classes in the system (A2-01), filterable by status.
public record GetAdminClassesQuery : BaseFilterQuery, IRequest<PaginatedResult<ClassListDto>>
{
    public ClassStatus? Status { get; init; }
}
