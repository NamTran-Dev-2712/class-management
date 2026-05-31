using ClassManagement.Domain.Modules.Classroom.Enums;

namespace ClassManagement.Domain.Modules.Classroom.Entities;

// No UpdatedAt — status changes are tracked via processed_at; rejected-then-retry creates a NEW row
public sealed class ClassMembership : BaseEntity, IHasPublicId
{
    public Guid PublicId { get; set; }
    public long ClassId { get; set; }
    public long StudentId { get; set; }
    public MembershipStatus Status { get; set; } = MembershipStatus.Pending;
    public DateTime JoinedAt { get; set; }
    public DateTime? ProcessedAt { get; set; }
    public long? ProcessedBy { get; set; }
    public string? RejectionReason { get; set; }
}
