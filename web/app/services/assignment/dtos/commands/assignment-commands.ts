import type { GradePublishPolicy, ScorePolicy } from "../queries/assignment-list";

export interface CreateAssignmentRequest {
    examId: string;
    classId: string;
    title: string;
    description: string | null;
    opensAt: string | null;
    closesAt: string | null;
    timeLimitMinutes: number | null;
    maxAttempts: number;
    scorePolicy: ScorePolicy;
    allowLate: boolean;
    gradePublishPolicy: GradePublishPolicy;
    shuffleQuestions: boolean;
    shuffleOptions: boolean;
    showAnswersAfterGrade: boolean;
}

/** Update omits the immutable exam/class link. */
export type UpdateAssignmentRequest = Omit<CreateAssignmentRequest, "examId" | "classId">;

export interface AttemptAnswerInput {
    questionId: string;
    selectedOptionIds: string[];
    textAnswer: string | null;
}

export interface SaveAttemptAnswersRequest {
    answers: AttemptAnswerInput[];
}

export interface GradeItemInput {
    questionPublicId: string;
    score: number;
    feedback: string | null;
}

export interface GradeAttemptRequest {
    grades: GradeItemInput[];
}
