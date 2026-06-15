using ClassManagement.Application.Modules.Classroom.DTOs;

public class GetClassMembersQueryHandler
    : IRequestHandler<GetClassMembersQuery, PaginatedResult<ClassMemberDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;

    public GetClassMembersQueryHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUser)
    {
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
    }

    public async Task<PaginatedResult<ClassMemberDto>> Handle(
        GetClassMembersQuery request,
        CancellationToken ct
    )
    {
        var cls = ClassroomGuard.EnsureOwned(
            await _unitOfWork.Classes.GetByPublicIdAsync(request.ClassPublicId, ct),
            _currentUser.UserId
        );

        var repo = _unitOfWork.Repository<ClassMemberView>();
        var query = repo.Query().Where(m => m.ClassId == cls.Id);

        if (request.Status.HasValue)
        {
            var status = request.Status.Value.ToString();
            query = query.Where(m => m.Status == status);
        }

        return await ClassMemberPaging.RunAsync(repo, query, request, ct);
    }
}
