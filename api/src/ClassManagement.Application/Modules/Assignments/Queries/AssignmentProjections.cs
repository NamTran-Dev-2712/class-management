using System.Linq.Expressions;
using ClassManagement.Application.Modules.Assignments.DTOs;

// Shared SQL projection from the read-model view to the list DTO.
internal static class AssignmentProjections
{
    public static readonly Expression<Func<AssignmentView, AssignmentListDto>> ToListDto =
        a => new AssignmentListDto
        {
            PublicId = a.PublicId,
            Title = a.Title,
            Status = a.Status,
            OpensAt = a.OpensAt,
            ClosesAt = a.ClosesAt,
            TimeLimitMinutes = a.TimeLimitMinutes,
            MaxAttempts = a.MaxAttempts,
            TotalPoint = a.TotalPoint,
            TotalQuestions = a.TotalQuestions,
            ExamPublicId = a.ExamPublicId,
            ExamTitle = a.ExamTitle,
            ClassPublicId = a.ClassPublicId,
            ClassName = a.ClassName,
            TeacherPublicId = a.TeacherPublicId,
            TeacherName = a.TeacherName,
            AttemptCount = a.AttemptCount,
            SubmittedCount = a.SubmittedCount,
            CreatedAt = a.CreatedAt,
            UpdatedAt = a.UpdatedAt,
        };
}
