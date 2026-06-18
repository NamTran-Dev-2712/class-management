using ClassManagement.Application.Common.Constants;
using ClassManagement.Infrastructure.Security;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.AspNetCore.RateLimiting;

namespace ClassManagement.Api.Controllers.Teacher;

[ApiController]
[Route("api/teacher/assignments")]
[Authorize(Roles = ApplicationRoles.Teacher)]
public class TeacherAssignmentsController : BaseApiController
{
    private readonly ISender _mediator;

    public TeacherAssignmentsController(ISender mediator)
    {
        _mediator = mediator;
    }

    // ── Reads ────────────────────────────────────────────────────────────────────────────────

    [HttpGet]
    [EnableRateLimiting(RateLimitOptions.Policies.Read)]
    [OutputCache(PolicyName = OutputCachePolicies.TeacherAssignmentsRead)]
    public async Task<IActionResult> GetMyAssignments(
        [FromQuery] GetTeacherAssignmentsQuery query,
        CancellationToken cancellationToken
    )
    {
        var result = await _mediator.Send(query, cancellationToken);
        return ApiOk(result);
    }

    [HttpGet("{publicId:guid}", Name = "GetTeacherAssignment")]
    [EnableRateLimiting(RateLimitOptions.Policies.Read)]
    public async Task<IActionResult> GetAssignment(
        Guid publicId,
        CancellationToken cancellationToken
    )
    {
        var result = await _mediator.Send(
            new GetAssignmentDetailQuery(publicId),
            cancellationToken
        );
        return ApiOk(result);
    }

    [HttpGet("{publicId:guid}/preview")]
    [EnableRateLimiting(RateLimitOptions.Policies.Read)]
    public async Task<IActionResult> PreviewAssignment(
        Guid publicId,
        CancellationToken cancellationToken
    )
    {
        var result = await _mediator.Send(
            new GetAssignmentPreviewQuery(publicId),
            cancellationToken
        );
        return ApiOk(result);
    }

    [HttpGet("{publicId:guid}/attempts")]
    [EnableRateLimiting(RateLimitOptions.Policies.Read)]
    [OutputCache(PolicyName = OutputCachePolicies.AttemptsRead)]
    public async Task<IActionResult> GetAttempts(
        Guid publicId,
        [FromQuery] GetAssignmentAttemptsQuery query,
        CancellationToken cancellationToken
    )
    {
        var result = await _mediator.Send(
            query with
            {
                AssignmentId = publicId,
            },
            cancellationToken
        );
        return ApiOk(result);
    }

    [HttpGet("attempts/{attemptId:guid}")]
    [EnableRateLimiting(RateLimitOptions.Policies.Read)]
    public async Task<IActionResult> GetAttempt(Guid attemptId, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new GetTeacherAttemptDetailQuery(attemptId),
            cancellationToken
        );
        return ApiOk(result);
    }

    // ── Writes ───────────────────────────────────────────────────────────────────────────────

    [HttpPost]
    [EnableRateLimiting(RateLimitOptions.Policies.AssignmentWrite)]
    public async Task<IActionResult> Create(
        CreateAssignmentCommand command,
        CancellationToken cancellationToken
    )
    {
        var publicId = await _mediator.Send(command, cancellationToken);
        await EvictCacheAsync(OutputCacheTags.Assignments, cancellationToken);
        return ApiCreated(
            "GetTeacherAssignment",
            new { publicId },
            new { publicId },
            "Assignment.Created"
        );
    }

    [HttpPut("{publicId:guid}")]
    [EnableRateLimiting(RateLimitOptions.Policies.AssignmentWrite)]
    public async Task<IActionResult> Update(
        Guid publicId,
        UpdateAssignmentCommand command,
        CancellationToken cancellationToken
    )
    {
        await _mediator.Send(command with { PublicId = publicId }, cancellationToken);
        await EvictCacheAsync(OutputCacheTags.Assignments, cancellationToken);
        return ApiOk("Assignment.Updated");
    }

    [HttpPost("{publicId:guid}/publish")]
    [EnableRateLimiting(RateLimitOptions.Policies.AssignmentWrite)]
    public async Task<IActionResult> Publish(Guid publicId, CancellationToken cancellationToken)
    {
        await _mediator.Send(new PublishAssignmentCommand(publicId), cancellationToken);
        await EvictCacheAsync(OutputCacheTags.Assignments, cancellationToken);
        return ApiOk("Assignment.Published");
    }

    [HttpPost("{publicId:guid}/close")]
    [EnableRateLimiting(RateLimitOptions.Policies.AssignmentWrite)]
    public async Task<IActionResult> Close(Guid publicId, CancellationToken cancellationToken)
    {
        await _mediator.Send(new CloseAssignmentCommand(publicId), cancellationToken);
        await EvictCacheAsync(OutputCacheTags.Assignments, cancellationToken);
        return ApiOk("Assignment.Closed");
    }

    [HttpPost("{publicId:guid}/archive")]
    [EnableRateLimiting(RateLimitOptions.Policies.AssignmentWrite)]
    public async Task<IActionResult> Archive(Guid publicId, CancellationToken cancellationToken)
    {
        await _mediator.Send(new ArchiveAssignmentCommand(publicId), cancellationToken);
        await EvictCacheAsync(OutputCacheTags.Assignments, cancellationToken);
        return ApiOk("Assignment.Archived");
    }

    [HttpDelete("{publicId:guid}")]
    [EnableRateLimiting(RateLimitOptions.Policies.AssignmentWrite)]
    public async Task<IActionResult> Delete(Guid publicId, CancellationToken cancellationToken)
    {
        await _mediator.Send(new DeleteAssignmentCommand(publicId), cancellationToken);
        await EvictCacheAsync(OutputCacheTags.Assignments, cancellationToken);
        return ApiOk("Assignment.Deleted");
    }
}
