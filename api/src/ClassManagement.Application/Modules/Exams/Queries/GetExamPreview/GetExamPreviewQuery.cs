using ClassManagement.Application.Modules.Exams.DTOs;

// Exam rendered as a student would see it (T4-08) — owner-only, since it is a pre-publish check of
// the teacher's own exam. PublicId from the route.
public record GetExamPreviewQuery(Guid PublicId) : IRequest<ExamPreviewDto>;
