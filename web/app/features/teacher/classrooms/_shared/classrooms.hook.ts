import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";

import { queryKeys } from "@/lib/query-keys";
import { teacherClassService } from "@/services/classroom/classroom.service";
import type {
    CreateClassRequest,
    RejectMemberRequest,
    UpdateClassRequest,
} from "@/services/classroom/dtos/commands/class-commands";
import type { ClassListQuery } from "@/services/classroom/dtos/queries/class-list";
import type { ClassMemberQuery } from "@/services/classroom/dtos/queries/class-member";
import { subjectService } from "@/services/subject/subject.service";

export function useTeacherClasses(query: ClassListQuery) {
    return useQuery({
        queryKey: queryKeys.classrooms.teacherList(query),
        queryFn: () => teacherClassService.list(query),
        placeholderData: (prev) => prev,
    });
}

export function useTeacherClass(publicId: string | undefined) {
    return useQuery({
        queryKey: queryKeys.classrooms.detail(publicId ?? ""),
        queryFn: () => teacherClassService.getById(publicId!),
        enabled: !!publicId,
    });
}

export function useClassMembers(publicId: string | undefined, query: ClassMemberQuery) {
    return useQuery({
        queryKey: queryKeys.classrooms.members(publicId ?? "", query),
        queryFn: () => teacherClassService.members(publicId!, query),
        enabled: !!publicId,
        placeholderData: (prev) => prev,
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

function useInvalidateClassrooms() {
    const qc = useQueryClient();
    return () => qc.invalidateQueries({ queryKey: queryKeys.classrooms.all });
}

export function useCreateClass() {
    const invalidate = useInvalidateClassrooms();
    return useMutation({
        mutationFn: (payload: CreateClassRequest) => teacherClassService.create(payload),
        onSuccess: invalidate,
    });
}

export function useUpdateClass() {
    const invalidate = useInvalidateClassrooms();
    return useMutation({
        mutationFn: (input: { publicId: string; payload: UpdateClassRequest }) =>
            teacherClassService.update(input.publicId, input.payload),
        onSuccess: invalidate,
    });
}

export function useArchiveClass() {
    const invalidate = useInvalidateClassrooms();
    return useMutation({
        mutationFn: (input: { publicId: string; archive: boolean }) =>
            input.archive
                ? teacherClassService.archive(input.publicId)
                : teacherClassService.unarchive(input.publicId),
        onSuccess: invalidate,
    });
}

export function useRegenerateInviteCode() {
    const invalidate = useInvalidateClassrooms();
    return useMutation({
        mutationFn: (publicId: string) => teacherClassService.regenerateInviteCode(publicId),
        onSuccess: invalidate,
    });
}

export function useApproveMember() {
    const invalidate = useInvalidateClassrooms();
    return useMutation({
        mutationFn: (input: { publicId: string; membershipId: string }) =>
            teacherClassService.approveMember(input.publicId, input.membershipId),
        onSuccess: invalidate,
    });
}

export function useRejectMember() {
    const invalidate = useInvalidateClassrooms();
    return useMutation({
        mutationFn: (input: {
            publicId: string;
            membershipId: string;
            payload: RejectMemberRequest;
        }) => teacherClassService.rejectMember(input.publicId, input.membershipId, input.payload),
        onSuccess: invalidate,
    });
}

export function useKickMember() {
    const invalidate = useInvalidateClassrooms();
    return useMutation({
        mutationFn: (input: { publicId: string; membershipId: string }) =>
            teacherClassService.kickMember(input.publicId, input.membershipId),
        onSuccess: invalidate,
    });
}
