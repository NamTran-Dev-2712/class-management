using ClassManagement.Application.Common.Constants;
using ClassManagement.Application.Exceptions;

public class UpdateSubjectCommandHandler : IRequestHandler<UpdateSubjectCommand>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICacheService _cache;

    public UpdateSubjectCommandHandler(IUnitOfWork unitOfWork, ICacheService cache)
    {
        _unitOfWork = unitOfWork;
        _cache = cache;
    }

    public async Task Handle(UpdateSubjectCommand request, CancellationToken ct)
    {
        var repo = _unitOfWork.Repository<Subject>();
        var name = request.Name.Trim();

        var subject =
            await repo.GetFirstOrDefaultAsync(s => s.PublicId == request.PublicId)
            ?? throw new NotFoundException("Subject.NotFound");

        if (
            !string.Equals(subject.Name, name, StringComparison.Ordinal)
            && await repo.ExistsAsync(s => s.Name == name && s.PublicId != request.PublicId)
        )
            throw new ConflictException("Subject.NameExists");

        subject.Name = name;
        subject.Description = request.Description;
        subject.IsActive = request.IsActive;
        subject.DisplayOrder = request.DisplayOrder;

        repo.Update(subject);
        await _unitOfWork.SaveChangesAsync(ct);

        await _cache.RemoveAsync(CacheKeys.SubjectList(), ct);
        await _cache.RemoveAsync(CacheKeys.SubjectDetail(request.PublicId), ct);
    }
}
