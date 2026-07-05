import type { GradePublishPolicy, ScorePolicy } from "../queries/assignment-list";

/** Server action when an attempt exceeds MaxViolations (MVP-10). */
export type ViolationAction = "WarnOnly" | "AutoSubmit" | "LockAttempt";

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
    // Proctoring config (MVP-10).
    requireFullscreen: boolean;
    detectTabSwitch: boolean;
    blockCopyPaste: boolean;
    maxViolations: number;
    violationAction: ViolationAction;
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

/** One proctoring signal reported by the browser (MVP-10). */
export interface ProctorEventInput {
    eventType: string;
    occurredAt?: string | null;
    metadata?: Record<string, unknown> | null;
}

export interface RecordProctorEventsRequest {
    events: ProctorEventInput[];
}
