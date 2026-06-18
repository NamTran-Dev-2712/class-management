import type { PageQuery } from "@/types/global/paginated";

export type AssignmentStatus = "Draft" | "Scheduled" | "Open" | "Closed" | "Archived";
export type ScorePolicy = "Highest" | "Latest";
export type GradePublishPolicy = "Immediate" | "AfterDeadline" | "Manual";
export type AttemptStatus =
    | "InProgress"
    | "Submitted"
    | "AutoGraded"
    | "NeedManualGrading"
    | "Graded";

/** Teacher/admin assignment list row (mirrors backend AssignmentListDto). */
export interface AssignmentListItem {
    publicId: string;
    title: string;
    status: AssignmentStatus;
    opensAt: string | null;
    closesAt: string | null;
    timeLimitMinutes: number | null;
    maxAttempts: number;
    totalPoint: number | null;
    totalQuestions: number | null;
    examPublicId: string;
    examTitle: string;
    classPublicId: string;
    className: string;
    teacherPublicId: string;
    teacherName: string;
    attemptCount: number;
    submittedCount: number;
    createdAt: string;
    updatedAt: string;
}

export interface AssignmentListQuery extends PageQuery {
    status?: AssignmentStatus;
    classId?: string;
}

/** Student assignment list row (mirrors backend StudentAssignmentListDto). */
export interface StudentAssignmentListItem {
    publicId: string;
    title: string;
    status: AssignmentStatus;
    opensAt: string | null;
    closesAt: string | null;
    timeLimitMinutes: number | null;
    maxAttempts: number;
    allowLate: boolean;
    classPublicId: string;
    className: string;
    teacherName: string;
    totalPoint: number | null;
    totalQuestions: number | null;
    usedAttempts: number;
    attemptsLeft: number;
    hasInProgress: boolean;
    inProgressAttemptPublicId: string | null;
    bestScore: number | null;
}

export interface StudentAssignmentListQuery extends PageQuery {
    status?: AssignmentStatus;
    classId?: string;
}

/** Attempt list row for rosters/history (mirrors backend AttemptListDto). */
export interface AttemptListItem {
    publicId: string;
    status: AttemptStatus;
    attemptNumber: number;
    startedAt: string;
    submittedAt: string | null;
    deadlineAt: string | null;
    autoSubmitted: boolean;
    totalAutoScore: number | null;
    totalManualScore: number | null;
    totalScore: number | null;
    totalPoint: number | null;
    assignmentPublicId: string;
    assignmentTitle: string;
    studentPublicId: string;
    studentName: string;
    createdAt: string;
}

export interface AttemptListQuery extends PageQuery {
    status?: AttemptStatus;
    assignmentId?: string;
}
