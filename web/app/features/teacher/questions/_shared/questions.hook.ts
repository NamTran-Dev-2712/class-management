import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";

import { queryKeys } from "@/lib/query-keys";
import { teacherQuestionService } from "@/services/question/question.service";
import type {
    CreateQuestionRequest,
    SetVisibilityRequest,
    UpdateQuestionRequest,
} from "@/services/question/dtos/commands/question-commands";
import type { QuestionListQuery } from "@/services/question/dtos/queries/question-list";
import { subjectService } from "@/services/subject/subject.service";

export function useTeacherQuestions(query: QuestionListQuery) {
    return useQuery({
        queryKey: queryKeys.questions.teacherList(query),
        queryFn: () => teacherQuestionService.list(query),
        placeholderData: (prev) => prev,
    });
}

export function useTeacherQuestion(publicId: string | undefined) {
    return useQuery({
        queryKey: queryKeys.questions.detail(publicId ?? ""),
        queryFn: () => teacherQuestionService.getById(publicId!),
        enabled: !!publicId,
    });
}

export function usePublicQuestions(query: QuestionListQuery) {
    return useQuery({
        queryKey: queryKeys.questions.publicList(query),
        queryFn: () => teacherQuestionService.listPublic(query),
        placeholderData: (prev) => prev,
    });
}

export function usePublicQuestion(publicId: string | undefined) {
    return useQuery({
        queryKey: queryKeys.questions.publicDetail(publicId ?? ""),
        queryFn: () => teacherQuestionService.getPublicById(publicId!),
        enabled: !!publicId,
    });
}

/** Active catalog subjects for the create/edit form's subject picker. */
export function useActiveSubjects() {
    return useQuery({
        queryKey: queryKeys.subjects.list({ active: true, all: true }),
        queryFn: () =>
            subjectService.list({
                pageNumber: 1,
                pageSize: 100,
                isActive: true,
                sortBy: "name",
                sortOrder: "asc",
            }),
    });
}

function useInvalidateQuestions() {
    const qc = useQueryClient();
    return () => qc.invalidateQueries({ queryKey: queryKeys.questions.all });
}

export function useCreateQuestion() {
    const invalidate = useInvalidateQuestions();
    return useMutation({
        mutationFn: (payload: CreateQuestionRequest) => teacherQuestionService.create(payload),
        onSuccess: invalidate,
    });
}

export function useUpdateQuestion() {
    const invalidate = useInvalidateQuestions();
    return useMutation({
        mutationFn: (input: { publicId: string; payload: UpdateQuestionRequest }) =>
            teacherQuestionService.update(input.publicId, input.payload),
        onSuccess: invalidate,
    });
}

export function useDeleteQuestion() {
    const invalidate = useInvalidateQuestions();
    return useMutation({
        mutationFn: (publicId: string) => teacherQuestionService.remove(publicId),
        onSuccess: invalidate,
    });
}

export function useSetQuestionVisibility() {
    const invalidate = useInvalidateQuestions();
    return useMutation({
        mutationFn: (input: { publicId: string; payload: SetVisibilityRequest }) =>
            teacherQuestionService.setVisibility(input.publicId, input.payload),
        onSuccess: invalidate,
    });
}

export function useDuplicateQuestion() {
    const invalidate = useInvalidateQuestions();
    return useMutation({
        mutationFn: (publicId: string) => teacherQuestionService.duplicate(publicId),
        onSuccess: invalidate,
    });
}
