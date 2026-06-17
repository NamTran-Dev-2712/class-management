import type { ExamVisibility } from "../queries/exam-list";

export interface CreateExamRequest {
    subjectId: string | null;
    title: string;
    description: string | null;
    visibility: ExamVisibility;
}

export type UpdateExamRequest = CreateExamRequest;

export interface ExamQuestionInput {
    questionId: string;
    point: number;
}

export interface UpdateExamQuestionsRequest {
    questions: ExamQuestionInput[];
}

export interface SetExamVisibilityRequest {
    visibility: ExamVisibility;
}
