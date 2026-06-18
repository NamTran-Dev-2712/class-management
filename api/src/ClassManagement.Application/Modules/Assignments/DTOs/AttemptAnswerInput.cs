namespace ClassManagement.Application.Modules.Assignments.DTOs;

/// <summary>
/// One answer to auto-save. <see cref="QuestionId"/> is a snapshot question's public id;
/// <see cref="SelectedOptionIds"/> are snapshot option public ids (choice questions);
/// <see cref="TextAnswer"/> is the writing answer. Unset fields are treated as "no answer".
/// </summary>
public sealed record AttemptAnswerInput
{
    public Guid QuestionId { get; init; }
    public List<Guid> SelectedOptionIds { get; init; } = [];
    public string? TextAnswer { get; init; }
}
