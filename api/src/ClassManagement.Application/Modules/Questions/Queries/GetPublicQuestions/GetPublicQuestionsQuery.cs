using ClassManagement.Application.Modules.Questions.DTOs;

// The public question pool — every Public question from any teacher (read-only browse, T3-09).
public record GetPublicQuestionsQuery
    : QuestionFilterQuery,
        IRequest<PaginatedResult<QuestionListDto>>;
