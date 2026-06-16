using ClassManagement.Application.Modules.Questions.DTOs;

// A teacher's own question bank (BR-3-07/08 owner scope), filterable by subject/type/difficulty/
// visibility/tag and searchable by content keyword.
public record GetTeacherQuestionsQuery
    : QuestionFilterQuery,
        IRequest<PaginatedResult<QuestionListDto>>;
