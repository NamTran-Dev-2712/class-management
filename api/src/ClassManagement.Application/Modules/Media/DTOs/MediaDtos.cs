namespace ClassManagement.Application.Modules.Media.DTOs;

// Response DTOs for the Media module (MVP-9). The internal storage_key is NEVER exposed (BR-9-01) — only
// public_id + the CDN url.

/// <summary>Result of a presign request: where to PUT the bytes + the id to confirm afterwards.</summary>
public sealed record PresignResultDto(
    Guid MediaPublicId,
    string UploadUrl,
    string HttpMethod,
    IReadOnlyDictionary<string, string> Headers,
    DateTime ExpiresAt
);

/// <summary>A single media asset (detail / confirm response).</summary>
public sealed record MediaAssetDto(
    Guid PublicId,
    string Url,
    string Kind,
    string ContentType,
    long ByteSize,
    int? Width,
    int? Height,
    int? DurationSeconds,
    string Status,
    DateTime CreatedAt
);

/// <summary>A media asset row for the library / admin lists (adds owner for the admin surface).</summary>
public sealed record MediaListDto(
    Guid PublicId,
    string Url,
    string Kind,
    string ContentType,
    long ByteSize,
    int? Width,
    int? Height,
    int? DurationSeconds,
    string Status,
    DateTime CreatedAt,
    Guid OwnerPublicId,
    string OwnerName
);

/// <summary>Teacher's current storage usage vs. their plan cap (0 = unlimited).</summary>
public sealed record StorageUsageDto(long UsedBytes, long LimitBytes, bool IsPro, string PlanName);

/// <summary>Per-teacher storage aggregate for the admin overview.</summary>
public sealed record StorageOverviewDto(
    Guid OwnerPublicId,
    string OwnerName,
    long TotalBytes,
    int AssetCount
);
