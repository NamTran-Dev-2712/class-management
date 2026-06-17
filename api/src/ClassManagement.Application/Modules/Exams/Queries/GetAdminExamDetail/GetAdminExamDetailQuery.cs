using ClassManagement.Application.Modules.Exams.DTOs;

// Full detail of any exam for an admin (including Private). PublicId from the route.
public record GetAdminExamDetailQuery(Guid PublicId) : IRequest<ExamDetailDto>;
