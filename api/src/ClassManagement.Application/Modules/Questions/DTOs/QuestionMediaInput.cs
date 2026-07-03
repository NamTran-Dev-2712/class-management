namespace ClassManagement.Application.Modules.Questions.DTOs;

/// <summary>
/// A media attachment supplied when creating/updating a question (MVP-9). References a Confirmed media
/// asset the teacher owns by its public id. <c>Role</c> distinguishes Markdown-inline references from
/// first-class attachments; <c>DisplayOrder</c> orders the attachment list.
/// </summary>
public sealed record QuestionMediaInput(Guid MediaPublicId, MediaRole Role, int DisplayOrder);
