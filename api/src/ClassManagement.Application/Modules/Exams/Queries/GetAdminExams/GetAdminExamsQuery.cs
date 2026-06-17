using ClassManagement.Application.Modules.Exams.DTOs;

// Admin read-only view of every exam (including Private), per the permission matrix.
public record GetAdminExamsQuery : ExamFilterQuery, IRequest<PaginatedResult<ExamListDto>>;
