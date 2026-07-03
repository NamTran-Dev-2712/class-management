import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";

import { queryKeys } from "@/lib/query-keys";
import { teacherMediaService } from "@/services/media/media.service";
import type { MediaListQuery } from "@/services/media/dtos/media-dtos";

export function useMediaList(query: MediaListQuery) {
    return useQuery({
        queryKey: queryKeys.media.list(query),
        queryFn: () => teacherMediaService.list(query),
    });
}

export function useStorageUsage() {
    return useQuery({
        queryKey: queryKeys.media.usage(),
        queryFn: () => teacherMediaService.usage(),
    });
}

export function useDeleteMedia() {
    const qc = useQueryClient();
    return useMutation({
        mutationFn: (publicId: string) => teacherMediaService.remove(publicId),
        onSuccess: () => qc.invalidateQueries({ queryKey: queryKeys.media.all }),
    });
}
