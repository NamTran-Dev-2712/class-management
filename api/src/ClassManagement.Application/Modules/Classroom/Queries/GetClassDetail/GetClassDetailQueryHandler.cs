using ClassManagement.Application.Exceptions;
using ClassManagement.Application.Modules.Classroom.DTOs;

public class GetClassDetailQueryHandler : IRequestHandler<GetClassDetailQuery, ClassDetailDto>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetClassDetailQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<ClassDetailDto> Handle(GetClassDetailQuery request, CancellationToken ct)
    {
        var repo = _unitOfWork.Repository<ClassView>();
        var query = repo.Query().Where(c => c.PublicId == request.PublicId).Take(1);

        var view =
            (await repo.ToListAsync(query, ct)).FirstOrDefault()
            ?? throw new NotFoundException("Class.NotFound");

        if (request.RestrictToOwnerId.HasValue && view.OwnerId != request.RestrictToOwnerId.Value)
            throw new ForbiddenException("Class.NotOwner");

        return new ClassDetailDto
        {
            PublicId = view.PublicId,
            Name = view.Name,
            Description = view.Description,
            SubjectId = view.SubjectPublicId,
            SubjectName = view.SubjectName,
            InviteCode = view.InviteCode,
            Status = view.Status,
            CoverImageUrl = view.CoverImageUrl,
            OwnerName = view.OwnerName,
            OwnerEmail = view.OwnerEmail,
            ApprovedMemberCount = view.ApprovedMemberCount,
            PendingCount = view.PendingCount,
            CreatedAt = view.CreatedAt,
            UpdatedAt = view.UpdatedAt,
        };
    }
}
