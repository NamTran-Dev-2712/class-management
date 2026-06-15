// Teacher updates an owned class. PublicId is set from the route by the controller.
public record UpdateClassCommand(
    Guid PublicId,
    string Name,
    string? Description,
    Guid? SubjectId,
    string? SubjectName,
    string? CoverImageUrl
) : IRequest;
