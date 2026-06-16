using ClassManagement.Application.Modules.Questions.DTOs;

// Full detail of any Public question (or one the caller owns) — read-only reference/preview before
// duplicating (T3-09/10). PublicId from the route.
public record GetPublicQuestionDetailQuery(Guid PublicId) : IRequest<QuestionDetailDto>;
