using ClassManagement.Application.Common.Constants;
using ClassManagement.Infrastructure.Security;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.AspNetCore.RateLimiting;

namespace ClassManagement.Api.Controllers.Admin;

[ApiController]
[Route("api/admin/reports")]
[Authorize(Roles = ApplicationRoles.Admin)]
public class AdminReportsController : BaseApiController
{
    private readonly ISender _mediator;

    public AdminReportsController(ISender mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    [EnableRateLimiting(RateLimitOptions.Policies.Read)]
    [OutputCache(PolicyName = OutputCachePolicies.AdminReportsRead)]
    public async Task<IActionResult> GetReports(
        [FromQuery] GetAdminReportsQuery query,
        CancellationToken cancellationToken
    )
    {
        var result = await _mediator.Send(query, cancellationToken);
        return ApiOk(result);
    }

    // Review + act on a report (Dismiss/Warn/Hide/Delete/Ban). A content action may hide/delete the
    // target, so evict the affected content caches alongside the reports cache.
    [HttpPost("{publicId:guid}/review")]
    [EnableRateLimiting(RateLimitOptions.Policies.AdminAction)]
    public async Task<IActionResult> Review(
        Guid publicId,
        ReviewReportBody body,
        CancellationToken cancellationToken
    )
    {
        await _mediator.Send(
            new ReviewReportCommand(publicId, body.Status, body.AdminAction, body.AdminNote),
            cancellationToken
        );

        await EvictCacheAsync(OutputCacheTags.Reports, cancellationToken);
        await EvictCacheAsync(OutputCacheTags.Questions, cancellationToken);
        await EvictCacheAsync(OutputCacheTags.Exams, cancellationToken);
        await EvictCacheAsync(OutputCacheTags.Classrooms, cancellationToken);
        await EvictCacheAsync(OutputCacheTags.Assignments, cancellationToken);
        await EvictCacheAsync(OutputCacheTags.Users, cancellationToken);
        return ApiOk("Report.Reviewed");
    }
}

// Request body for the review endpoint (publicId comes from the route).
public sealed record ReviewReportBody(
    Domain.Modules.Admin.Enums.ReportStatus Status,
    Domain.Modules.Admin.Enums.ReportAdminAction? AdminAction,
    string? AdminNote
);
