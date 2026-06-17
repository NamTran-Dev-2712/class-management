import { http } from "@/lib/axios.config";
import type { Paginated } from "@/types/global/paginated";
import type {
    CreateExamRequest,
    SetExamVisibilityRequest,
    UpdateExamQuestionsRequest,
    UpdateExamRequest,
} from "./dtos/commands/exam-commands";
import type { ExamDetail, ExamPreview } from "./dtos/queries/exam-detail";
import type { ExamListItem, ExamListQuery } from "./dtos/queries/exam-list";

const TEACHER = "/teacher/exams";
const ADMIN = "/admin/exams";

/** Teacher exam-builder management + public-pool browsing. */
export const teacherExamService = {
    list: (query: ExamListQuery) => http.get<Paginated<ExamListItem>>(TEACHER, { params: query }),
    getById: (publicId: string) => http.get<ExamDetail>(`${TEACHER}/${publicId}`),
    preview: (publicId: string) => http.get<ExamPreview>(`${TEACHER}/${publicId}/preview`),
    create: (payload: CreateExamRequest) => http.post<{ publicId: string }>(TEACHER, payload),
    update: (publicId: string, payload: UpdateExamRequest) =>
        http.put<null>(`${TEACHER}/${publicId}`, payload),
    updateQuestions: (publicId: string, payload: UpdateExamQuestionsRequest) =>
        http.put<null>(`${TEACHER}/${publicId}/questions`, payload),
    remove: (publicId: string) => http.delete<null>(`${TEACHER}/${publicId}`),
    setVisibility: (publicId: string, payload: SetExamVisibilityRequest) =>
        http.patch<null>(`${TEACHER}/${publicId}/visibility`, payload),
    duplicate: (publicId: string) =>
        http.post<{ publicId: string }>(`${TEACHER}/${publicId}/duplicate`),
    listPublic: (query: ExamListQuery) =>
        http.get<Paginated<ExamListItem>>(`${TEACHER}/public`, { params: query }),
    getPublicById: (publicId: string) => http.get<ExamDetail>(`${TEACHER}/public/${publicId}`),
};

/** Admin read-only exam overview (all exams, including Private). */
export const adminExamService = {
    list: (query: ExamListQuery) => http.get<Paginated<ExamListItem>>(ADMIN, { params: query }),
    getById: (publicId: string) => http.get<ExamDetail>(`${ADMIN}/${publicId}`),
};
