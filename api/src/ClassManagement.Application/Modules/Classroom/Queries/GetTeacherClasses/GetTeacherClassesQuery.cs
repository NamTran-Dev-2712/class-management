using ClassManagement.Application.Modules.Classroom.DTOs;
using ClassManagement.Domain.Modules.Classroom.Enums;

// Teacher's own classes (Active + Archived), filterable by status.
public record GetTeacherClassesQuery : BaseFilterQuery, IRequest<PaginatedResult<ClassListDto>>
{
    public ClassStatus? Status { get; init; }
}
