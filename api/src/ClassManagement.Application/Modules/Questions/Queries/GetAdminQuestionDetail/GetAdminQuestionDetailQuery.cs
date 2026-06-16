using ClassManagement.Application.Modules.Questions.DTOs;

// Full detail of any question for an admin (including Private). PublicId from the route.
public record GetAdminQuestionDetailQuery(Guid PublicId) : IRequest<QuestionDetailDto>;
