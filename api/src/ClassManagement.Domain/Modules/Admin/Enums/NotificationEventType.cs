namespace ClassManagement.Domain.Modules.Admin.Enums;

// The system event that produced a notification (MVP-7). Stored as PascalCase text. Payment/subscription
// events are reserved for MVP-8 but allowed in the schema now so it does not need to change later.
public enum NotificationEventType
{
    ClassJoinApproved,
    ClassJoinRejected,
    AssignmentCreated,
    AssignmentDueSoon,
    AssignmentClosed,
    GradePublished,
    PendingGradingReminder,
    ReportResolved,
    ReportReceived,
    SystemAnnouncement,
    SubscriptionExpiringSoon,
    SubscriptionExpired,
    PaymentSucceeded,
    PaymentFailed,
}
