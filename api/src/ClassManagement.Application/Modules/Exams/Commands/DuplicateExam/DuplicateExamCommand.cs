// Teacher duplicates an exam they can see (their own, or any Public one) into their own bank as a
// fresh Private copy (BR-4-08: independent, no back-link to the original). The question references
// (and their points/order) are copied as-is. Returns the new PublicId.
public record DuplicateExamCommand(Guid PublicId) : IRequest<Guid>;
