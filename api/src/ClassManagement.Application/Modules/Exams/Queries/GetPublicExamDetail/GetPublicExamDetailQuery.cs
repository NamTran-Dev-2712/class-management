using ClassManagement.Application.Modules.Exams.DTOs;

// Full detail of any Public exam (or one the caller owns) — read-only reference before duplicating
// (T4-09/10). PublicId from the route.
public record GetPublicExamDetailQuery(Guid PublicId) : IRequest<ExamDetailDto>;
