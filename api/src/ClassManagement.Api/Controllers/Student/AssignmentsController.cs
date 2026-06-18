using ClassManagement.Application.Common.Constants;
using ClassManagement.Infrastructure.Security;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.AspNetCore.RateLimiting;

namespace ClassManagement.Api.Controllers.Student;

[ApiController]
[Route("api/student/assignments")]
[Authorize(Roles = ApplicationRoles.Student)]
public class StudentAssignmentsController : BaseApiController
{
    private readonly ISender _mediator;

    public StudentAssignmentsController(ISender mediator)
    {
        _mediator = mediator;
    }

    // ── Reads ────────────────────────────────────────────────────────────────────────────────

    [HttpGet]
    [EnableRateLimiting(RateLimitOptions.Policies.Read)]
    [OutputCache(PolicyName = OutputCachePolicies.StudentAssignmentsRead)]
    public async Task<IActionResult> GetMyAssignments(
        [FromQuery] GetStudentAssignmentsQuery query,
        CancellationToken cancellationToken
    )
    {
        var result = await _mediator.Send(query, cancellationToken);
        return ApiOk(result);
    }

    // Not output-cached: CanStart + the open/close window are time-sensitive and per-student.
    [HttpGet("{publicId:guid}")]
    [EnableRateLimiting(RateLimitOptions.Policies.Read)]
    public async Task<IActionResult> GetAssignment(
        Guid publicId,
        CancellationToken cancellationToken
    )
    {
        var result = await _mediator.Send(
            new GetStudentAssignmentDetailQuery(publicId),
            cancellationToken
        );
        return ApiOk(result);
    }

    [HttpGet("attempts")]
    [EnableRateLimiting(RateLimitOptions.Policies.Read)]
    [OutputCache(PolicyName = OutputCachePolicies.AttemptsRead)]
    public async Task<IActionResult> GetMyAttempts(
        [FromQuery] GetMyAttemptsQuery query,
        CancellationToken cancellationToken
    )
    {
        var result = await _mediator.Send(query, cancellationToken);
        return ApiOk(result);
    }

    // Live test-taking session — NEVER cached (server-authoritative remaining time + draft answers).
    [HttpGet("attempts/{attemptId:guid}/taking")]
    [EnableRateLimiting(RateLimitOptions.Policies.Read)]
    public async Task<IActionResult> GetAttemptForTaking(
        Guid attemptId,
        CancellationToken cancellationToken
    )
    {
        var result = await _mediator.Send(
            new GetAttemptForTakingQuery(attemptId),
            cancellationToken
        );
        return ApiOk(result);
    }

    // Attempt result — NEVER cached (score visibility is gated by the grade-publish policy).
    [HttpGet("attempts/{attemptId:guid}/result")]
    [EnableRateLimiting(RateLimitOptions.Policies.Read)]
    public async Task<IActionResult> GetAttemptResult(
        Guid attemptId,
        CancellationToken cancellationToken
    )
    {
        var result = await _mediator.Send(
            new GetMyAttemptResultQuery(attemptId),
            cancellationToken
        );
        return ApiOk(result);
    }

    // ── Writes ───────────────────────────────────────────────────────────────────────────────

    [HttpPost("{publicId:guid}/attempts")]
    [EnableRateLimiting(RateLimitOptions.Policies.AttemptStart)]
    public async Task<IActionResult> Start(Guid publicId, CancellationToken cancellationToken)
    {
        var command = new StartAttemptCommand(publicId)
        {
            IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
            UserAgent = Request.Headers.UserAgent.ToString(),
        };
        var attemptId = await _mediator.Send(command, cancellationToken);
        await EvictCacheAsync(OutputCacheTags.Assignments, cancellationToken);
        return ApiOk(new { attemptId }, "Attempt.Started");
    }

    // Auto-save: deliberately does NOT evict the list caches (drafts don't appear in any list).
    [HttpPatch("attempts/{attemptId:guid}/answers")]
    [EnableRateLimiting(RateLimitOptions.Policies.AttemptSave)]
    public async Task<IActionResult> SaveAnswers(
        Guid attemptId,
        SaveAttemptAnswersCommand command,
        CancellationToken cancellationToken
    )
    {
        await _mediator.Send(command with { AttemptId = attemptId }, cancellationToken);
        return ApiOk("Attempt.Saved");
    }

    [HttpPost("attempts/{attemptId:guid}/submit")]
    [EnableRateLimiting(RateLimitOptions.Policies.AttemptSubmit)]
    public async Task<IActionResult> Submit(Guid attemptId, CancellationToken cancellationToken)
    {
        await _mediator.Send(new SubmitAttemptCommand(attemptId), cancellationToken);
        await EvictCacheAsync(OutputCacheTags.Assignments, cancellationToken);
        return ApiOk("Attempt.Submitted");
    }
}
