using ClassManagement.Application.Common.Constants;
using ClassManagement.Application.Exceptions;

public class GetSubjectByIdQueryHandler : IRequestHandler<GetSubjectByIdQuery, SubjectDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICacheService _cache;

    public GetSubjectByIdQueryHandler(IUnitOfWork unitOfWork, ICacheService cache)
    {
        _unitOfWork = unitOfWork;
        _cache = cache;
    }

    public async Task<SubjectDto> Handle(GetSubjectByIdQuery request, CancellationToken ct)
    {
        var cacheKey = CacheKeys.SubjectDetail(request.PublicId);

        var cached = await _cache.GetAsync<SubjectDto>(cacheKey, ct);
        if (cached is not null)
            return cached;

        var subject =
            await _unitOfWork.Subjects.GetByPublicIdAsync(request.PublicId, ct)
            ?? throw new NotFoundException("Subject.NotFound");

        var dto = new SubjectDto
        {
            PublicId = subject.PublicId,
            Name = subject.Name,
            Description = subject.Description,
            IsActive = subject.IsActive,
            DisplayOrder = subject.DisplayOrder,
            CreatedAt = subject.CreatedAt,
            UpdatedAt = subject.UpdatedAt,
        };

        await _cache.SetAsync(cacheKey, dto, ct: ct);
        return dto;
    }
}
