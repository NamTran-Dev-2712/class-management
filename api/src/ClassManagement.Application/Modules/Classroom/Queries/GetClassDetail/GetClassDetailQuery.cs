using ClassManagement.Application.Modules.Classroom.DTOs;

// Single-class detail. RestrictToOwnerId is set for teachers (owner-only, BR-2-04); null for admins
// who may view any class.
public record GetClassDetailQuery(Guid PublicId, long? RestrictToOwnerId)
    : IRequest<ClassDetailDto>;
