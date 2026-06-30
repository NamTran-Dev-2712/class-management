namespace ClassManagement.Application.Modules.Payment.DTOs;

/// <summary>
/// A subscription plan as shown on the pricing page (MVP-8). <c>BillingCycle</c> is the enum name
/// (<c>Monthly</c>/<c>Annual</c>) or null for Free; a null resource limit means unlimited. Feature
/// labels are i18n keys the frontend localizes.
/// </summary>
public sealed record PlanDto(
    Guid PublicId,
    string Name,
    string? BillingCycle,
    long PriceVnd,
    int? MaxClasses,
    int? MaxQuestions,
    int? MaxExams,
    int? MaxStudentsPerClass,
    bool IsActive,
    int DisplayOrder,
    IReadOnlyList<string> Features
);
