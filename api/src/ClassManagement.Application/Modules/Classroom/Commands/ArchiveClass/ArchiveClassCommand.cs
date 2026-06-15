// Teacher archives (archive=true) or unarchives (archive=false) an owned class.
public record ArchiveClassCommand(Guid PublicId, bool Archive) : IRequest;
