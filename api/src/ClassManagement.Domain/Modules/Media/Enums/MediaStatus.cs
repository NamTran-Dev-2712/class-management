namespace ClassManagement.Domain.Modules.Media.Enums;

// Lifecycle of a media asset (MVP-9). Pending = presigned but not yet confirmed on storage; Confirmed =
// object verified present and safe to reference. A Pending asset past its confirm TTL is swept by the
// cleanup job (BR-9-08). Stored as PascalCase text.
public enum MediaStatus
{
    Pending,
    Confirmed,
}
