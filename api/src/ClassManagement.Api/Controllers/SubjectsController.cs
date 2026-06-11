using ClassManagement.Application.Common.Constants;
using ClassManagement.Infrastructure.Security;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.RateLimiting;

[ApiController]
[Route("api/[controller]")]
public class SubjectsController : BaseApiController
{
    private readonly ISender _mediator;

    public SubjectsController(ISender mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> GetSubjects(
        [FromQuery] GetSubjectsQuery query,
        CancellationToken cancellationToken
    )
    {
        var result = await _mediator.Send(query, cancellationToken);
        return ApiOk(result);
    }

    [HttpGet("{publicId:guid}", Name = "GetSubjectById")]
    public async Task<IActionResult> GetSubject(Guid publicId, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetSubjectByIdQuery(publicId), cancellationToken);
        return ApiOk(result);
    }

    [HttpPost]
    [EnableRateLimiting(RateLimitOptions.Policies.SubjectWrite)]
    [Authorize(Roles = ApplicationRoles.Admin)]
    public async Task<IActionResult> Create(
        CreateSubjectCommand command,
        CancellationToken cancellationToken
    )
    {
        var publicId = await _mediator.Send(command, cancellationToken);
        return ApiCreated("GetSubjectById", new { publicId }, new { publicId }, "Subject.Created");
    }

    [HttpPut("{publicId:guid}")]
    [EnableRateLimiting(RateLimitOptions.Policies.SubjectWrite)]
    [Authorize(Roles = ApplicationRoles.Admin)]
    public async Task<IActionResult> Update(
        Guid publicId,
        UpdateSubjectCommand command,
        CancellationToken cancellationToken
    )
    {
        await _mediator.Send(command with { PublicId = publicId }, cancellationToken);
        return ApiOk("Subject.Updated");
    }

    [HttpDelete("{publicId:guid}")]
    [EnableRateLimiting(RateLimitOptions.Policies.SubjectWrite)]
    [Authorize(Roles = ApplicationRoles.Admin)]
    public async Task<IActionResult> Delete(Guid publicId, CancellationToken cancellationToken)
    {
        await _mediator.Send(new DeleteSubjectCommand(publicId), cancellationToken);
        return ApiOk("Subject.Deleted");
    }
}
