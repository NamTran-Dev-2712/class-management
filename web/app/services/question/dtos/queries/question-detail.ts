import type { QuestionDifficulty, QuestionType, QuestionVisibility } from "./question-list";

export interface QuestionOptionDetail {
    content: string;
    isCorrect: boolean;
    displayOrder: number;
}

/** Single question detail (mirrors backend QuestionDetailDto). */
export interface QuestionDetail {
    publicId: string;
    type: QuestionType;
    content: string;
    difficulty: QuestionDifficulty;
    suggestedPoint: number;
    visibility: QuestionVisibility;
    explanation: string | null;
    subjectPublicId: string | null;
    subjectName: string | null;
    teacherPublicId: string;
    teacherName: string;
    tags: string[];
    options: QuestionOptionDetail[];
    createdAt: string;
    updatedAt: string;
}
