import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";

import { queryKeys } from "@/lib/query-keys";
import { studentClassService } from "@/services/classroom/classroom.service";
import type { JoinClassRequest } from "@/services/classroom/dtos/commands/class-commands";
import type { ClassListQuery } from "@/services/classroom/dtos/queries/class-list";
import type { ClassMemberQuery } from "@/services/classroom/dtos/queries/class-member";
import type { MembershipRequestQuery } from "@/services/classroom/dtos/queries/student-class";

export function useMyClasses(query: ClassListQuery) {
    return useQuery({
        queryKey: queryKeys.classrooms.studentList(query),
        queryFn: () => studentClassService.myClasses(query),
        placeholderData: (prev) => prev,
    });
}

export function useMyRequests(query: MembershipRequestQuery) {
    return useQuery({
        queryKey: queryKeys.classrooms.requests(query),
        queryFn: () => studentClassService.requests(query),
        placeholderData: (prev) => prev,
    });
}

export function useStudentClassMembers(publicId: string | undefined, query: ClassMemberQuery) {
    return useQuery({
        queryKey: queryKeys.classrooms.members(publicId ?? "", query),
        queryFn: () => studentClassService.members(publicId!, query),
        enabled: !!publicId,
        placeholderData: (prev) => prev,
    });
}

function useInvalidateClassrooms() {
    const qc = useQueryClient();
    return () => qc.invalidateQueries({ queryKey: queryKeys.classrooms.all });
}

export function useJoinClass() {
    const invalidate = useInvalidateClassrooms();
    return useMutation({
        mutationFn: (payload: JoinClassRequest) => studentClassService.join(payload),
        onSuccess: invalidate,
    });
}

export function useLeaveClass() {
    const invalidate = useInvalidateClassrooms();
    return useMutation({
        mutationFn: (publicId: string) => studentClassService.leave(publicId),
        onSuccess: invalidate,
    });
}
