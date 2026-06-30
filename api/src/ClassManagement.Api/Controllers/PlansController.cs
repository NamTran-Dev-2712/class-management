using ClassManagement.Application.Common.Constants;
using ClassManagement.Infrastructure.Security;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.AspNetCore.RateLimiting;

namespace ClassManagement.Api.Controllers;

// Anonymous-readable list of active plans for the pricing page (MVP-8).
[ApiController]
[Route("api/plans")]
[AllowAnonymous]
public class PlansController : BaseApiController
{
    private readonly ISender _mediator;

    public PlansController(ISender mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    [EnableRateLimiting(RateLimitOptions.Policies.Read)]
    [OutputCache(PolicyName = OutputCachePolicies.PlansRead)]
    public async Task<IActionResult> GetPlans(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetPlansQuery(), cancellationToken);
        return ApiOk(result);
    }
}
