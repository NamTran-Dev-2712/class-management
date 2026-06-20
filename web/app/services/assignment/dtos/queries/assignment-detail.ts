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

export interface AttemptAnswerOption {
    publicId: string;
    content: string;
    isSelected: boolean;
    isCorrect: boolean | null;
}

export interface AttemptAnswerResult {
    questionPublicId: string;
    displayPosition: number;
    type: string;
    content: string;
    point: number;
    isWriting: boolean;
    autoScore: number | null;
    isAutoGraded: boolean;
    textAnswer: string | null;
    manualScore: number | null;
    feedback: string | null;
    options: AttemptAnswerOption[];
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
    showAnswers: boolean;
    totalAutoScore: number | null;
    totalManualScore: number | null;
    totalScore: number | null;
    answers: AttemptAnswerResult[];
}

/** One snapshot option in the teacher's grading view (mirrors backend GradingOptionDto). */
export interface GradingOption {
    publicId: string;
    content: string;
    isCorrect: boolean;
    isSelected: boolean;
}

/** One question with the student's answer, for grading (mirrors backend GradingQuestionDto). */
export interface GradingQuestion {
    questionPublicId: string;
    displayPosition: number;
    type: string;
    content: string;
    point: number;
    isWriting: boolean;
    autoScore: number | null;
    options: GradingOption[];
    textAnswer: string | null;
    manualScore: number | null;
    feedback: string | null;
}

/** Teacher grading view of one attempt (mirrors backend AttemptGradingDto). */
export interface AttemptGrading {
    publicId: string;
    assignmentPublicId: string;
    assignmentTitle: string;
    studentPublicId: string;
    studentName: string;
    attemptNumber: number;
    status: AttemptStatus;
    submittedAt: string | null;
    autoSubmitted: boolean;
    totalPoint: number | null;
    totalAutoScore: number | null;
    totalManualScore: number | null;
    totalScore: number | null;
    questions: GradingQuestion[];
}

export interface ReportBucket {
    rangeStart: number;
    rangeEnd: number;
    count: number;
    label: string;
}

export interface ReportStudentRow {
    studentPublicId: string;
    studentName: string;
    submitted: boolean;
    status: string;
    effectiveScore: number | null;
    attemptCount: number;
}

/** Assignment report (mirrors backend AssignmentReportDto). */
export interface AssignmentReport {
    publicId: string;
    title: string;
    scorePolicy: ScorePolicy;
    gradePublishPolicy: GradePublishPolicy;
    gradesReleasedAt: string | null;
    totalPoint: number | null;
    totalStudents: number;
    submittedCount: number;
    notSubmittedCount: number;
    gradedCount: number;
    pendingGradingCount: number;
    averageScore: number | null;
    minScore: number | null;
    maxScore: number | null;
    buckets: ReportBucket[];
    students: ReportStudentRow[];
}
