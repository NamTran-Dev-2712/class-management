using ClassManagement.Application.Common.Constants;
using ClassManagement.Application.Interfaces.Identity;
using ClassManagement.Infrastructure.Security;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.RateLimiting;

namespace ClassManagement.Api.Controllers.Teacher;

[ApiController]
[Route("api/teacher/classes")]
[Authorize(Roles = ApplicationRoles.Teacher)]
public class TeacherClassesController : BaseApiController
{
    private readonly ISender _mediator;
    private readonly ICurrentUserService _currentUser;

    public TeacherClassesController(ISender mediator, ICurrentUserService currentUser)
    {
        _mediator = mediator;
        _currentUser = currentUser;
    }

    [HttpGet]
    public async Task<IActionResult> GetClasses(
        [FromQuery] GetTeacherClassesQuery query,
        CancellationToken cancellationToken
    )
    {
        var result = await _mediator.Send(query, cancellationToken);
        return ApiOk(result);
    }

    [HttpGet("{publicId:guid}", Name = "GetTeacherClass")]
    public async Task<IActionResult> GetClass(Guid publicId, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new GetClassDetailQuery(publicId, _currentUser.UserId),
            cancellationToken
        );
        return ApiOk(result);
    }

    [HttpPost]
    [EnableRateLimiting(RateLimitOptions.Policies.ClassWrite)]
    public async Task<IActionResult> Create(
        CreateClassCommand command,
        CancellationToken cancellationToken
    )
    {
        var publicId = await _mediator.Send(command, cancellationToken);
        return ApiCreated("GetTeacherClass", new { publicId }, new { publicId }, "Class.Created");
    }

    [HttpPut("{publicId:guid}")]
    [EnableRateLimiting(RateLimitOptions.Policies.ClassWrite)]
    public async Task<IActionResult> Update(
        Guid publicId,
        UpdateClassCommand command,
        CancellationToken cancellationToken
    )
    {
        await _mediator.Send(command with { PublicId = publicId }, cancellationToken);
        return ApiOk("Class.Updated");
    }

    [HttpPost("{publicId:guid}/archive")]
    [EnableRateLimiting(RateLimitOptions.Policies.ClassWrite)]
    public async Task<IActionResult> Archive(Guid publicId, CancellationToken cancellationToken)
    {
        await _mediator.Send(new ArchiveClassCommand(publicId, true), cancellationToken);
        return ApiOk("Class.Archived");
    }

    [HttpPost("{publicId:guid}/unarchive")]
    [EnableRateLimiting(RateLimitOptions.Policies.ClassWrite)]
    public async Task<IActionResult> Unarchive(Guid publicId, CancellationToken cancellationToken)
    {
        await _mediator.Send(new ArchiveClassCommand(publicId, false), cancellationToken);
        return ApiOk("Class.Unarchived");
    }

    [HttpPost("{publicId:guid}/invite-code/regenerate")]
    [EnableRateLimiting(RateLimitOptions.Policies.ClassWrite)]
    public async Task<IActionResult> RegenerateInviteCode(
        Guid publicId,
        CancellationToken cancellationToken
    )
    {
        var inviteCode = await _mediator.Send(
            new RegenerateInviteCodeCommand(publicId),
            cancellationToken
        );
        return ApiOk(new { inviteCode }, "Class.InviteCodeRegenerated");
    }

    [HttpGet("{publicId:guid}/members")]
    public async Task<IActionResult> GetMembers(
        Guid publicId,
        [FromQuery] GetClassMembersQuery query,
        CancellationToken cancellationToken
    )
    {
        var result = await _mediator.Send(
            query with
            {
                ClassPublicId = publicId,
            },
            cancellationToken
        );
        return ApiOk(result);
    }

    [HttpPost("{publicId:guid}/members/{membershipPublicId:guid}/approve")]
    [EnableRateLimiting(RateLimitOptions.Policies.ClassWrite)]
    public async Task<IActionResult> ApproveMember(
        Guid publicId,
        Guid membershipPublicId,
        CancellationToken cancellationToken
    )
    {
        await _mediator.Send(
            new ApproveMemberCommand(publicId, membershipPublicId),
            cancellationToken
        );
        return ApiOk("Class.MemberApproved");
    }

    [HttpPost("{publicId:guid}/members/{membershipPublicId:guid}/reject")]
    [EnableRateLimiting(RateLimitOptions.Policies.ClassWrite)]
    public async Task<IActionResult> RejectMember(
        Guid publicId,
        Guid membershipPublicId,
        [FromBody] RejectMemberCommand? command,
        CancellationToken cancellationToken
    )
    {
        await _mediator.Send(
            new RejectMemberCommand(publicId, membershipPublicId, command?.RejectionReason),
            cancellationToken
        );
        return ApiOk("Class.MemberRejected");
    }

    [HttpDelete("{publicId:guid}/members/{membershipPublicId:guid}")]
    [EnableRateLimiting(RateLimitOptions.Policies.ClassWrite)]
    public async Task<IActionResult> KickMember(
        Guid publicId,
        Guid membershipPublicId,
        CancellationToken cancellationToken
    )
    {
        await _mediator.Send(
            new KickMemberCommand(publicId, membershipPublicId),
            cancellationToken
        );
        return ApiOk("Class.MemberRemoved");
    }
}
