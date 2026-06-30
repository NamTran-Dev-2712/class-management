// MVP-8 payment/subscription DTOs — mirror the backend ApiResponse payloads.

export interface PlanDto {
    publicId: string;
    name: string;
    billingCycle: string | null; // "Monthly" | "Annual" | null (Free)
    priceVnd: number;
    maxClasses: number | null; // null = unlimited
    maxQuestions: number | null;
    maxExams: number | null;
    maxStudentsPerClass: number | null;
    isActive: boolean;
    displayOrder: number;
    features: string[];
}

export type SubscriptionStatus = "Active" | "PastDue" | "Cancelled" | "Expired";

export interface SubscriptionDto {
    publicId: string | null; // null when on Free
    planName: string;
    isPro: boolean;
    status: string;
    billingCycle: string | null;
    startedAt: string | null;
    expiresAt: string | null;
    cancelledAt: string | null;
    gracePeriodEndsAt: string | null;
    paymentType: string;
}

export interface ResourceUsageItem {
    used: number;
    limit: number; // 0 = unlimited
}

export interface ResourceUsageDto {
    planName: string;
    isPro: boolean;
    classes: ResourceUsageItem;
    questions: ResourceUsageItem;
    exams: ResourceUsageItem;
}

export type PaymentProvider = "Momo" | "VnPay";

export interface CheckoutRequest {
    planPublicId: string;
    provider: PaymentProvider;
}

export interface CheckoutResultDto {
    paymentPublicId: string;
    provider: string;
    amountVnd: number;
    redirectUrl: string | null;
    qrCodeUrl: string | null;
}

export interface PaymentStatusDto {
    publicId: string;
    provider: string;
    status: string; // Pending | Completed | Failed | Expired
    amountVnd: number;
    billingCycle: string;
    createdAt: string;
    completedAt: string | null;
}

export interface InvoiceDto {
    publicId: string;
    invoiceNumber: string;
    amountVnd: number;
    planName: string;
    billingCycle: string;
    issuedAt: string;
}

// --- Admin ---

export interface AdminSubscriptionDto {
    publicId: string;
    teacherPublicId: string | null;
    teacherName: string | null;
    teacherEmail: string | null;
    planName: string;
    isPro: boolean;
    billingCycle: string | null;
    status: string;
    startedAt: string;
    expiresAt: string | null;
    cancelledAt: string | null;
    gracePeriodEndsAt: string | null;
    paymentType: string;
    adminNote: string | null;
    createdAt: string;
}

export interface AdminPaymentDto {
    publicId: string;
    teacherPublicId: string | null;
    teacherName: string | null;
    teacherEmail: string | null;
    planName: string;
    provider: string;
    billingCycle: string;
    amountVnd: number;
    status: string;
    providerTransactionId: string | null;
    createdAt: string;
    completedAt: string | null;
}

export interface RevenueByMonthDto {
    month: string; // "2026-06"
    revenueVnd: number;
    count: number;
}

export interface RevenueByPlanDto {
    planName: string;
    revenueVnd: number;
    count: number;
}

export interface RevenueSummaryDto {
    totalRevenueVnd: number;
    completedPayments: number;
    byMonth: RevenueByMonthDto[];
    byPlan: RevenueByPlanDto[];
}

export interface AdminSubscriptionListQuery {
    pageNumber: number;
    pageSize: number;
    sortBy?: string;
    sortOrder: "asc" | "desc";
    searchTerm?: string;
    status?: SubscriptionStatus;
}

export interface AdminPaymentListQuery {
    pageNumber: number;
    pageSize: number;
    sortBy?: string;
    sortOrder: "asc" | "desc";
    searchTerm?: string;
    status?: string;
    provider?: PaymentProvider;
}

export interface ManualSetProRequest {
    teacherPublicId: string;
    planPublicId: string;
    adminNote?: string;
}
