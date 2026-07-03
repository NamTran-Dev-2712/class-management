using ClassManagement.Application.Modules.Media.DTOs;

// Requests a presigned upload URL for a media file (MVP-9). The server validates kind/MIME/size + quota
// BEFORE presigning and creates a Pending MediaAsset; the client then PUTs the bytes straight to storage
// and calls Confirm.
public sealed record PresignUploadCommand(
    string FileName,
    string ContentType,
    long ByteSize,
    int? Width,
    int? Height,
    int? DurationSeconds
) : IRequest<PresignResultDto>;
