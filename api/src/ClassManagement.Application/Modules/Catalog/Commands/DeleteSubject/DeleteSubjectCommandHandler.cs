using ClassManagement.Application.Common.Constants;
using ClassManagement.Application.Exceptions;

public class DeleteSubjectCommandHandler : IRequestHandler<DeleteSubjectCommand>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICacheService _cache;

    public DeleteSubjectCommandHandler(IUnitOfWork unitOfWork, ICacheService cache)
    {
        _unitOfWork = unitOfWork;
        _cache = cache;
    }

    public async Task Handle(DeleteSubjectCommand request, CancellationToken ct)
    {
        var subject =
            await _unitOfWork.Subjects.GetByPublicIdAsync(request.PublicId, ct)
            ?? throw new NotFoundException("Subject.NotFound");

        // AuditableEntityInterceptor converts deletes of ISoftDeletable into DeletedAt = now.
        _unitOfWork.Subjects.Remove(subject);
        await _unitOfWork.SaveChangesAsync(ct);

        await _cache.RemoveAsync(CacheKeys.SubjectList(), ct);
        await _cache.RemoveAsync(CacheKeys.SubjectDetail(request.PublicId), ct);
    }
}
