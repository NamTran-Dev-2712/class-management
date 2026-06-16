// Teacher duplicates a question they can see (their own, or any Public one) into their own bank as a
// fresh Private copy (BR-3-09: independent, no back-link to the original). Returns the new PublicId.
public record DuplicateQuestionCommand(Guid PublicId) : IRequest<Guid>;
