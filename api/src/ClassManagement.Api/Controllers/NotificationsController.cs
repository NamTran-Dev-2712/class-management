using ClassManagement.Application.Common.Constants;
using ClassManagement.Infrastructure.Security;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.AspNetCore.RateLimiting;

namespace ClassManagement.Api.Controllers;

// In-app notifications for the current user (MVP-7) — same surface for every authenticated role; all
// reads/writes are scoped to the caller in the handlers.
[ApiController]
[Route("api/notifications")]
[Authorize]
public class NotificationsController : BaseApiController
{
    private readonly ISender _mediator;

    public NotificationsController(ISender mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    [EnableRateLimiting(RateLimitOptions.Policies.Read)]
    [OutputCache(PolicyName = OutputCachePolicies.NotificationsRead)]
    public async Task<IActionResult> GetMyNotifications(
        [FromQuery] GetMyNotificationsQuery query,
        CancellationToken cancellationToken
    )
    {
        var result = await _mediator.Send(query, cancellationToken);
        return ApiOk(result);
    }

    [HttpGet("unread-count")]
    [EnableRateLimiting(RateLimitOptions.Policies.Read)]
    [OutputCache(PolicyName = OutputCachePolicies.NotificationUnreadCount)]
    public async Task<IActionResult> GetUnreadCount(CancellationToken cancellationToken)
    {
        var count = await _mediator.Send(new GetUnreadNotificationCountQuery(), cancellationToken);
        return ApiOk(new { count });
    }

    [HttpPost("{publicId:guid}/read")]
    [EnableRateLimiting(RateLimitOptions.Policies.NotificationWrite)]
    public async Task<IActionResult> MarkRead(Guid publicId, CancellationToken cancellationToken)
    {
        await _mediator.Send(new MarkNotificationReadCommand(publicId), cancellationToken);
        await EvictCacheAsync(OutputCacheTags.Notifications, cancellationToken);
        return ApiOk("Notification.Read");
    }

    [HttpPost("read-all")]
    [EnableRateLimiting(RateLimitOptions.Policies.NotificationWrite)]
    public async Task<IActionResult> MarkAllRead(CancellationToken cancellationToken)
    {
        await _mediator.Send(new MarkAllNotificationsReadCommand(), cancellationToken);
        await EvictCacheAsync(OutputCacheTags.Notifications, cancellationToken);
        return ApiOk("Notification.AllRead");
    }

    [HttpPost("{publicId:guid}/archive")]
    [EnableRateLimiting(RateLimitOptions.Policies.NotificationWrite)]
    public async Task<IActionResult> Archive(Guid publicId, CancellationToken cancellationToken)
    {
        await _mediator.Send(new ArchiveNotificationCommand(publicId), cancellationToken);
        await EvictCacheAsync(OutputCacheTags.Notifications, cancellationToken);
        return ApiOk("Notification.Archived");
    }
}
