// Soft-deletes a media asset (MVP-9). Owner or Admin only (BR-9-07). The physical object is removed later
// by the cleanup job, which skips assets still pinned by a published snapshot (BR-9-06).
public sealed record DeleteMediaCommand(Guid PublicId) : IRequest;
