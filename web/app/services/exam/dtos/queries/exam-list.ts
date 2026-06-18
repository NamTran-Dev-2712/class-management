import type { PageQuery } from "@/types/global/paginated";

export type ExamVisibility = "Private" | "Public";

/** Exam list row (mirrors backend ExamListDto). */
export interface ExamListItem {
    publicId: string;
    title: string;
    description: string | null;
    visibility: ExamVisibility;
    version: number;
    totalPoint: number;
    totalQuestions: number;
    subjectPublicId: string | null;
    subjectName: string | null;
    teacherPublicId: string;
    teacherName: string;
    tags: string[];
    createdAt: string;
    updatedAt: string;
}

export interface ExamListQuery extends PageQuery {
    subjectId?: string;
    visibility?: ExamVisibility;
    tag?: string;
}
