import { useQuery } from "@tanstack/react-query";

import { queryKeys } from "@/lib/query-keys";
import { adminExamService } from "@/services/exam/exam.service";
import type { ExamListQuery } from "@/services/exam/dtos/queries/exam-list";

export function useAdminExams(query: ExamListQuery) {
    return useQuery({
        queryKey: queryKeys.exams.adminList(query),
        queryFn: () => adminExamService.list(query),
        placeholderData: (prev) => prev,
    });
}

export function useAdminExam(publicId: string | undefined) {
    return useQuery({
        queryKey: queryKeys.exams.adminDetail(publicId ?? ""),
        queryFn: () => adminExamService.getById(publicId!),
        enabled: !!publicId,
    });
}
