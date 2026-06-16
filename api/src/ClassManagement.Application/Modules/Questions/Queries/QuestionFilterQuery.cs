using ClassManagement.Domain.Modules.Questions.Enums;

// Shared filter surface for question lists: subject (by public id), type, difficulty, visibility,
// tag, plus the inherited paging/sort/search. Concrete queries differ only by their scope.
public abstract record QuestionFilterQuery : BaseFilterQuery
{
    public Guid? SubjectId { get; init; }
    public QuestionType? Type { get; init; }
    public QuestionDifficulty? Difficulty { get; init; }
    public QuestionVisibility? Visibility { get; init; }
    public string? Tag { get; init; }
}
