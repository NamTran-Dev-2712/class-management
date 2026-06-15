using ClassManagement.Application.Exceptions;
using ClassManagement.Application.Modules.Classroom.DTOs;
using ClassManagement.Domain.Modules.Classroom.Enums;

public class GetStudentClassMembersQueryHandler
    : IRequestHandler<GetStudentClassMembersQuery, PaginatedResult<ClassMemberDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;

    public GetStudentClassMembersQueryHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUser
    )
    {
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
    }

    public async Task<PaginatedResult<ClassMemberDto>> Handle(
        GetStudentClassMembersQuery request,
        CancellationToken ct
    )
    {
        var studentId = _currentUser.UserId ?? throw new UnauthorizedException("Auth.Unauthorized");

        var cls =
            await _unitOfWork.Classes.GetByPublicIdAsync(request.ClassPublicId, ct)
            ?? throw new NotFoundException("Class.NotFound");

        // Only an approved member may see the roster (S2-04 / permission matrix).
        if (!await _unitOfWork.ClassMemberships.IsApprovedMemberAsync(cls.Id, studentId, ct))
            throw new ForbiddenException("Class.NotMember");

        var approved = MembershipStatus.Approved.ToString();
        var repo = _unitOfWork.Repository<ClassMemberView>();
        var query = repo.Query().Where(m => m.ClassId == cls.Id && m.Status == approved);

        return await ClassMemberPaging.RunAsync(repo, query, request, ct);
    }
}
