import { http } from "@/lib/axios.config";
import type { SubjectFormRequest } from "@/services/subject/dtos/subject-form.request";
import type { SubjectListQuery } from "@/services/subject/dtos/subject-list.query";
import type { Subject } from "@/services/subject/dtos/subject";
import type { Paginated } from "@/types/global/paginated";

export const subjectService = {
    list: (query: SubjectListQuery) => http.get<Paginated<Subject>>("/subjects", { params: query }),
    getById: (publicId: string) => http.get<Subject>(`/subjects/${publicId}`),
    create: (payload: SubjectFormRequest) => http.post<{ publicId: string }>("/subjects", payload),
    update: (publicId: string, payload: SubjectFormRequest) =>
        http.put<null>(`/subjects/${publicId}`, payload),
    remove: (publicId: string) => http.delete<null>(`/subjects/${publicId}`),
};
