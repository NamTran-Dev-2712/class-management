namespace ClassManagement.Domain.Modules.Media.Enums;

// Broad category of an uploaded media asset (MVP-9). Drives the MIME allowlist + per-kind size cap and
// which HTML element renders it (img / audio / video). Stored as PascalCase text.
public enum MediaKind
{
    Image,
    Audio,
    Video,
}
