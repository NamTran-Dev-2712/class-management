namespace ClassManagement.Domain.Modules.Admin.Enums;

// The kind of entity a report points at (MVP-7). Stored as PascalCase text. The matching internal id
// is resolved at submit time; there is no FK (polymorphic target).
public enum ReportTargetType
{
    Question,
    Exam,
    Assignment,
    Class,
    User,
}
