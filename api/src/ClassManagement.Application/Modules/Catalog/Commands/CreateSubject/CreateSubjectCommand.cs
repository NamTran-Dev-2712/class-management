public record CreateSubjectCommand(
    string Name,
    string? Description,
    bool IsActive,
    int DisplayOrder
) : IRequest<Guid>;
