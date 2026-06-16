import type {
    QuestionDifficulty,
    QuestionType,
    QuestionVisibility,
} from "../queries/question-list";

export interface QuestionOptionInput {
    content: string;
    isCorrect: boolean;
}

export interface CreateQuestionRequest {
    subjectId: string | null;
    type: QuestionType;
    content: string;
    difficulty: QuestionDifficulty;
    suggestedPoint: number;
    visibility: QuestionVisibility;
    explanation: string | null;
    options: QuestionOptionInput[];
    tags: string[];
}

export type UpdateQuestionRequest = CreateQuestionRequest;

export interface SetVisibilityRequest {
    visibility: QuestionVisibility;
}
