using ClassManagement.Application.Modules.Exams.DTOs;

// A teacher's own exam bank (BR-4-04 owner scope), filterable by subject/visibility and searchable by
// title keyword.
public record GetTeacherExamsQuery : ExamFilterQuery, IRequest<PaginatedResult<ExamListDto>>;
