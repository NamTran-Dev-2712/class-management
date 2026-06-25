using ClassManagement.Domain.Modules.Admin.Enums;

// Admin reviews a report (A7-04). Moving to Reviewing just claims it; moving to Resolved applies the
// chosen action (Dismiss/Warn/Hide/Delete/Ban); Rejected dismisses it. The reporter is notified on a
// terminal decision.
public record ReviewReportCommand(
    Guid PublicId,
    ReportStatus Status,
    ReportAdminAction? AdminAction,
    string? AdminNote
) : IRequest;
