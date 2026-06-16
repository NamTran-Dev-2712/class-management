using System.Linq.Expressions;
using ClassManagement.Application.Modules.Questions.DTOs;

// Shared SQL projection from the read-model view to the list DTO.
internal static class QuestionProjections
{
    public static readonly Expression<Func<QuestionView, QuestionListDto>> ToListDto =
        q => new QuestionListDto
        {
            PublicId = q.PublicId,
            Type = q.Type,
            Content = q.Content,
            Difficulty = q.Difficulty,
            SuggestedPoint = q.SuggestedPoint,
            Visibility = q.Visibility,
            SubjectPublicId = q.SubjectPublicId,
            SubjectName = q.SubjectName,
            TeacherPublicId = q.TeacherPublicId,
            TeacherName = q.TeacherName,
            OptionCount = q.OptionCount,
            Tags = q.Tags,
            CreatedAt = q.CreatedAt,
            UpdatedAt = q.UpdatedAt,
        };
}
