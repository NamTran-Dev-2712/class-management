using ClassManagement.Application.Exceptions;
using ClassManagement.Application.Modules.Assignments.DTOs;
using ClassManagement.Domain.Modules.Assignments.Entities;

public class GetAttemptEventsQueryHandler
    : IRequestHandler<GetAttemptEventsQuery, IReadOnlyList<AttemptEventDto>>
{
    // Bound the evidence list so a pathological attempt can't return unbounded rows.
    private const int MaxEvents = 1000;

    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;

    public GetAttemptEventsQueryHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUser)
    {
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
    }

    public async Task<IReadOnlyList<AttemptEventDto>> Handle(
        GetAttemptEventsQuery request,
        CancellationToken ct
    )
    {
        var attempt =
            await _unitOfWork.Attempts.GetByPublicIdAsync(request.AttemptId, false, ct)
            ?? throw new NotFoundException("Attempt.NotFound");

        if (request.OwnerScoped)
        {
            var avRepo = _unitOfWork.Repository<AssignmentView>();
            var ownerId = (
                await avRepo.ToListAsync(
                    avRepo
                        .Query()
                        .Where(a => a.Id == attempt.AssignmentId)
                        .Select(a => a.TeacherId)
                        .Take(1),
                    ct
                )
            ).FirstOrDefault();

            if (ownerId != _currentUser.UserId)
                throw new ForbiddenException("Assignment.NotOwner");
        }

        var repo = _unitOfWork.Repository<AttemptEvent>();
        var events = await repo.ToListAsync(
            repo.Query()
                .Where(e => e.AttemptId == attempt.Id)
                .OrderBy(e => e.OccurredAt)
                .ThenBy(e => e.Id)
                .Take(MaxEvents)
                .Select(e => new AttemptEventDto
                {
                    EventType = e.EventType,
                    OccurredAt = e.OccurredAt,
                    CreatedAt = e.CreatedAt,
                    Metadata = e.Metadata,
                }),
            ct
        );

        return events;
    }
}
