using ClassManagement.Application.Modules.Media.DTOs;

// Confirms an upload after the client PUT the bytes to storage (MVP-9). The server verifies the object
// exists (BR-9-03) and flips the asset Pending → Confirmed.
public sealed record ConfirmUploadCommand(Guid PublicId) : IRequest<MediaAssetDto>;
