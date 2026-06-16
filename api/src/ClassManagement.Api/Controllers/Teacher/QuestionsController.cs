using ClassManagement.Application.Common.Constants;
using ClassManagement.Infrastructure.Security;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.AspNetCore.RateLimiting;

namespace ClassManagement.Api.Controllers.Teacher;

[ApiController]
[Route("api/teacher/questions")]
[Authorize(Roles = ApplicationRoles.Teacher)]
public class TeacherQuestionsController : BaseApiController
{
    private readonly ISender _mediator;

    public TeacherQuestionsController(ISender mediator)
    {
        _mediator = mediator;
    }

    // ── Reads ────────────────────────────────────────────────────────────────────────────────

    [HttpGet]
    [EnableRateLimiting(RateLimitOptions.Policies.Read)]
    [OutputCache(PolicyName = OutputCachePolicies.TeacherQuestionsRead)]
    public async Task<IActionResult> GetMyQuestions(
        [FromQuery] GetTeacherQuestionsQuery query,
        CancellationToken cancellationToken
    )
    {
        var result = await _mediator.Send(query, cancellationToken);
        return ApiOk(result);
    }

    [HttpGet("public")]
    [EnableRateLimiting(RateLimitOptions.Policies.Read)]
    [OutputCache(PolicyName = OutputCachePolicies.PublicQuestionsRead)]
    public async Task<IActionResult> GetPublicQuestions(
        [FromQuery] GetPublicQuestionsQuery query,
        CancellationToken cancellationToken
    )
    {
        var result = await _mediator.Send(query, cancellationToken);
        return ApiOk(result);
    }

    [HttpGet("public/{publicId:guid}")]
    [EnableRateLimiting(RateLimitOptions.Policies.Read)]
    public async Task<IActionResult> GetPublicQuestion(
        Guid publicId,
        CancellationToken cancellationToken
    )
    {
        var result = await _mediator.Send(
            new GetPublicQuestionDetailQuery(publicId),
            cancellationToken
        );
        return ApiOk(result);
    }

    [HttpGet("{publicId:guid}", Name = "GetTeacherQuestion")]
    [EnableRateLimiting(RateLimitOptions.Policies.Read)]
    public async Task<IActionResult> GetQuestion(Guid publicId, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetQuestionDetailQuery(publicId), cancellationToken);
        return ApiOk(result);
    }

    // ── Writes ───────────────────────────────────────────────────────────────────────────────

    [HttpPost]
    [EnableRateLimiting(RateLimitOptions.Policies.QuestionWrite)]
    public async Task<IActionResult> Create(
        CreateQuestionCommand command,
        CancellationToken cancellationToken
    )
    {
        var publicId = await _mediator.Send(command, cancellationToken);
        await EvictCacheAsync(OutputCacheTags.Questions, cancellationToken);
        return ApiCreated(
            "GetTeacherQuestion",
            new { publicId },
            new { publicId },
            "Question.Created"
        );
    }

    [HttpPut("{publicId:guid}")]
    [EnableRateLimiting(RateLimitOptions.Policies.QuestionWrite)]
    public async Task<IActionResult> Update(
        Guid publicId,
        UpdateQuestionCommand command,
        CancellationToken cancellationToken
    )
    {
        await _mediator.Send(command with { PublicId = publicId }, cancellationToken);
        await EvictCacheAsync(OutputCacheTags.Questions, cancellationToken);
        return ApiOk("Question.Updated");
    }

    [HttpDelete("{publicId:guid}")]
    [EnableRateLimiting(RateLimitOptions.Policies.QuestionWrite)]
    public async Task<IActionResult> Delete(Guid publicId, CancellationToken cancellationToken)
    {
        await _mediator.Send(new DeleteQuestionCommand(publicId), cancellationToken);
        await EvictCacheAsync(OutputCacheTags.Questions, cancellationToken);
        return ApiOk("Question.Deleted");
    }

    [HttpPatch("{publicId:guid}/visibility")]
    [EnableRateLimiting(RateLimitOptions.Policies.QuestionWrite)]
    public async Task<IActionResult> SetVisibility(
        Guid publicId,
        SetQuestionVisibilityCommand command,
        CancellationToken cancellationToken
    )
    {
        await _mediator.Send(command with { PublicId = publicId }, cancellationToken);
        await EvictCacheAsync(OutputCacheTags.Questions, cancellationToken);
        return ApiOk("Question.VisibilityUpdated");
    }

    [HttpPost("{publicId:guid}/duplicate")]
    [EnableRateLimiting(RateLimitOptions.Policies.QuestionWrite)]
    public async Task<IActionResult> Duplicate(Guid publicId, CancellationToken cancellationToken)
    {
        var newId = await _mediator.Send(new DuplicateQuestionCommand(publicId), cancellationToken);
        await EvictCacheAsync(OutputCacheTags.Questions, cancellationToken);
        return ApiCreated(
            "GetTeacherQuestion",
            new { publicId = newId },
            new { publicId = newId },
            "Question.Duplicated"
        );
    }
}
