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
    [OutputCache(PolicyName = OutputCachePolicies.AssignmentReportRead)]
    public async Task<IActionResult> GetReport(Guid publicId, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new GetAssignmentReportQuery(publicId, OwnerScoped: false),
            cancellationToken
        );
        return ApiOk(result);
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
