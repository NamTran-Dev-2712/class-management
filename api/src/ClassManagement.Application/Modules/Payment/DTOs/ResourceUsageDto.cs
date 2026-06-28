namespace ClassManagement.Application.Modules.Payment.DTOs;

/// <summary>A teacher's resource usage vs. their effective plan limits (MVP-8). Limit 0 = unlimited.</summary>
public sealed record ResourceUsageDto(
    string PlanName,
    bool IsPro,
    ResourceUsageItem Classes,
    ResourceUsageItem Questions,
    ResourceUsageItem Exams
);

/// <summary>Used count vs. limit for one resource. <c>Limit == 0</c> means unlimited.</summary>
public sealed record ResourceUsageItem(int Used, int Limit);
