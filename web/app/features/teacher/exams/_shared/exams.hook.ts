import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";

import { queryKeys } from "@/lib/query-keys";
import { teacherExamService } from "@/services/exam/exam.service";
import type {
    CreateExamRequest,
    SetExamVisibilityRequest,
    UpdateExamQuestionsRequest,
    UpdateExamRequest,
} from "@/services/exam/dtos/commands/exam-commands";
import type { ExamListQuery } from "@/services/exam/dtos/queries/exam-list";

export function useTeacherExams(query: ExamListQuery) {
    return useQuery({
        queryKey: queryKeys.exams.teacherList(query),
        queryFn: () => teacherExamService.list(query),
        placeholderData: (prev) => prev,
    });
}

export function useTeacherExam(publicId: string | undefined) {
    return useQuery({
        queryKey: queryKeys.exams.detail(publicId ?? ""),
        queryFn: () => teacherExamService.getById(publicId!),
        enabled: !!publicId,
    });
}

export function useExamPreview(publicId: string | undefined) {
    return useQuery({
        queryKey: queryKeys.exams.preview(publicId ?? ""),
        queryFn: () => teacherExamService.preview(publicId!),
        enabled: !!publicId,
    });
}

export function usePublicExam(publicId: string | undefined) {
    return useQuery({
        queryKey: queryKeys.exams.publicDetail(publicId ?? ""),
        queryFn: () => teacherExamService.getPublicById(publicId!),
        enabled: !!publicId,
    });
}

export function usePublicExams(query: ExamListQuery) {
    return useQuery({
        queryKey: queryKeys.exams.publicList(query),
        queryFn: () => teacherExamService.listPublic(query),
        placeholderData: (prev) => prev,
    });
}

function useInvalidateExams() {
    const qc = useQueryClient();
    return () => qc.invalidateQueries({ queryKey: queryKeys.exams.all });
}

export function useCreateExam() {
    const invalidate = useInvalidateExams();
    return useMutation({
        mutationFn: (payload: CreateExamRequest) => teacherExamService.create(payload),
        onSuccess: invalidate,
    });
}

export function useUpdateExam() {
    const invalidate = useInvalidateExams();
    return useMutation({
        mutationFn: (input: { publicId: string; payload: UpdateExamRequest }) =>
            teacherExamService.update(input.publicId, input.payload),
        onSuccess: invalidate,
    });
}

export function useUpdateExamQuestions() {
    const invalidate = useInvalidateExams();
    return useMutation({
        mutationFn: (input: { publicId: string; payload: UpdateExamQuestionsRequest }) =>
            teacherExamService.updateQuestions(input.publicId, input.payload),
        onSuccess: invalidate,
    });
}

export function useDeleteExam() {
    const invalidate = useInvalidateExams();
    return useMutation({
        mutationFn: (publicId: string) => teacherExamService.remove(publicId),
        onSuccess: invalidate,
    });
}

export function useSetExamVisibility() {
    const invalidate = useInvalidateExams();
    return useMutation({
        mutationFn: (input: { publicId: string; payload: SetExamVisibilityRequest }) =>
            teacherExamService.setVisibility(input.publicId, input.payload),
        onSuccess: invalidate,
    });
}

export function useDuplicateExam() {
    const invalidate = useInvalidateExams();
    return useMutation({
        mutationFn: (publicId: string) => teacherExamService.duplicate(publicId),
        onSuccess: invalidate,
    });
}
