using ClassManagement.Application.Common.Constants;
using ClassManagement.Infrastructure.Security;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.RateLimiting;

namespace ClassManagement.Api.Controllers.Admin;

[ApiController]
[Route("api/admin/announcements")]
[Authorize(Roles = ApplicationRoles.Admin)]
public class AdminAnnouncementsController : BaseApiController
{
    private readonly ISender _mediator;

    public AdminAnnouncementsController(ISender mediator)
    {
        _mediator = mediator;
    }

    // Broadcast an announcement to all users or a specific role (A7-08).
    [HttpPost]
    [EnableRateLimiting(RateLimitOptions.Policies.AdminAction)]
    public async Task<IActionResult> Send(
        SendSystemAnnouncementCommand command,
        CancellationToken cancellationToken
    )
    {
        var recipients = await _mediator.Send(command, cancellationToken);
        await EvictCacheAsync(OutputCacheTags.Notifications, cancellationToken);
        return ApiOk(new { recipients }, "Announcement.Sent");
    }
}
