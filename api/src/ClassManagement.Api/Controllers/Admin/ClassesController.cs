using ClassManagement.Application.Common.Constants;
using MediatR;
using Microsoft.AspNetCore.Authorization;

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
    public async Task<IActionResult> GetClasses(
        [FromQuery] GetAdminClassesQuery query,
        CancellationToken cancellationToken
    )
    {
        var result = await _mediator.Send(query, cancellationToken);
        return ApiOk(result);
    }

    [HttpGet("{publicId:guid}")]
    public async Task<IActionResult> GetClass(Guid publicId, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new GetClassDetailQuery(publicId, null),
            cancellationToken
        );
        return ApiOk(result);
    }
}
