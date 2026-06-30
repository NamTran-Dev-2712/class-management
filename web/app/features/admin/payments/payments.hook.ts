import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";

import { queryKeys } from "@/lib/query-keys";
import { adminPaymentService } from "@/services/payment/payment.service";
import type {
    AdminPaymentListQuery,
    AdminSubscriptionListQuery,
    ManualSetProRequest,
} from "@/services/payment/dtos/payment-dtos";

export function useAdminSubscriptions(query: AdminSubscriptionListQuery) {
    return useQuery({
        queryKey: queryKeys.subscriptions.adminList(query),
        queryFn: () => adminPaymentService.subscriptions(query),
        placeholderData: (prev) => prev,
    });
}

export function useAdminPayments(query: AdminPaymentListQuery) {
    return useQuery({
        queryKey: queryKeys.payments.adminList(query),
        queryFn: () => adminPaymentService.payments(query),
        placeholderData: (prev) => prev,
    });
}

export function useRevenue(months = 12) {
    return useQuery({
        queryKey: queryKeys.payments.revenue(months),
        queryFn: () => adminPaymentService.revenue(months),
    });
}

export function useManualSetPro() {
    const qc = useQueryClient();
    return useMutation({
        mutationFn: (payload: ManualSetProRequest) => adminPaymentService.manualSetPro(payload),
        onSuccess: () => qc.invalidateQueries({ queryKey: queryKeys.subscriptions.all }),
    });
}
