using ClassManagement.Application.Common.Constants;
using ClassManagement.Infrastructure.Security;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.AspNetCore.RateLimiting;

namespace ClassManagement.Api.Controllers.Admin;

// Admin storage overview + moderation (MVP-9, A9-01/A9-02). Read-only lists plus the ability to remove
// offending media (reuses the shared DeleteMedia command, which lets an admin target any owner's asset).
[ApiController]
[Route("api/admin/media")]
[Authorize(Roles = ApplicationRoles.Admin)]
public class AdminMediaController : BaseApiController
{
    private readonly ISender _mediator;

    public AdminMediaController(ISender mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    [EnableRateLimiting(RateLimitOptions.Policies.Read)]
    [OutputCache(PolicyName = OutputCachePolicies.AdminMediaRead)]
    public async Task<IActionResult> GetAll(
        [FromQuery] GetAdminMediaQuery query,
        CancellationToken cancellationToken
    )
    {
        var result = await _mediator.Send(query, cancellationToken);
        return ApiOk(result);
    }

    [HttpGet("overview")]
    [EnableRateLimiting(RateLimitOptions.Policies.Read)]
    [OutputCache(PolicyName = OutputCachePolicies.AdminStorageOverviewRead)]
    public async Task<IActionResult> GetOverview(
        [FromQuery] GetAdminStorageOverviewQuery query,
        CancellationToken cancellationToken
    )
    {
        var result = await _mediator.Send(query, cancellationToken);
        return ApiOk(result);
    }

    [HttpDelete("{publicId:guid}")]
    [EnableRateLimiting(RateLimitOptions.Policies.AdminAction)]
    public async Task<IActionResult> Delete(Guid publicId, CancellationToken cancellationToken)
    {
        await _mediator.Send(new DeleteMediaCommand(publicId), cancellationToken);
        await EvictCacheAsync(OutputCacheTags.Media, cancellationToken);
        await EvictCacheAsync(OutputCacheTags.Questions, cancellationToken);
        return ApiOk("Media.Deleted");
    }
}
