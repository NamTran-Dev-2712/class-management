namespace ClassManagement.Application.Modules.Exams.DTOs;

/// <summary>Row for teacher/public/admin exam lists. Read from <c>vw_exams</c>.</summary>
public sealed record ExamListDto
{
    public Guid PublicId { get; init; }
    public string Title { get; init; } = string.Empty;
    public string? Description { get; init; }
    public string Visibility { get; init; } = string.Empty;
    public int Version { get; init; }
    public decimal TotalPoint { get; init; }
    public int TotalQuestions { get; init; }
    public Guid? SubjectPublicId { get; init; }
    public string? SubjectName { get; init; }
    public Guid TeacherPublicId { get; init; }
    public string TeacherName { get; init; } = string.Empty;
    public List<string> Tags { get; init; } = [];
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }
}
