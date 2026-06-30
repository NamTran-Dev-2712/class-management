namespace ClassManagement.Application.Modules.Payment.DTOs;

/// <summary>One subscription in the admin subscription list (MVP-8 A8-01).</summary>
public sealed record AdminSubscriptionDto(
    Guid PublicId,
    Guid? TeacherPublicId,
    string? TeacherName,
    string? TeacherEmail,
    string PlanName,
    bool IsPro,
    string? BillingCycle,
    string Status,
    DateTime StartedAt,
    DateTime? ExpiresAt,
    DateTime? CancelledAt,
    DateTime? GracePeriodEndsAt,
    string PaymentType,
    string? AdminNote,
    DateTime CreatedAt
);

/// <summary>One payment in the admin payment history (MVP-8 A8-03).</summary>
public sealed record AdminPaymentDto(
    Guid PublicId,
    Guid? TeacherPublicId,
    string? TeacherName,
    string? TeacherEmail,
    string PlanName,
    string Provider,
    string BillingCycle,
    long AmountVnd,
    string Status,
    string? ProviderTransactionId,
    DateTime CreatedAt,
    DateTime? CompletedAt
);

/// <summary>Revenue summary for the admin dashboard (MVP-8 A8-04). Amounts in VND.</summary>
public sealed record RevenueSummaryDto(
    long TotalRevenueVnd,
    int CompletedPayments,
    IReadOnlyList<RevenueByMonthDto> ByMonth,
    IReadOnlyList<RevenueByPlanDto> ByPlan
);

/// <summary>Revenue for one calendar month (UTC), e.g. <c>Month = "2026-06"</c>.</summary>
public sealed record RevenueByMonthDto(string Month, long RevenueVnd, int Count);

public sealed record RevenueByPlanDto(string PlanName, long RevenueVnd, int Count);
