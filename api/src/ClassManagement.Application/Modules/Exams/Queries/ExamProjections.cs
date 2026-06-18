using System.Linq.Expressions;
using ClassManagement.Application.Modules.Exams.DTOs;

// Shared SQL projection from the read-model view to the list DTO.
internal static class ExamProjections
{
    public static readonly Expression<Func<ExamView, ExamListDto>> ToListDto = e => new ExamListDto
    {
        PublicId = e.PublicId,
        Title = e.Title,
        Description = e.Description,
        Visibility = e.Visibility,
        Version = e.Version,
        TotalPoint = e.TotalPoint,
        TotalQuestions = e.TotalQuestions,
        SubjectPublicId = e.SubjectPublicId,
        SubjectName = e.SubjectName,
        TeacherPublicId = e.TeacherPublicId,
        TeacherName = e.TeacherName,
        Tags = e.Tags,
        CreatedAt = e.CreatedAt,
        UpdatedAt = e.UpdatedAt,
    };
}
