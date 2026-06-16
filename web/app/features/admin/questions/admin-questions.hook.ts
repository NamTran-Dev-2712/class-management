import { useQuery } from "@tanstack/react-query";

import { queryKeys } from "@/lib/query-keys";
import { adminQuestionService } from "@/services/question/question.service";
import type { QuestionListQuery } from "@/services/question/dtos/queries/question-list";

export function useAdminQuestions(query: QuestionListQuery) {
    return useQuery({
        queryKey: queryKeys.questions.adminList(query),
        queryFn: () => adminQuestionService.list(query),
        placeholderData: (prev) => prev,
    });
}

export function useAdminQuestion(publicId: string | undefined) {
    return useQuery({
        queryKey: queryKeys.questions.adminDetail(publicId ?? ""),
        queryFn: () => adminQuestionService.getById(publicId!),
        enabled: !!publicId,
    });
}
