using ClassManagement.Application.Common.Constants;
using ClassManagement.Infrastructure.Security;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.AspNetCore.RateLimiting;

namespace ClassManagement.Api.Controllers;

// User-facing reporting (MVP-7) — any authenticated role may submit a report and view their own.
[ApiController]
[Route("api/reports")]
[Authorize]
public class ReportsController : BaseApiController
{
    private readonly ISender _mediator;

    public ReportsController(ISender mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    [EnableRateLimiting(RateLimitOptions.Policies.ReportWrite)]
    public async Task<IActionResult> Submit(
        SubmitReportCommand command,
        CancellationToken cancellationToken
    )
    {
        var publicId = await _mediator.Send(command, cancellationToken);
        await EvictCacheAsync(OutputCacheTags.Reports, cancellationToken);
        return ApiOk(new { publicId }, "Report.Submitted");
    }

    [HttpGet("mine")]
    [EnableRateLimiting(RateLimitOptions.Policies.Read)]
    [OutputCache(PolicyName = OutputCachePolicies.MyReportsRead)]
    public async Task<IActionResult> GetMine(
        [FromQuery] GetMyReportsQuery query,
        CancellationToken cancellationToken
    )
    {
        var result = await _mediator.Send(query, cancellationToken);
        return ApiOk(result);
    }
}
