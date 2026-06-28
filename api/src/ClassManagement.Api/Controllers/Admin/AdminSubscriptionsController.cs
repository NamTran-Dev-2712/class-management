using ClassManagement.Application.Common.Constants;
using ClassManagement.Application.Modules.Payment.DTOs;
using ClassManagement.Infrastructure.Security;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.AspNetCore.RateLimiting;

namespace ClassManagement.Api.Controllers.Admin;

// Admin subscription management (MVP-8 A8-01/A8-02): list all subscriptions + grant Pro manually.
[ApiController]
[Route("api/admin/subscriptions")]
[Authorize(Roles = ApplicationRoles.Admin)]
public class AdminSubscriptionsController : BaseApiController
{
    private readonly ISender _mediator;

    public AdminSubscriptionsController(ISender mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    [EnableRateLimiting(RateLimitOptions.Policies.Read)]
    [OutputCache(PolicyName = OutputCachePolicies.AdminSubscriptionsRead)]
    public async Task<IActionResult> List(
        [FromQuery] GetAdminSubscriptionsQuery query,
        CancellationToken cancellationToken
    )
    {
        var result = await _mediator.Send(query, cancellationToken);
        return ApiOk(result);
    }

    // Grant Pro to a teacher manually (BR-8-08).
    [HttpPost("manual-set")]
    [EnableRateLimiting(RateLimitOptions.Policies.AdminAction)]
    public async Task<IActionResult> ManualSet(
        SetManualProSubscriptionCommand command,
        CancellationToken cancellationToken
    )
    {
        await _mediator.Send(command, cancellationToken);
        await EvictCacheAsync(OutputCacheTags.Subscriptions, cancellationToken);
        return ApiOk("Subscription.ManualProSet");
    }
}
