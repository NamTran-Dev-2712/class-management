using System.Linq.Expressions;
using ClassManagement.Application.Modules.Classroom.DTOs;

// Shared SQL projections from the read-model views to DTOs.
internal static class ClassProjections
{
    public static readonly Expression<Func<ClassView, ClassListDto>> ToListDto =
        c => new ClassListDto
        {
            PublicId = c.PublicId,
            Name = c.Name,
            Description = c.Description,
            SubjectName = c.SubjectName,
            Status = c.Status,
            CoverImageUrl = c.CoverImageUrl,
            OwnerName = c.OwnerName,
            ApprovedMemberCount = c.ApprovedMemberCount,
            PendingCount = c.PendingCount,
            CreatedAt = c.CreatedAt,
            UpdatedAt = c.UpdatedAt,
        };

    public static readonly Expression<Func<ClassMemberView, ClassMemberDto>> ToMemberDto =
        m => new ClassMemberDto
        {
            PublicId = m.PublicId,
            StudentPublicId = m.StudentPublicId,
            StudentName = m.StudentName,
            StudentEmail = m.StudentEmail,
            Status = m.Status,
            JoinedAt = m.JoinedAt,
            ProcessedAt = m.ProcessedAt,
            RejectionReason = m.RejectionReason,
        };
}
