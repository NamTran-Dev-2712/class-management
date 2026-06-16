import { http } from "@/lib/axios.config";
import type { Paginated } from "@/types/global/paginated";
import type {
    CreateQuestionRequest,
    SetVisibilityRequest,
    UpdateQuestionRequest,
} from "./dtos/commands/question-commands";
import type { QuestionDetail } from "./dtos/queries/question-detail";
import type { QuestionListItem, QuestionListQuery } from "./dtos/queries/question-list";

const TEACHER = "/teacher/questions";
const ADMIN = "/admin/questions";

/** Teacher question-bank management + public-pool browsing. */
export const teacherQuestionService = {
    list: (query: QuestionListQuery) =>
        http.get<Paginated<QuestionListItem>>(TEACHER, { params: query }),
    getById: (publicId: string) => http.get<QuestionDetail>(`${TEACHER}/${publicId}`),
    create: (payload: CreateQuestionRequest) => http.post<{ publicId: string }>(TEACHER, payload),
    update: (publicId: string, payload: UpdateQuestionRequest) =>
        http.put<null>(`${TEACHER}/${publicId}`, payload),
    remove: (publicId: string) => http.delete<null>(`${TEACHER}/${publicId}`),
    setVisibility: (publicId: string, payload: SetVisibilityRequest) =>
        http.patch<null>(`${TEACHER}/${publicId}/visibility`, payload),
    duplicate: (publicId: string) =>
        http.post<{ publicId: string }>(`${TEACHER}/${publicId}/duplicate`),
    listPublic: (query: QuestionListQuery) =>
        http.get<Paginated<QuestionListItem>>(`${TEACHER}/public`, { params: query }),
    getPublicById: (publicId: string) => http.get<QuestionDetail>(`${TEACHER}/public/${publicId}`),
};

/** Admin read-only question overview (all questions, including Private). */
export const adminQuestionService = {
    list: (query: QuestionListQuery) =>
        http.get<Paginated<QuestionListItem>>(ADMIN, { params: query }),
    getById: (publicId: string) => http.get<QuestionDetail>(`${ADMIN}/${publicId}`),
};
