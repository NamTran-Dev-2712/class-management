using ClassManagement.Application.Modules.Assignments.DTOs;

// Proctoring event timeline for one attempt (MVP-10). OwnerScoped = true → teacher path (must own the
// assignment); false → admin path (any attempt, read-only). AttemptId comes from the route.
public record GetAttemptEventsQuery(Guid AttemptId, bool OwnerScoped)
    : IRequest<IReadOnlyList<AttemptEventDto>>;
