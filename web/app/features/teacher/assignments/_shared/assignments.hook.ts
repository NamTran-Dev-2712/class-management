import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";

import { queryKeys } from "@/lib/query-keys";
import { teacherClassService } from "@/services/classroom/classroom.service";
import { teacherExamService } from "@/services/exam/exam.service";
import { teacherAssignmentService } from "@/services/assignment/assignment.service";
import type {
    CreateAssignmentRequest,
    UpdateAssignmentRequest,
} from "@/services/assignment/dtos/commands/assignment-commands";
import type {
    AssignmentListQuery,
    AttemptListQuery,
} from "@/services/assignment/dtos/queries/assignment-list";

export function useTeacherAssignments(query: AssignmentListQuery) {
    return useQuery({
        queryKey: queryKeys.assignments.teacherList(query),
        queryFn: () => teacherAssignmentService.list(query),
        placeholderData: (prev) => prev,
    });
}

export function useTeacherAssignment(publicId: string | undefined) {
    return useQuery({
        queryKey: queryKeys.assignments.detail(publicId ?? ""),
        queryFn: () => teacherAssignmentService.getById(publicId!),
        enabled: !!publicId,
    });
}

export function useAssignmentPreview(publicId: string | undefined) {
    return useQuery({
        queryKey: queryKeys.assignments.preview(publicId ?? ""),
        queryFn: () => teacherAssignmentService.preview(publicId!),
        enabled: !!publicId,
    });
}

export function useAssignmentAttempts(publicId: string | undefined, query: AttemptListQuery) {
    return useQuery({
        queryKey: queryKeys.assignments.attempts(publicId ?? "", query),
        queryFn: () => teacherAssignmentService.attempts(publicId!, query),
        enabled: !!publicId,
        placeholderData: (prev) => prev,
    });
}

/** Active exams to pick from when creating an assignment. */
export function useExamOptions() {
    return useQuery({
        queryKey: queryKeys.exams.teacherList({ pageSize: 100, sortBy: "title", sortOrder: "asc" }),
        queryFn: () =>
            teacherExamService.list({ pageSize: 100, sortBy: "title", sortOrder: "asc" }),
    });
}

/** Classes to assign to. */
export function useClassOptions() {
    return useQuery({
        queryKey: queryKeys.classrooms.teacherList({
            pageSize: 100,
            sortBy: "name",
            sortOrder: "asc",
        }),
        queryFn: () =>
            teacherClassService.list({ pageSize: 100, sortBy: "name", sortOrder: "asc" }),
    });
}

function useInvalidateAssignments() {
    const qc = useQueryClient();
    return () => qc.invalidateQueries({ queryKey: queryKeys.assignments.all });
}

export function useCreateAssignment() {
    const invalidate = useInvalidateAssignments();
    return useMutation({
        mutationFn: (payload: CreateAssignmentRequest) => teacherAssignmentService.create(payload),
        onSuccess: invalidate,
    });
}

export function useUpdateAssignment() {
    const invalidate = useInvalidateAssignments();
    return useMutation({
        mutationFn: (input: { publicId: string; payload: UpdateAssignmentRequest }) =>
            teacherAssignmentService.update(input.publicId, input.payload),
        onSuccess: invalidate,
    });
}

export function usePublishAssignment() {
    const invalidate = useInvalidateAssignments();
    return useMutation({
        mutationFn: (publicId: string) => teacherAssignmentService.publish(publicId),
        onSuccess: invalidate,
    });
}

export function useCloseAssignment() {
    const invalidate = useInvalidateAssignments();
    return useMutation({
        mutationFn: (publicId: string) => teacherAssignmentService.close(publicId),
        onSuccess: invalidate,
    });
}

export function useArchiveAssignment() {
    const invalidate = useInvalidateAssignments();
    return useMutation({
        mutationFn: (publicId: string) => teacherAssignmentService.archive(publicId),
        onSuccess: invalidate,
    });
}

export function useDeleteAssignment() {
    const invalidate = useInvalidateAssignments();
    return useMutation({
        mutationFn: (publicId: string) => teacherAssignmentService.remove(publicId),
        onSuccess: invalidate,
    });
}
