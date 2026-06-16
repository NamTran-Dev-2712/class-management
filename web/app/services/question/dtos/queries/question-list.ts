import type { PageQuery } from "@/types/global/paginated";

export type QuestionType =
    | "SingleChoice"
    | "MultipleChoice"
    | "TrueFalse"
    | "ShortWriting"
    | "LongWriting";

export type QuestionDifficulty = "Easy" | "Medium" | "Hard";

export type QuestionVisibility = "Private" | "Public";

/** Question list row (mirrors backend QuestionListDto). */
export interface QuestionListItem {
    publicId: string;
    type: QuestionType;
    content: string;
    difficulty: QuestionDifficulty;
    suggestedPoint: number;
    visibility: QuestionVisibility;
    subjectPublicId: string | null;
    subjectName: string | null;
    teacherPublicId: string;
    teacherName: string;
    optionCount: number;
    tags: string[];
    createdAt: string;
    updatedAt: string;
}

export interface QuestionListQuery extends PageQuery {
    subjectId?: string;
    type?: QuestionType;
    difficulty?: QuestionDifficulty;
    visibility?: QuestionVisibility;
    tag?: string;
}
