import { http } from "@/lib/axios.config";
import type { Paginated } from "@/types/global/paginated";
import type {
    CreateClassRequest,
    JoinClassRequest,
    RejectMemberRequest,
    UpdateClassRequest,
} from "./dtos/commands/class-commands";
import type { ClassDetail } from "./dtos/queries/class-detail";
import type { ClassListItem, ClassListQuery } from "./dtos/queries/class-list";
import type { ClassMember, ClassMemberQuery } from "./dtos/queries/class-member";
import type {
    MembershipRequest,
    MembershipRequestQuery,
    StudentClass,
} from "./dtos/queries/student-class";

const TEACHER = "/teacher/classes";
const STUDENT = "/student/classes";
const ADMIN = "/admin/classes";

/** Teacher class management. */
export const teacherClassService = {
    list: (query: ClassListQuery) => http.get<Paginated<ClassListItem>>(TEACHER, { params: query }),
    getById: (publicId: string) => http.get<ClassDetail>(`${TEACHER}/${publicId}`),
    create: (payload: CreateClassRequest) => http.post<{ publicId: string }>(TEACHER, payload),
    update: (publicId: string, payload: UpdateClassRequest) =>
        http.put<null>(`${TEACHER}/${publicId}`, payload),
    archive: (publicId: string) => http.post<null>(`${TEACHER}/${publicId}/archive`),
    unarchive: (publicId: string) => http.post<null>(`${TEACHER}/${publicId}/unarchive`),
    regenerateInviteCode: (publicId: string) =>
        http.post<{ inviteCode: string }>(`${TEACHER}/${publicId}/invite-code/regenerate`),
    members: (publicId: string, query: ClassMemberQuery) =>
        http.get<Paginated<ClassMember>>(`${TEACHER}/${publicId}/members`, { params: query }),
    approveMember: (publicId: string, membershipId: string) =>
        http.post<null>(`${TEACHER}/${publicId}/members/${membershipId}/approve`),
    rejectMember: (publicId: string, membershipId: string, payload: RejectMemberRequest) =>
        http.post<null>(`${TEACHER}/${publicId}/members/${membershipId}/reject`, payload),
    kickMember: (publicId: string, membershipId: string) =>
        http.delete<null>(`${TEACHER}/${publicId}/members/${membershipId}`),
};

/** Student class participation. */
export const studentClassService = {
    join: (payload: JoinClassRequest) =>
        http.post<{ classPublicId: string }>(`${STUDENT}/join`, payload),
    myClasses: (query: ClassListQuery) =>
        http.get<Paginated<StudentClass>>(STUDENT, { params: query }),
    requests: (query: MembershipRequestQuery) =>
        http.get<Paginated<MembershipRequest>>(`${STUDENT}/requests`, { params: query }),
    members: (publicId: string, query: ClassMemberQuery) =>
        http.get<Paginated<ClassMember>>(`${STUDENT}/${publicId}/members`, { params: query }),
    leave: (publicId: string) => http.post<null>(`${STUDENT}/${publicId}/leave`),
};

/** Admin read-only class overview. */
export const adminClassService = {
    list: (query: ClassListQuery) => http.get<Paginated<ClassListItem>>(ADMIN, { params: query }),
    getById: (publicId: string) => http.get<ClassDetail>(`${ADMIN}/${publicId}`),
};
