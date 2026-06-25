namespace ClassManagement.Domain.Modules.Admin.Enums;

// Why a user is reporting a target (MVP-7). Stored as PascalCase text to stay consistent with every
// other enum-as-string in the system (the schema doc's snake_case values are normalised here).
public enum ReportReason
{
    InappropriateContent,
    Spam,
    Copyright,
    IncorrectAnswer,
    Other,
}
