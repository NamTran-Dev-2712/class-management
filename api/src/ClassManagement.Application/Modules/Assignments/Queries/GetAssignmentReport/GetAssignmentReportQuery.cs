using ClassManagement.Application.Modules.Assignments.DTOs;

// Teacher/admin aggregate report for one assignment (T6-08). OwnerScoped = true enforces that the caller
// owns the assignment (teacher surface); the admin surface passes false (already gated by role).
public sealed record GetAssignmentReportQuery(Guid PublicId, bool OwnerScoped = true)
    : IRequest<AssignmentReportDto>;
