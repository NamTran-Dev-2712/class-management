using ClassManagement.Application.Common.Constants;
using ClassManagement.Infrastructure.Security;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.AspNetCore.RateLimiting;

namespace ClassManagement.Api.Controllers;

// Anonymous-readable app config (brand name + maintenance flag) for the frontend to consume on boot.
[ApiController]
[Route("api/public")]
[AllowAnonymous]
public class PublicConfigController : BaseApiController
{
    private readonly ISender _mediator;

    public PublicConfigController(ISender mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("app-config")]
    [EnableRateLimiting(RateLimitOptions.Policies.Read)]
    [OutputCache(PolicyName = OutputCachePolicies.PublicConfigRead)]
    public async Task<IActionResult> GetAppConfig(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetPublicConfigQuery(), cancellationToken);
        return ApiOk(result);
    }
}
