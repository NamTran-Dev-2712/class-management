using ClassManagement.Application.Common.Constants;
using ClassManagement.Infrastructure.Security;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.AspNetCore.RateLimiting;

namespace ClassManagement.Api.Controllers.Admin;

[ApiController]
[Route("api/admin/exams")]
[Authorize(Roles = ApplicationRoles.Admin)]
public class AdminExamsController : BaseApiController
{
    private readonly ISender _mediator;

    public AdminExamsController(ISender mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    [EnableRateLimiting(RateLimitOptions.Policies.Read)]
    [OutputCache(PolicyName = OutputCachePolicies.AdminExamsRead)]
    public async Task<IActionResult> GetExams(
        [FromQuery] GetAdminExamsQuery query,
        CancellationToken cancellationToken
    )
    {
        var result = await _mediator.Send(query, cancellationToken);
        return ApiOk(result);
    }

    [HttpGet("{publicId:guid}")]
    [EnableRateLimiting(RateLimitOptions.Policies.Read)]
    public async Task<IActionResult> GetExam(Guid publicId, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetAdminExamDetailQuery(publicId), cancellationToken);
        return ApiOk(result);
    }
}
