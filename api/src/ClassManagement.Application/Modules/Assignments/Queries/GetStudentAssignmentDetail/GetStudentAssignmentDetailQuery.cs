using ClassManagement.Application.Modules.Assignments.DTOs;

// Student views one assignment's detail (info + their attempt stats) before starting an attempt.
public record GetStudentAssignmentDetailQuery(Guid PublicId) : IRequest<StudentAssignmentDetailDto>;
