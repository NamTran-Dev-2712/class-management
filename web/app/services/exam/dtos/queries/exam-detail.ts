import type { ExamVisibility } from "./exam-list";

/** One question placed in an exam (mirrors backend ExamQuestionDto). */
export interface ExamQuestionItem {
    questionPublicId: string;
    displayOrder: number;
    point: number;
    isAvailable: boolean;
    type: string;
    content: string;
    difficulty: string;
    optionCount: number;
    subjectPublicId: string | null;
    subjectName: string | null;
}

/** Single exam detail (mirrors backend ExamDetailDto). */
export interface ExamDetail {
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
    questions: ExamQuestionItem[];
    createdAt: string;
    updatedAt: string;
}

/** Student-facing preview (mirrors backend ExamPreviewDto) — no correct-answer flags. */
export interface ExamPreviewOption {
    content: string;
    displayOrder: number;
}

export interface ExamPreviewQuestion {
    displayOrder: number;
    point: number;
    isAvailable: boolean;
    type: string;
    content: string;
    difficulty: string;
    options: ExamPreviewOption[];
}

export interface ExamPreview {
    publicId: string;
    title: string;
    description: string | null;
    totalPoint: number;
    totalQuestions: number;
    questions: ExamPreviewQuestion[];
}
