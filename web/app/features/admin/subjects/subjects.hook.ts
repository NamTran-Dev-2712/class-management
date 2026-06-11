import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";

import { queryKeys } from "@/lib/query-keys";
import { subjectService } from "@/services/subject/subject.service";
import type { SubjectFormRequest } from "@/services/subject/dtos/subject-form.request";
import type { SubjectListQuery } from "@/services/subject/dtos/subject-list.query";

export function useSubjects(query: SubjectListQuery) {
    return useQuery({
        queryKey: queryKeys.subjects.list(query),
        queryFn: () => subjectService.list(query),
        // Keep the previous page visible while the next loads (no flash of empty table).
        placeholderData: (prev) => prev,
    });
}

export function useCreateSubject() {
    const qc = useQueryClient();
    return useMutation({
        mutationFn: (payload: SubjectFormRequest) => subjectService.create(payload),
        onSuccess: () => qc.invalidateQueries({ queryKey: queryKeys.subjects.all }),
    });
}

export function useUpdateSubject() {
    const qc = useQueryClient();
    return useMutation({
        mutationFn: (input: { publicId: string; payload: SubjectFormRequest }) =>
            subjectService.update(input.publicId, input.payload),
        onSuccess: () => qc.invalidateQueries({ queryKey: queryKeys.subjects.all }),
    });
}

export function useDeleteSubject() {
    const qc = useQueryClient();
    return useMutation({
        mutationFn: (publicId: string) => subjectService.remove(publicId),
        onSuccess: () => qc.invalidateQueries({ queryKey: queryKeys.subjects.all }),
    });
}
