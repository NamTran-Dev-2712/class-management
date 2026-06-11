using ClassManagement.Application.Common.Constants;
using ClassManagement.Application.Exceptions;

public class CreateSubjectCommandHandler : IRequestHandler<CreateSubjectCommand, Guid>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICacheService _cache;

    public CreateSubjectCommandHandler(IUnitOfWork unitOfWork, ICacheService cache)
    {
        _unitOfWork = unitOfWork;
        _cache = cache;
    }

    public async Task<Guid> Handle(CreateSubjectCommand request, CancellationToken ct)
    {
        var repo = _unitOfWork.Repository<Subject>();
        var name = request.Name.Trim();

        // Uniqueness among active (non-deleted) rows — matches uq_subjects_name_active.
        if (await repo.ExistsAsync(s => s.Name == name))
            throw new ConflictException("Subject.NameExists");

        var subject = new Subject
        {
            Name = name,
            Description = request.Description,
            IsActive = request.IsActive,
            DisplayOrder = request.DisplayOrder,
        };

        await repo.AddAsync(subject, ct);
        await _unitOfWork.SaveChangesAsync(ct);
        await _cache.RemoveAsync(CacheKeys.SubjectList(), ct);

        return subject.PublicId;
    }
}
