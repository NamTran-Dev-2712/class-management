using ClassManagement.Application.Modules.Exams.DTOs;

// Full detail of one of the calling teacher's own exams (edit screen). PublicId from the route.
public record GetExamDetailQuery(Guid PublicId) : IRequest<ExamDetailDto>;
