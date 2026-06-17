// Teacher deletes (soft) one of their own exams. PublicId is bound from the route.
public record DeleteExamCommand(Guid PublicId) : IRequest;
