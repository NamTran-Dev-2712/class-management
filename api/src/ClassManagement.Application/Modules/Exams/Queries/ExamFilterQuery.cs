using ClassManagement.Domain.Modules.Exams.Enums;

// Shared filter surface for exam lists: subject (by public id), visibility, plus the inherited
// paging/sort/search. Concrete queries differ only by their scope.
public abstract record ExamFilterQuery : BaseFilterQuery
{
    public Guid? SubjectId { get; init; }
    public ExamVisibility? Visibility { get; init; }
}
