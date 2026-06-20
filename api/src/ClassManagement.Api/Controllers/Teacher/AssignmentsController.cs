using ClassManagement.Application.Common.Constants;
using ClassManagement.Application.Interfaces.Localization;
using ClassManagement.Application.Modules.Assignments.Interfaces;
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
    private readonly IGradeExportService _exportService;
    private readonly ILocalizationService _localization;

    public TeacherAssignmentsController(
        ISender mediator,
        IGradeExportService exportService,
        ILocalizationService localization
    )
    {
        _mediator = mediator;
        _exportService = exportService;
        _localization = localization;
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

    // Full per-question breakdown of one attempt for manual grading (T6-03). Not cached (edit workflow).
    [HttpGet("attempts/{attemptId:guid}/grading")]
    [EnableRateLimiting(RateLimitOptions.Policies.Read)]
    public async Task<IActionResult> GetAttemptForGrading(
        Guid attemptId,
        CancellationToken cancellationToken
    )
    {
        var result = await _mediator.Send(
            new GetAttemptForGradingQuery(attemptId),
            cancellationToken
        );
        return ApiOk(result);
    }

    // Aggregate report (stats + histogram + per-student grades) for one assignment (T6-08).
    [HttpGet("{publicId:guid}/report")]
    [EnableRateLimiting(RateLimitOptions.Policies.Read)]
    [OutputCache(PolicyName = OutputCachePolicies.AssignmentReportRead)]
    public async Task<IActionResult> GetReport(Guid publicId, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new GetAssignmentReportQuery(publicId),
            cancellationToken
        );
        return ApiOk(result);
    }

    // CSV grade export (T6-09). Server-generated file; not cached.
    [HttpGet("{publicId:guid}/export")]
    [EnableRateLimiting(RateLimitOptions.Policies.Export)]
    public async Task<IActionResult> ExportGrades(
        Guid publicId,
        CancellationToken cancellationToken
    )
    {
        var report = await _mediator.Send(
            new GetAssignmentReportQuery(publicId),
            cancellationToken
        );

        var labels = new GradeExportLabels(
            _localization.Translate("Report.Student"),
            _localization.Translate("Report.Score"),
            _localization.Translate("Report.Total"),
            _localization.Translate("Report.Attempts"),
            _localization.Translate("Report.NotSubmitted"),
            _localization.Translate("Report.NotGraded")
        );

        var bytes = _exportService.BuildCsv(report, labels);
        return File(bytes, "text/csv", $"grades-{publicId}.csv");
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

    // Manually grade the writing answers of an attempt (T6-04). Batch upsert; transitions the attempt to
    // Graded once every writing question is scored.
    [HttpPost("attempts/{attemptId:guid}/grade")]
    [EnableRateLimiting(RateLimitOptions.Policies.GradeWrite)]
    public async Task<IActionResult> GradeAttempt(
        Guid attemptId,
        GradeAttemptCommand command,
        CancellationToken cancellationToken
    )
    {
        await _mediator.Send(command with { AttemptId = attemptId }, cancellationToken);
        await EvictCacheAsync(OutputCacheTags.Assignments, cancellationToken);
        return ApiOk("Attempt.Graded");
    }

    // Publish grades to students (T6-07, Manual policy). Sets grades_released_at.
    [HttpPost("{publicId:guid}/release-grades")]
    [EnableRateLimiting(RateLimitOptions.Policies.AssignmentWrite)]
    public async Task<IActionResult> ReleaseGrades(
        Guid publicId,
        CancellationToken cancellationToken
    )
    {
        await _mediator.Send(new ReleaseAssignmentGradesCommand(publicId), cancellationToken);
        await EvictCacheAsync(OutputCacheTags.Assignments, cancellationToken);
        return ApiOk("Assignment.GradesReleased");
    }
}
