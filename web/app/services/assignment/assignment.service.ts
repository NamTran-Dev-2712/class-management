import { apiClient, http } from "@/lib/axios.config";
import type { Paginated } from "@/types/global/paginated";
import type {
    CreateAssignmentRequest,
    GradeAttemptRequest,
    RecordProctorEventsRequest,
    SaveAttemptAnswersRequest,
    UpdateAssignmentRequest,
} from "./dtos/commands/assignment-commands";
import type {
    AssignmentDetail,
    AssignmentPreview,
    AssignmentReport,
    AttemptEventItem,
    AttemptGrading,
    AttemptResult,
    AttemptTaking,
    RecordEventsResult,
    StudentAssignmentDetail,
} from "./dtos/queries/assignment-detail";
import type {
    AssignmentListItem,
    AssignmentListQuery,
    AttemptListItem,
    AttemptListQuery,
    StudentAssignmentListItem,
    StudentAssignmentListQuery,
} from "./dtos/queries/assignment-list";

const TEACHER = "/teacher/assignments";
const STUDENT = "/student/assignments";
const ADMIN = "/admin/assignments";

/** Teacher assignment management + attempt rosters. */
export const teacherAssignmentService = {
    list: (query: AssignmentListQuery) =>
        http.get<Paginated<AssignmentListItem>>(TEACHER, { params: query }),
    getById: (publicId: string) => http.get<AssignmentDetail>(`${TEACHER}/${publicId}`),
    preview: (publicId: string) => http.get<AssignmentPreview>(`${TEACHER}/${publicId}/preview`),
    attempts: (publicId: string, query: AttemptListQuery) =>
        http.get<Paginated<AttemptListItem>>(`${TEACHER}/${publicId}/attempts`, { params: query }),
    attempt: (attemptId: string) => http.get<AttemptResult>(`${TEACHER}/attempts/${attemptId}`),
    grading: (attemptId: string) =>
        http.get<AttemptGrading>(`${TEACHER}/attempts/${attemptId}/grading`),
    report: (publicId: string) => http.get<AssignmentReport>(`${TEACHER}/${publicId}/report`),
    create: (payload: CreateAssignmentRequest) => http.post<{ publicId: string }>(TEACHER, payload),
    update: (publicId: string, payload: UpdateAssignmentRequest) =>
        http.put<null>(`${TEACHER}/${publicId}`, payload),
    publish: (publicId: string) => http.post<null>(`${TEACHER}/${publicId}/publish`),
    close: (publicId: string) => http.post<null>(`${TEACHER}/${publicId}/close`),
    archive: (publicId: string) => http.post<null>(`${TEACHER}/${publicId}/archive`),
    remove: (publicId: string) => http.delete<null>(`${TEACHER}/${publicId}`),
    grade: (attemptId: string, payload: GradeAttemptRequest) =>
        http.post<null>(`${TEACHER}/attempts/${attemptId}/grade`, payload),
    releaseGrades: (publicId: string) => http.post<null>(`${TEACHER}/${publicId}/release-grades`),
    // Proctoring (MVP-10): event timeline + attempt moderation.
    attemptEvents: (attemptId: string) =>
        http.get<AttemptEventItem[]>(`${TEACHER}/attempts/${attemptId}/events`),
    forceSubmit: (attemptId: string) =>
        http.post<null>(`${TEACHER}/attempts/${attemptId}/force-submit`),
    flag: (attemptId: string) => http.post<null>(`${TEACHER}/attempts/${attemptId}/flag`),
    unflag: (attemptId: string) => http.post<null>(`${TEACHER}/attempts/${attemptId}/unflag`),
    unlock: (attemptId: string) => http.post<null>(`${TEACHER}/attempts/${attemptId}/unlock`),
    // Raw blob download (CSV file, not the ApiResponse envelope).
    exportGrades: async (publicId: string) => {
        const res = await apiClient.get<Blob>(`${TEACHER}/${publicId}/export`, {
            responseType: "blob",
        });
        return res.data;
    },
};

/** Student assignment list + online test-taking. */
export const studentAssignmentService = {
    list: (query: StudentAssignmentListQuery) =>
        http.get<Paginated<StudentAssignmentListItem>>(STUDENT, { params: query }),
    getById: (publicId: string) => http.get<StudentAssignmentDetail>(`${STUDENT}/${publicId}`),
    myAttempts: (query: AttemptListQuery) =>
        http.get<Paginated<AttemptListItem>>(`${STUDENT}/attempts`, { params: query }),
    start: (publicId: string) =>
        http.post<{ attemptId: string }>(`${STUDENT}/${publicId}/attempts`),
    taking: (attemptId: string) =>
        http.get<AttemptTaking>(`${STUDENT}/attempts/${attemptId}/taking`),
    saveAnswers: (attemptId: string, payload: SaveAttemptAnswersRequest) =>
        http.patch<null>(`${STUDENT}/attempts/${attemptId}/answers`, payload),
    submit: (attemptId: string) => http.post<null>(`${STUDENT}/attempts/${attemptId}/submit`),
    result: (attemptId: string) =>
        http.get<AttemptResult>(`${STUDENT}/attempts/${attemptId}/result`),
    // Proctoring (MVP-10): report a batch of integrity events for an in-progress attempt.
    recordEvents: (attemptId: string, payload: RecordProctorEventsRequest) =>
        http.post<RecordEventsResult>(`${STUDENT}/attempts/${attemptId}/events`, payload),
};

/** Admin read-only assignment overview. */
export const adminAssignmentService = {
    list: (query: AssignmentListQuery) =>
        http.get<Paginated<AssignmentListItem>>(ADMIN, { params: query }),
    getById: (publicId: string) => http.get<AssignmentDetail>(`${ADMIN}/${publicId}`),
};
