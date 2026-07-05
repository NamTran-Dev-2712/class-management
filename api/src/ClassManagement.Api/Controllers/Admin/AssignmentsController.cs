using ClassManagement.Application.Common.Constants;
using ClassManagement.Infrastructure.Security;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.AspNetCore.RateLimiting;

namespace ClassManagement.Api.Controllers.Admin;

[ApiController]
[Route("api/admin/assignments")]
[Authorize(Roles = ApplicationRoles.Admin)]
public class AdminAssignmentsController : BaseApiController
{
    private readonly ISender _mediator;

    public AdminAssignmentsController(ISender mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    [EnableRateLimiting(RateLimitOptions.Policies.Read)]
    [OutputCache(PolicyName = OutputCachePolicies.AdminAssignmentsRead)]
    public async Task<IActionResult> GetAssignments(
        [FromQuery] GetAdminAssignmentsQuery query,
        CancellationToken cancellationToken
    )
    {
        var result = await _mediator.Send(query, cancellationToken);
        return ApiOk(result);
    }

    [HttpGet("{publicId:guid}")]
    [EnableRateLimiting(RateLimitOptions.Policies.Read)]
    [OutputCache(PolicyName = OutputCachePolicies.AdminAssignmentsRead)]
    public async Task<IActionResult> GetAssignment(
        Guid publicId,
        CancellationToken cancellationToken
    )
    {
        var result = await _mediator.Send(
            new GetAdminAssignmentDetailQuery(publicId),
            cancellationToken
        );
        return ApiOk(result);
    }

    // Admin may view any assignment's report (permission matrix). OwnerScoped = false skips the owner
    // guard (role-gated by [Authorize(Admin)] above).
    [HttpGet("{publicId:guid}/report")]
    [EnableRateLimiting(RateLimitOptions.Policies.Read)]
    [OutputCache(PolicyName = OutputCachePolicies.AdminAssignmentReportRead)]
    public async Task<IActionResult> GetReport(Guid publicId, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new GetAssignmentReportQuery(publicId, OwnerScoped: false),
            cancellationToken
        );
        return ApiOk(result);
    }

    // Proctoring event timeline for any attempt, read-only (MVP-10, A10-01). Time-sensitive; not cached.
    [HttpGet("attempts/{attemptId:guid}/events")]
    [EnableRateLimiting(RateLimitOptions.Policies.Read)]
    public async Task<IActionResult> GetAttemptEvents(
        Guid attemptId,
        CancellationToken cancellationToken
    )
    {
        var result = await _mediator.Send(
            new GetAttemptEventsQuery(attemptId, OwnerScoped: false),
            cancellationToken
        );
        return ApiOk(result);
    }

    // ── Proctoring moderation (MVP-10, any attempt) ──────────────────────────────────────────

    [HttpPost("attempts/{attemptId:guid}/force-submit")]
    [EnableRateLimiting(RateLimitOptions.Policies.AdminAction)]
    public async Task<IActionResult> ForceSubmitAttempt(
        Guid attemptId,
        CancellationToken cancellationToken
    )
    {
        await _mediator.Send(
            new ForceSubmitAttemptCommand(attemptId, OwnerScoped: false),
            cancellationToken
        );
        await EvictCacheAsync(OutputCacheTags.Assignments, cancellationToken);
        return ApiOk("Attempt.ForceSubmitted");
    }

    [HttpPost("attempts/{attemptId:guid}/flag")]
    [EnableRateLimiting(RateLimitOptions.Policies.AdminAction)]
    public async Task<IActionResult> FlagAttempt(
        Guid attemptId,
        CancellationToken cancellationToken
    )
    {
        await _mediator.Send(
            new SetAttemptFlagCommand(attemptId, Flagged: true, OwnerScoped: false),
            cancellationToken
        );
        await EvictCacheAsync(OutputCacheTags.Assignments, cancellationToken);
        return ApiOk("Attempt.Flagged");
    }

    [HttpPost("attempts/{attemptId:guid}/unflag")]
    [EnableRateLimiting(RateLimitOptions.Policies.AdminAction)]
    public async Task<IActionResult> UnflagAttempt(
        Guid attemptId,
        CancellationToken cancellationToken
    )
    {
        await _mediator.Send(
            new SetAttemptFlagCommand(attemptId, Flagged: false, OwnerScoped: false),
            cancellationToken
        );
        await EvictCacheAsync(OutputCacheTags.Assignments, cancellationToken);
        return ApiOk("Attempt.Unflagged");
    }

    [HttpPost("attempts/{attemptId:guid}/unlock")]
    [EnableRateLimiting(RateLimitOptions.Policies.AdminAction)]
    public async Task<IActionResult> UnlockAttempt(
        Guid attemptId,
        CancellationToken cancellationToken
    )
    {
        await _mediator.Send(
            new UnlockAttemptCommand(attemptId, OwnerScoped: false),
            cancellationToken
        );
        await EvictCacheAsync(OutputCacheTags.Assignments, cancellationToken);
        return ApiOk("Attempt.Unlocked");
    }

    // Emergency force-close (A7-07): closes the assignment + auto-submits any in-progress attempts.
    [HttpPost("{publicId:guid}/force-close")]
    [EnableRateLimiting(RateLimitOptions.Policies.AdminAction)]
    public async Task<IActionResult> ForceClose(Guid publicId, CancellationToken cancellationToken)
    {
        await _mediator.Send(new ForceCloseAssignmentCommand(publicId), cancellationToken);
        await EvictCacheAsync(OutputCacheTags.Assignments, cancellationToken);
        return ApiOk("Assignment.ForceClosed");
    }
}
