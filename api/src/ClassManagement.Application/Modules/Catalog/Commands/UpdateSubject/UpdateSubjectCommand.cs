public record UpdateSubjectCommand(
    Guid PublicId,
    string Name,
    string? Description,
    bool IsActive,
    int DisplayOrder
) : IRequest;
