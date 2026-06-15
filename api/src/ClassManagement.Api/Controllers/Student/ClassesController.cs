using ClassManagement.Application.Common.Constants;
using ClassManagement.Infrastructure.Security;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.AspNetCore.RateLimiting;

namespace ClassManagement.Api.Controllers.Student;

[ApiController]
[Route("api/student/classes")]
[Authorize(Roles = ApplicationRoles.Student)]
public class StudentClassesController : BaseApiController
{
    private readonly ISender _mediator;

    public StudentClassesController(ISender mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("join")]
    [EnableRateLimiting(RateLimitOptions.Policies.ClassJoin)]
    public async Task<IActionResult> Join(
        JoinClassCommand command,
        CancellationToken cancellationToken
    )
    {
        var classPublicId = await _mediator.Send(command, cancellationToken);
        await EvictCacheAsync(OutputCacheTags.Classrooms, cancellationToken);
        return ApiOk(new { classPublicId }, "Class.Joined");
    }

    [HttpGet]
    [EnableRateLimiting(RateLimitOptions.Policies.Read)]
    [OutputCache(PolicyName = OutputCachePolicies.StudentClassesRead)]
    public async Task<IActionResult> GetMyClasses(
        [FromQuery] GetStudentClassesQuery query,
        CancellationToken cancellationToken
    )
    {
        var result = await _mediator.Send(query, cancellationToken);
        return ApiOk(result);
    }

    [HttpGet("requests")]
    [EnableRateLimiting(RateLimitOptions.Policies.Read)]
    [OutputCache(PolicyName = OutputCachePolicies.StudentClassesRead)]
    public async Task<IActionResult> GetMyRequests(
        [FromQuery] GetStudentMembershipRequestsQuery query,
        CancellationToken cancellationToken
    )
    {
        var result = await _mediator.Send(query, cancellationToken);
        return ApiOk(result);
    }

    [HttpGet("{publicId:guid}/members")]
    [EnableRateLimiting(RateLimitOptions.Policies.Read)]
    [OutputCache(PolicyName = OutputCachePolicies.StudentClassesRead)]
    public async Task<IActionResult> GetMembers(
        Guid publicId,
        [FromQuery] GetStudentClassMembersQuery query,
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

    [HttpPost("{publicId:guid}/leave")]
    [EnableRateLimiting(RateLimitOptions.Policies.ClassWrite)]
    public async Task<IActionResult> Leave(Guid publicId, CancellationToken cancellationToken)
    {
        await _mediator.Send(new LeaveClassCommand(publicId), cancellationToken);
        await EvictCacheAsync(OutputCacheTags.Classrooms, cancellationToken);
        return ApiOk("Class.Left");
    }
}
