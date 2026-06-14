import { http } from "@/lib/axios.config";
import type { CreateSubjectRequest } from "@/services/subject/dtos/commands/create-subject/request";
import type { UpdateSubjectRequest } from "@/services/subject/dtos/commands/update-subject/request";
import type { SubjectListQuery } from "@/services/subject/dtos/queries/list/query";
import type { Subject } from "@/services/subject/dtos/queries/list/response";
import type { SubjectDetail } from "@/services/subject/dtos/queries/detail/response";
import type { Paginated } from "@/types/global/paginated";

export const subjectService = {
    list: (query: SubjectListQuery) => http.get<Paginated<Subject>>("/subjects", { params: query }),
    getById: (publicId: string) => http.get<SubjectDetail>(`/subjects/${publicId}`),
    create: (payload: CreateSubjectRequest) =>
        http.post<{ publicId: string }>("/subjects", payload),
    update: (publicId: string, payload: UpdateSubjectRequest) =>
        http.put<null>(`/subjects/${publicId}`, payload),
    remove: (publicId: string) => http.delete<null>(`/subjects/${publicId}`),
};
