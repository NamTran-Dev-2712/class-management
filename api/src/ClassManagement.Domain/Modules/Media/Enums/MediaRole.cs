namespace ClassManagement.Domain.Modules.Media.Enums;

// How a media asset is attached to a question (MVP-9). Inline = embedded in Markdown content/explanation
// (the URL lives in the text); Attachment = a first-class attachment listed at the question level.
// Stored as PascalCase text on question_media / snapshot_media.
public enum MediaRole
{
    Inline,
    Attachment,
}
