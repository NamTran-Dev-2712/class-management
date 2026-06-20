import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";

import { queryKeys } from "@/lib/query-keys";
import { studentAssignmentService } from "@/services/assignment/assignment.service";
import type { SaveAttemptAnswersRequest } from "@/services/assignment/dtos/commands/assignment-commands";
import type {
    AttemptListQuery,
    StudentAssignmentListQuery,
} from "@/services/assignment/dtos/queries/assignment-list";

export function useStudentAssignments(query: StudentAssignmentListQuery) {
    return useQuery({
        queryKey: queryKeys.assignments.studentList(query),
        queryFn: () => studentAssignmentService.list(query),
        placeholderData: (prev) => prev,
    });
}

export function useStudentAssignment(publicId: string | undefined) {
    return useQuery({
        queryKey: queryKeys.assignments.studentDetail(publicId ?? ""),
        queryFn: () => studentAssignmentService.getById(publicId!),
        enabled: !!publicId,
    });
}

export function useAttemptTaking(attemptId: string | undefined) {
    return useQuery({
        queryKey: queryKeys.assignments.taking(attemptId ?? ""),
        queryFn: () => studentAssignmentService.taking(attemptId!),
        enabled: !!attemptId,
        // Always refetch on mount so the remaining-time + draft answers are fresh after a reload.
        staleTime: 0,
        refetchOnWindowFocus: false,
    });
}

export function useMyAttempts(query: AttemptListQuery, enabled = true) {
    return useQuery({
        queryKey: queryKeys.assignments.myAttempts(query),
        queryFn: () => studentAssignmentService.myAttempts(query),
        enabled,
    });
}

export function useAttemptResult(attemptId: string | undefined, enabled = true) {
    return useQuery({
        queryKey: queryKeys.assignments.result(attemptId ?? ""),
        queryFn: () => studentAssignmentService.result(attemptId!),
        enabled: !!attemptId && enabled,
    });
}

function useInvalidateAssignments() {
    const qc = useQueryClient();
    return () => qc.invalidateQueries({ queryKey: queryKeys.assignments.all });
}

export function useStartAttempt() {
    const invalidate = useInvalidateAssignments();
    return useMutation({
        mutationFn: (publicId: string) => studentAssignmentService.start(publicId),
        onSuccess: invalidate,
    });
}

export function useSaveAttemptAnswers() {
    return useMutation({
        mutationFn: (input: { attemptId: string; payload: SaveAttemptAnswersRequest }) =>
            studentAssignmentService.saveAnswers(input.attemptId, input.payload),
    });
}

export function useSubmitAttempt() {
    const invalidate = useInvalidateAssignments();
    return useMutation({
        mutationFn: (attemptId: string) => studentAssignmentService.submit(attemptId),
        onSuccess: invalidate,
    });
}
