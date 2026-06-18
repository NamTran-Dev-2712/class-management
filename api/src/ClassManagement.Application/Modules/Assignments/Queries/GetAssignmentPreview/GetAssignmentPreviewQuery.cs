using ClassManagement.Application.Modules.Assignments.DTOs;

// Owning teacher previews a published assignment as a student would see it (no correct flags).
public record GetAssignmentPreviewQuery(Guid PublicId) : IRequest<AssignmentPreviewDto>;
