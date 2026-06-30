import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";

import { queryKeys } from "@/lib/query-keys";
import { planService, teacherSubscriptionService } from "@/services/payment/payment.service";
import type { CheckoutRequest } from "@/services/payment/dtos/payment-dtos";

export function usePlans() {
    return useQuery({
        queryKey: queryKeys.plans.list(),
        queryFn: () => planService.list(),
    });
}

export function useMySubscription() {
    return useQuery({
        queryKey: queryKeys.subscriptions.mine(),
        queryFn: () => teacherSubscriptionService.mine(),
    });
}

export function useUsage() {
    return useQuery({
        queryKey: queryKeys.subscriptions.usage(),
        queryFn: () => teacherSubscriptionService.usage(),
    });
}

export function useInvoices() {
    return useQuery({
        queryKey: queryKeys.subscriptions.invoices(),
        queryFn: () => teacherSubscriptionService.invoices(),
    });
}

export function useCancelSubscription() {
    const qc = useQueryClient();
    return useMutation({
        mutationFn: () => teacherSubscriptionService.cancel(),
        onSuccess: () => qc.invalidateQueries({ queryKey: queryKeys.subscriptions.all }),
    });
}

export function useReactivateSubscription() {
    const qc = useQueryClient();
    return useMutation({
        mutationFn: () => teacherSubscriptionService.reactivate(),
        onSuccess: () => qc.invalidateQueries({ queryKey: queryKeys.subscriptions.all }),
    });
}

export function useCheckout() {
    return useMutation({
        mutationFn: (payload: CheckoutRequest) => teacherSubscriptionService.checkout(payload),
    });
}

/** Polls a payment's status while it's still Pending (checkout return page). */
export function usePaymentStatus(paymentId: string | null) {
    return useQuery({
        queryKey: queryKeys.subscriptions.paymentStatus(paymentId ?? ""),
        queryFn: () => teacherSubscriptionService.paymentStatus(paymentId!),
        enabled: !!paymentId,
        refetchInterval: (query) => (query.state.data?.status === "Pending" ? 3000 : false),
    });
}
