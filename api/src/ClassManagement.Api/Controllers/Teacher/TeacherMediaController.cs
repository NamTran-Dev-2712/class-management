using ClassManagement.Application.Common.Constants;
using ClassManagement.Infrastructure.Security;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.AspNetCore.RateLimiting;

namespace ClassManagement.Api.Controllers.Teacher;

// Teacher media library + upload flow (MVP-9). Upload is presign → (client PUTs bytes straight to
// storage) → confirm; the API never receives the file bytes (BR-9-02). Admins may also upload/manage
// their own media (they own it), so this controller allows Teacher or Admin.
[ApiController]
[Route("api/teacher/media")]
[Authorize(Roles = $"{ApplicationRoles.Teacher},{ApplicationRoles.Admin}")]
public class TeacherMediaController : BaseApiController
{
    private readonly ISender _mediator;

    public TeacherMediaController(ISender mediator)
    {
        _mediator = mediator;
    }

    // ── Reads ────────────────────────────────────────────────────────────────────────────────

    [HttpGet]
    [EnableRateLimiting(RateLimitOptions.Policies.Read)]
    [OutputCache(PolicyName = OutputCachePolicies.TeacherMediaRead)]
    public async Task<IActionResult> GetMyMedia(
        [FromQuery] GetTeacherMediaQuery query,
        CancellationToken cancellationToken
    )
    {
        var result = await _mediator.Send(query, cancellationToken);
        return ApiOk(result);
    }

    [HttpGet("usage")]
    [EnableRateLimiting(RateLimitOptions.Policies.Read)]
    [OutputCache(PolicyName = OutputCachePolicies.MediaUsageRead)]
    public async Task<IActionResult> GetUsage(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetStorageUsageQuery(), cancellationToken);
        return ApiOk(result);
    }

    // ── Upload flow ──────────────────────────────────────────────────────────────────────────

    [HttpPost("presign")]
    [EnableRateLimiting(RateLimitOptions.Policies.MediaPresign)]
    public async Task<IActionResult> Presign(
        PresignUploadCommand command,
        CancellationToken cancellationToken
    )
    {
        var result = await _mediator.Send(command, cancellationToken);
        return ApiOk(result);
    }

    [HttpPost("{publicId:guid}/confirm")]
    [EnableRateLimiting(RateLimitOptions.Policies.MediaWrite)]
    public async Task<IActionResult> Confirm(Guid publicId, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new ConfirmUploadCommand(publicId), cancellationToken);
        await EvictCacheAsync(OutputCacheTags.Media, cancellationToken);
        return ApiOk(result, "Media.Confirmed");
    }

    [HttpDelete("{publicId:guid}")]
    [EnableRateLimiting(RateLimitOptions.Policies.MediaWrite)]
    public async Task<IActionResult> Delete(Guid publicId, CancellationToken cancellationToken)
    {
        await _mediator.Send(new DeleteMediaCommand(publicId), cancellationToken);
        await EvictCacheAsync(OutputCacheTags.Media, cancellationToken);
        // A deleted asset may be referenced by question attachments/options — refresh those reads too.
        await EvictCacheAsync(OutputCacheTags.Questions, cancellationToken);
        return ApiOk("Media.Deleted");
    }
}
