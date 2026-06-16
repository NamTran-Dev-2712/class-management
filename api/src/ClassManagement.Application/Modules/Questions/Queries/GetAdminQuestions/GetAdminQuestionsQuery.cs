using ClassManagement.Application.Modules.Questions.DTOs;

// Admin read-only view of every question (including Private), per the permission matrix.
public record GetAdminQuestionsQuery
    : QuestionFilterQuery,
        IRequest<PaginatedResult<QuestionListDto>>;
