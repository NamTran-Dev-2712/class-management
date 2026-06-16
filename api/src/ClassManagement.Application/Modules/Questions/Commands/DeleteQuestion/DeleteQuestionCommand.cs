// Teacher deletes (soft) one of their own questions. PublicId bound from the route.
public record DeleteQuestionCommand(Guid PublicId) : IRequest;
