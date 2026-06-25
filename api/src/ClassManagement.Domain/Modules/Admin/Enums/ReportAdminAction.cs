namespace ClassManagement.Domain.Modules.Admin.Enums;

// The action an Admin took when resolving a report (MVP-7). Stored as PascalCase text.
public enum ReportAdminAction
{
    Dismiss,
    WarnUser,
    HideContent,
    DeleteContent,
    BanUser,
}
