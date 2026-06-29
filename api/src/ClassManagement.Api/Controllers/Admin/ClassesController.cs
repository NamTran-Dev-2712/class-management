using ClassManagement.Application.Common.Constants;
using ClassManagement.Infrastructure.Security;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.AspNetCore.RateLimiting;

namespace ClassManagement.Api.Controllers.Admin;

[ApiController]
[Route("api/admin/classes")]
[Authorize(Roles = ApplicationRoles.Admin)]
public class AdminClassesController : BaseApiController
{
    private readonly ISender _mediator;

    public AdminClassesController(ISender mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    [EnableRateLimiting(RateLimitOptions.Policies.Read)]
    [OutputCache(PolicyName = OutputCachePolicies.AdminClassesRead)]
    public async Task<IActionResult> GetClasses(
        [FromQuery] GetAdminClassesQuery query,
        CancellationToken cancellationToken
    )
    {
        var result = await _mediator.Send(query, cancellationToken);
        return ApiOk(result);
    }

    [HttpGet("{publicId:guid}")]
    [EnableRateLimiting(RateLimitOptions.Policies.Read)]
    [OutputCache(PolicyName = OutputCachePolicies.AdminClassesRead)]
    public async Task<IActionResult> GetClass(Guid publicId, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new GetClassDetailQuery(publicId, null),
            cancellationToken
        );
        return ApiOk(result);
    }
}
