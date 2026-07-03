import type { PageQuery } from "@/types/global/paginated";

export type MediaKind = "Image" | "Audio" | "Video";
export type MediaStatus = "Pending" | "Confirmed";

/** Request body for POST /teacher/media/presign. */
export interface PresignRequest {
    fileName: string;
    contentType: string;
    byteSize: number;
    width?: number | null;
    height?: number | null;
    durationSeconds?: number | null;
}

/** Result of a presign — where to PUT the bytes + the id to confirm. */
export interface PresignResultDto {
    mediaPublicId: string;
    uploadUrl: string;
    httpMethod: string;
    headers: Record<string, string>;
    expiresAt: string;
}

/** A confirmed/pending media asset (never exposes the internal storage key). */
export interface MediaAssetDto {
    publicId: string;
    url: string;
    kind: MediaKind;
    contentType: string;
    byteSize: number;
    width?: number | null;
    height?: number | null;
    durationSeconds?: number | null;
    status: MediaStatus;
    createdAt: string;
}

/** A media row for the library / admin lists (adds owner for the admin surface). */
export interface MediaListItem extends MediaAssetDto {
    ownerPublicId: string;
    ownerName: string;
}

export interface StorageUsageDto {
    usedBytes: number;
    limitBytes: number; // 0 = unlimited
    isPro: boolean;
    planName: string;
}

export interface StorageOverviewItem {
    ownerPublicId: string;
    ownerName: string;
    totalBytes: number;
    assetCount: number;
}

export interface MediaListQuery extends PageQuery {
    kind?: MediaKind;
    status?: MediaStatus;
}

export interface AdminMediaListQuery extends MediaListQuery {
    ownerPublicId?: string;
}
