using ClassManagement.Application.Common.Constants;
using ClassManagement.Infrastructure.Security;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.AspNetCore.RateLimiting;

namespace ClassManagement.Api.Controllers.Admin;

[ApiController]
[Route("api/admin/audit-logs")]
[Authorize(Roles = ApplicationRoles.Admin)]
public class AdminAuditLogsController : BaseApiController
{
    private readonly ISender _mediator;

    public AdminAuditLogsController(ISender mediator)
    {
        _mediator = mediator;
    }

    // Read-only audit trail for investigation (A7-05). Filterable by action/target/actor/date range.
    [HttpGet]
    [EnableRateLimiting(RateLimitOptions.Policies.Read)]
    [OutputCache(PolicyName = OutputCachePolicies.AdminAuditLogsRead)]
    public async Task<IActionResult> GetAuditLogs(
        [FromQuery] GetAuditLogsQuery query,
        CancellationToken cancellationToken
    )
    {
        var result = await _mediator.Send(query, cancellationToken);
        return ApiOk(result);
    }
}
