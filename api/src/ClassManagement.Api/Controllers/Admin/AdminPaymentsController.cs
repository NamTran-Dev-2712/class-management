using ClassManagement.Application.Common.Constants;
using ClassManagement.Application.Modules.Payment.DTOs;
using ClassManagement.Infrastructure.Security;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.AspNetCore.RateLimiting;

namespace ClassManagement.Api.Controllers.Admin;

// Admin payment history + revenue (MVP-8 A8-03/A8-04). Read-only.
[ApiController]
[Route("api/admin/payments")]
[Authorize(Roles = ApplicationRoles.Admin)]
public class AdminPaymentsController : BaseApiController
{
    private readonly ISender _mediator;

    public AdminPaymentsController(ISender mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    [EnableRateLimiting(RateLimitOptions.Policies.Read)]
    [OutputCache(PolicyName = OutputCachePolicies.AdminPaymentsRead)]
    public async Task<IActionResult> List(
        [FromQuery] GetAdminPaymentsQuery query,
        CancellationToken cancellationToken
    )
    {
        var result = await _mediator.Send(query, cancellationToken);
        return ApiOk(result);
    }

    [HttpGet("revenue")]
    [EnableRateLimiting(RateLimitOptions.Policies.Read)]
    [OutputCache(PolicyName = OutputCachePolicies.AdminRevenueRead)]
    public async Task<IActionResult> Revenue(
        [FromQuery] int months,
        CancellationToken cancellationToken
    )
    {
        var result = await _mediator.Send(
            new GetRevenueSummaryQuery(months <= 0 ? 12 : months),
            cancellationToken
        );
        return ApiOk(result);
    }
}
