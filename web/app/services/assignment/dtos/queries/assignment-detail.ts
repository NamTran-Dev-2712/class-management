import type {
    AssignmentStatus,
    AttemptStatus,
    GradePublishPolicy,
    ScorePolicy,
} from "./assignment-list";

export interface SnapshotOption {
    publicId: string;
    content: string;
    isCorrect: boolean;
    displayOrder: number;
}

export interface SnapshotQuestion {
    publicId: string;
    type: string;
    content: string;
    point: number;
    displayOrder: number;
    explanation: string | null;
    options: SnapshotOption[];
}

/** Teacher/admin assignment detail (mirrors backend AssignmentDetailDto). */
export interface AssignmentDetail {
    publicId: string;
    title: string;
    description: string | null;
    status: AssignmentStatus;
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
    publishedAt: string | null;
    closedAt: string | null;
    examVersionAtPublish: number | null;
    examPublicId: string;
    examTitle: string;
    classPublicId: string;
    className: string;
    teacherPublicId: string;
    teacherName: string;
    totalPoint: number | null;
    totalQuestions: number | null;
    attemptCount: number;
    submittedCount: number;
    questions: SnapshotQuestion[];
    createdAt: string;
    updatedAt: string;
}

export interface PreviewOption {
    publicId: string;
    content: string;
    displayOrder: number;
}

export interface PreviewQuestion {
    publicId: string;
    type: string;
    content: string;
    point: number;
    displayOrder: number;
    options: PreviewOption[];
}

export interface AssignmentPreview {
    publicId: string;
    title: string;
    description: string | null;
    totalPoint: number | null;
    totalQuestions: number | null;
    questions: PreviewQuestion[];
}

/** Student assignment detail (mirrors backend StudentAssignmentDetailDto). */
export interface StudentAssignmentDetail {
    publicId: string;
    title: string;
    description: string | null;
    status: AssignmentStatus;
    opensAt: string | null;
    closesAt: string | null;
    timeLimitMinutes: number | null;
    maxAttempts: number;
    allowLate: boolean;
    gradePublishPolicy: GradePublishPolicy;
    classPublicId: string;
    className: string;
    teacherName: string;
    totalPoint: number | null;
    totalQuestions: number | null;
    usedAttempts: number;
    attemptsLeft: number;
    hasInProgress: boolean;
    inProgressAttemptPublicId: string | null;
    canStart: boolean;
}

export interface TakingOption {
    publicId: string;
    content: string;
    displayOrder: number;
}

export interface TakingQuestion {
    publicId: string;
    displayPosition: number;
    type: string;
    content: string;
    point: number;
    options: TakingOption[];
    selectedOptionIds: string[];
    textAnswer: string | null;
}

/** Live test-taking session (mirrors backend AttemptTakingDto). */
export interface AttemptTaking {
    publicId: string;
    assignmentPublicId: string;
    assignmentTitle: string;
    status: AttemptStatus;
    attemptNumber: number;
    startedAt: string;
    deadlineAt: string | null;
    remainingSeconds: number | null;
    totalPoint: number | null;
    questions: TakingQuestion[];
}

export interface AttemptAnswerResult {
    questionPublicId: string;
    displayPosition: number;
    type: string;
    point: number;
    autoScore: number | null;
    isAutoGraded: boolean;
}

/** Attempt result (mirrors backend AttemptResultDto). */
export interface AttemptResult {
    publicId: string;
    assignmentPublicId: string;
    assignmentTitle: string;
    status: AttemptStatus;
    attemptNumber: number;
    startedAt: string;
    submittedAt: string | null;
    autoSubmitted: boolean;
    totalPoint: number | null;
    scoreReleased: boolean;
    totalAutoScore: number | null;
    totalManualScore: number | null;
    totalScore: number | null;
    answers: AttemptAnswerResult[];
}
