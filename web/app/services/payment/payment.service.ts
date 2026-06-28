import { apiClient, http } from "@/lib/axios.config";
import type { Paginated } from "@/types/global/paginated";
import type {
    AdminPaymentDto,
    AdminPaymentListQuery,
    AdminSubscriptionDto,
    AdminSubscriptionListQuery,
    CheckoutRequest,
    CheckoutResultDto,
    InvoiceDto,
    ManualSetProRequest,
    PaymentStatusDto,
    PlanDto,
    ResourceUsageDto,
    RevenueSummaryDto,
    SubscriptionDto,
} from "./dtos/payment-dtos";

const TEACHER = "/teacher/subscription";
const ADMIN_SUBS = "/admin/subscriptions";
const ADMIN_PAYMENTS = "/admin/payments";

/** Public pricing — anonymous-readable list of active plans. */
export const planService = {
    list: () => http.get<PlanDto[]>("/plans"),
};

/** A teacher's own premium surface. */
export const teacherSubscriptionService = {
    mine: () => http.get<SubscriptionDto>(TEACHER),
    usage: () => http.get<ResourceUsageDto>(`${TEACHER}/usage`),
    checkout: (payload: CheckoutRequest) =>
        http.post<CheckoutResultDto>(`${TEACHER}/checkout`, payload),
    paymentStatus: (paymentId: string) =>
        http.get<PaymentStatusDto>(`${TEACHER}/payments/${paymentId}/status`),
    cancel: () => http.post<null>(`${TEACHER}/cancel`),
    reactivate: () => http.post<null>(`${TEACHER}/reactivate`),
    invoices: () => http.get<InvoiceDto[]>(`${TEACHER}/invoices`),
    // Raw blob download (PDF file, not the ApiResponse envelope).
    downloadInvoice: async (invoiceId: string) => {
        const res = await apiClient.get<Blob>(`${TEACHER}/invoices/${invoiceId}/pdf`, {
            responseType: "blob",
        });
        return res.data;
    },
};

/** Admin subscription + payment management. */
export const adminPaymentService = {
    subscriptions: (query: AdminSubscriptionListQuery) =>
        http.get<Paginated<AdminSubscriptionDto>>(ADMIN_SUBS, { params: query }),
    manualSetPro: (payload: ManualSetProRequest) =>
        http.post<null>(`${ADMIN_SUBS}/manual-set`, payload),
    payments: (query: AdminPaymentListQuery) =>
        http.get<Paginated<AdminPaymentDto>>(ADMIN_PAYMENTS, { params: query }),
    revenue: (months = 12) =>
        http.get<RevenueSummaryDto>(`${ADMIN_PAYMENTS}/revenue`, { params: { months } }),
};
