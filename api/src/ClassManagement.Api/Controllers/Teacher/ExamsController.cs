using ClassManagement.Application.Common.Constants;
using ClassManagement.Infrastructure.Security;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.AspNetCore.RateLimiting;

namespace ClassManagement.Api.Controllers.Teacher;

[ApiController]
[Route("api/teacher/exams")]
[Authorize(Roles = ApplicationRoles.Teacher)]
public class TeacherExamsController : BaseApiController
{
    private readonly ISender _mediator;

    public TeacherExamsController(ISender mediator)
    {
        _mediator = mediator;
    }

    // ── Reads ────────────────────────────────────────────────────────────────────────────────

    [HttpGet]
    [EnableRateLimiting(RateLimitOptions.Policies.Read)]
    [OutputCache(PolicyName = OutputCachePolicies.TeacherExamsRead)]
    public async Task<IActionResult> GetMyExams(
        [FromQuery] GetTeacherExamsQuery query,
        CancellationToken cancellationToken
    )
    {
        var result = await _mediator.Send(query, cancellationToken);
        return ApiOk(result);
    }

    [HttpGet("public")]
    [EnableRateLimiting(RateLimitOptions.Policies.Read)]
    [OutputCache(PolicyName = OutputCachePolicies.PublicExamsRead)]
    public async Task<IActionResult> GetPublicExams(
        [FromQuery] GetPublicExamsQuery query,
        CancellationToken cancellationToken
    )
    {
        var result = await _mediator.Send(query, cancellationToken);
        return ApiOk(result);
    }

    [HttpGet("public/{publicId:guid}")]
    [EnableRateLimiting(RateLimitOptions.Policies.Read)]
    [OutputCache(PolicyName = OutputCachePolicies.PublicExamsRead)]
    public async Task<IActionResult> GetPublicExam(
        Guid publicId,
        CancellationToken cancellationToken
    )
    {
        var result = await _mediator.Send(
            new GetPublicExamDetailQuery(publicId),
            cancellationToken
        );
        return ApiOk(result);
    }

    [HttpGet("{publicId:guid}", Name = "GetTeacherExam")]
    [EnableRateLimiting(RateLimitOptions.Policies.Read)]
    [OutputCache(PolicyName = OutputCachePolicies.TeacherExamsRead)]
    public async Task<IActionResult> GetExam(Guid publicId, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetExamDetailQuery(publicId), cancellationToken);
        return ApiOk(result);
    }

    [HttpGet("{publicId:guid}/preview")]
    [EnableRateLimiting(RateLimitOptions.Policies.Read)]
    public async Task<IActionResult> PreviewExam(Guid publicId, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetExamPreviewQuery(publicId), cancellationToken);
        return ApiOk(result);
    }

    // ── Writes ───────────────────────────────────────────────────────────────────────────────

    [HttpPost]
    [EnableRateLimiting(RateLimitOptions.Policies.ExamWrite)]
    public async Task<IActionResult> Create(
        CreateExamCommand command,
        CancellationToken cancellationToken
    )
    {
        var publicId = await _mediator.Send(command, cancellationToken);
        await EvictCacheAsync(OutputCacheTags.Exams, cancellationToken);
        return ApiCreated("GetTeacherExam", new { publicId }, new { publicId }, "Exam.Created");
    }

    [HttpPut("{publicId:guid}")]
    [EnableRateLimiting(RateLimitOptions.Policies.ExamWrite)]
    public async Task<IActionResult> Update(
        Guid publicId,
        UpdateExamCommand command,
        CancellationToken cancellationToken
    )
    {
        await _mediator.Send(command with { PublicId = publicId }, cancellationToken);
        await EvictCacheAsync(OutputCacheTags.Exams, cancellationToken);
        return ApiOk("Exam.Updated");
    }

    [HttpPut("{publicId:guid}/questions")]
    [EnableRateLimiting(RateLimitOptions.Policies.ExamWrite)]
    public async Task<IActionResult> UpdateQuestions(
        Guid publicId,
        UpdateExamQuestionsCommand command,
        CancellationToken cancellationToken
    )
    {
        await _mediator.Send(command with { PublicId = publicId }, cancellationToken);
        await EvictCacheAsync(OutputCacheTags.Exams, cancellationToken);
        return ApiOk("Exam.QuestionsUpdated");
    }

    [HttpPatch("{publicId:guid}/visibility")]
    [EnableRateLimiting(RateLimitOptions.Policies.ExamWrite)]
    public async Task<IActionResult> SetVisibility(
        Guid publicId,
        SetExamVisibilityCommand command,
        CancellationToken cancellationToken
    )
    {
        await _mediator.Send(command with { PublicId = publicId }, cancellationToken);
        await EvictCacheAsync(OutputCacheTags.Exams, cancellationToken);
        return ApiOk("Exam.VisibilityUpdated");
    }

    [HttpPost("{publicId:guid}/duplicate")]
    [EnableRateLimiting(RateLimitOptions.Policies.ExamWrite)]
    public async Task<IActionResult> Duplicate(Guid publicId, CancellationToken cancellationToken)
    {
        var newId = await _mediator.Send(new DuplicateExamCommand(publicId), cancellationToken);
        await EvictCacheAsync(OutputCacheTags.Exams, cancellationToken);
        return ApiCreated(
            "GetTeacherExam",
            new { publicId = newId },
            new { publicId = newId },
            "Exam.Duplicated"
        );
    }

    [HttpDelete("{publicId:guid}")]
    [EnableRateLimiting(RateLimitOptions.Policies.ExamWrite)]
    public async Task<IActionResult> Delete(Guid publicId, CancellationToken cancellationToken)
    {
        await _mediator.Send(new DeleteExamCommand(publicId), cancellationToken);
        await EvictCacheAsync(OutputCacheTags.Exams, cancellationToken);
        return ApiOk("Exam.Deleted");
    }
}
