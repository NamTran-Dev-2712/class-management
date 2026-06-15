// Teacher creates a class. SubjectId = a catalog subject's PublicId (optional); SubjectName = a
// free-text subject (used when no catalog subject is picked). Returns the new class PublicId.
public record CreateClassCommand(
    string Name,
    string? Description,
    Guid? SubjectId,
    string? SubjectName,
    string? CoverImageUrl
) : IRequest<Guid>;
