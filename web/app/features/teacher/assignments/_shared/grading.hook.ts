import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";

import { queryKeys } from "@/lib/query-keys";
import { teacherAssignmentService } from "@/services/assignment/assignment.service";
import type { GradeAttemptRequest } from "@/services/assignment/dtos/commands/assignment-commands";

/** Teacher grading view of one attempt (questions + student answers). */
export function useAttemptGrading(attemptId: string | undefined) {
    return useQuery({
        queryKey: queryKeys.assignments.grading(attemptId ?? ""),
        queryFn: () => teacherAssignmentService.grading(attemptId!),
        enabled: !!attemptId,
    });
}

/** Aggregate report (stats + histogram + per-student grades) for an assignment. */
export function useAssignmentReport(publicId: string | undefined) {
    return useQuery({
        queryKey: queryKeys.assignments.report(publicId ?? ""),
        queryFn: () => teacherAssignmentService.report(publicId!),
        enabled: !!publicId,
    });
}

export function useGradeAttempt(attemptId: string) {
    const qc = useQueryClient();
    return useMutation({
        mutationFn: (payload: GradeAttemptRequest) =>
            teacherAssignmentService.grade(attemptId, payload),
        onSuccess: () => {
            qc.invalidateQueries({ queryKey: queryKeys.assignments.all });
        },
    });
}

export function usePublishGrades() {
    const qc = useQueryClient();
    return useMutation({
        mutationFn: (publicId: string) => teacherAssignmentService.releaseGrades(publicId),
        onSuccess: () => {
            qc.invalidateQueries({ queryKey: queryKeys.assignments.all });
        },
    });
}
