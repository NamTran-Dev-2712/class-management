using ClassManagement.Application.Modules.Exams.DTOs;

// The public exam pool — every Public exam from any teacher (read-only browse/reference, T4-09/10).
// Same data for everyone → Shared output cache.
public record GetPublicExamsQuery : ExamFilterQuery, IRequest<PaginatedResult<ExamListDto>>;
