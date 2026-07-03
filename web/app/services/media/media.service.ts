import axios from "axios";

import { http } from "@/lib/axios.config";
import type { Paginated } from "@/types/global/paginated";
import type {
    AdminMediaListQuery,
    MediaAssetDto,
    MediaListItem,
    MediaListQuery,
    PresignRequest,
    PresignResultDto,
    StorageOverviewItem,
    StorageUsageDto,
} from "./dtos/media-dtos";

const TEACHER = "/teacher/media";
const ADMIN = "/admin/media";

/** Teacher media library + presigned upload flow. */
export const teacherMediaService = {
    list: (query: MediaListQuery) => http.get<Paginated<MediaListItem>>(TEACHER, { params: query }),
    usage: () => http.get<StorageUsageDto>(`${TEACHER}/usage`),
    presign: (payload: PresignRequest) =>
        http.post<PresignResultDto>(`${TEACHER}/presign`, payload),
    confirm: (mediaPublicId: string) =>
        http.post<MediaAssetDto>(`${TEACHER}/${mediaPublicId}/confirm`),
    remove: (mediaPublicId: string) => http.delete<null>(`${TEACHER}/${mediaPublicId}`),
};

/** Admin storage overview + moderation. */
export const adminMediaService = {
    list: (query: AdminMediaListQuery) =>
        http.get<Paginated<MediaListItem>>(ADMIN, { params: query }),
    overview: (query: { pageNumber?: number; pageSize?: number }) =>
        http.get<Paginated<StorageOverviewItem>>(`${ADMIN}/overview`, { params: query }),
    remove: (mediaPublicId: string) => http.delete<null>(`${ADMIN}/${mediaPublicId}`),
};

/**
 * Uploads the file bytes directly to storage using a presigned URL. Uses a BARE axios call — NOT the
 * shared apiClient — so it does not send auth cookies, prepend the API baseURL, force a JSON content
 * type, or run the 401 refresh interceptor (BR-9-02: client → storage, never through the API).
 */
export async function putToPresignedUrl(
    uploadUrl: string,
    file: File,
    headers: Record<string, string>,
): Promise<void> {
    await axios.put(uploadUrl, file, {
        withCredentials: false,
        headers: { ...headers, "Content-Type": file.type },
    });
}
