using ClassManagement.Application.Modules.Questions.DTOs;

// Full detail of one of the calling teacher's own questions (edit/preview). PublicId from the route.
public record GetQuestionDetailQuery(Guid PublicId) : IRequest<QuestionDetailDto>;
