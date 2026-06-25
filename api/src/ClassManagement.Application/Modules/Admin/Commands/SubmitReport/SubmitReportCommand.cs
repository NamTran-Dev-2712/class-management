using ClassManagement.Domain.Modules.Admin.Enums;

// Any user reports a target (MVP-7). Returns the report's public id — for an existing active report of
// the same target by the same user, returns that one (idempotent, BR-7-02 / edge case).
public record SubmitReportCommand(
    ReportTargetType TargetType,
    Guid TargetPublicId,
    ReportReason Reason,
    string? Description
) : IRequest<Guid>;
