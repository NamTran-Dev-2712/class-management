namespace ClassManagement.Domain.Modules.Admin.Enums;

// Report workflow state (MVP-7). Stored as PascalCase text; see ReportConfiguration check constraint.
public enum ReportStatus
{
    Pending,
    Reviewing,
    Resolved,
    Rejected,
}
