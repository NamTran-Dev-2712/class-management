using ClassManagement.Application.Common.Constants;
using ClassManagement.Infrastructure.Security;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.AspNetCore.RateLimiting;

namespace ClassManagement.Api.Controllers.Admin;

[ApiController]
[Route("api/admin/questions")]
[Authorize(Roles = ApplicationRoles.Admin)]
public class AdminQuestionsController : BaseApiController
{
    private readonly ISender _mediator;

    public AdminQuestionsController(ISender mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    [EnableRateLimiting(RateLimitOptions.Policies.Read)]
    [OutputCache(PolicyName = OutputCachePolicies.AdminQuestionsRead)]
    public async Task<IActionResult> GetQuestions(
        [FromQuery] GetAdminQuestionsQuery query,
        CancellationToken cancellationToken
    )
    {
        var result = await _mediator.Send(query, cancellationToken);
        return ApiOk(result);
    }

    [HttpGet("{publicId:guid}")]
    [EnableRateLimiting(RateLimitOptions.Policies.Read)]
    public async Task<IActionResult> GetQuestion(Guid publicId, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new GetAdminQuestionDetailQuery(publicId),
            cancellationToken
        );
        return ApiOk(result);
    }
}
