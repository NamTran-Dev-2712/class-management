using ClassManagement.Application.Common.Constants;
using ClassManagement.Infrastructure.Security;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.AspNetCore.RateLimiting;

[ApiController]
[Route("api/admin/users")]
[Authorize(Roles = ApplicationRoles.Admin)]
public class UsersController : BaseApiController
{
    private readonly ISender _mediator;

    public UsersController(ISender mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    [EnableRateLimiting(RateLimitOptions.Policies.Read)]
    [OutputCache(PolicyName = OutputCachePolicies.UsersRead)]
    public async Task<IActionResult> GetUsers(
        [FromQuery] GetUsersQuery query,
        CancellationToken cancellationToken
    )
    {
        var result = await _mediator.Send(query, cancellationToken);
        return ApiOk(result);
    }

    [HttpGet("{publicId:guid}", Name = "GetUserById")]
    [OutputCache(PolicyName = OutputCachePolicies.UsersRead)]
    public async Task<IActionResult> GetUser(Guid publicId, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetUserByIdQuery(publicId), cancellationToken);
        return ApiOk(result);
    }

    [HttpPost]
    [EnableRateLimiting(RateLimitOptions.Policies.UserWrite)]
    public async Task<IActionResult> Create(
        CreateUserCommand command,
        CancellationToken cancellationToken
    )
    {
        var publicId = await _mediator.Send(command, cancellationToken);
        await EvictCacheAsync(OutputCacheTags.Users, cancellationToken);
        return ApiCreated("GetUserById", new { publicId }, new { publicId }, "User.Created");
    }

    [HttpPut("{publicId:guid}")]
    [EnableRateLimiting(RateLimitOptions.Policies.UserWrite)]
    public async Task<IActionResult> Update(
        Guid publicId,
        UpdateUserCommand command,
        CancellationToken cancellationToken
    )
    {
        await _mediator.Send(command with { PublicId = publicId }, cancellationToken);
        await EvictCacheAsync(OutputCacheTags.Users, cancellationToken);
        return ApiOk("User.Updated");
    }

    [HttpPost("{publicId:guid}/lock")]
    [EnableRateLimiting(RateLimitOptions.Policies.UserWrite)]
    public async Task<IActionResult> Lock(Guid publicId, CancellationToken cancellationToken)
    {
        await _mediator.Send(new LockUserCommand(publicId), cancellationToken);
        await EvictCacheAsync(OutputCacheTags.Users, cancellationToken);
        return ApiOk("User.Locked");
    }

    [HttpPost("{publicId:guid}/unlock")]
    [EnableRateLimiting(RateLimitOptions.Policies.UserWrite)]
    public async Task<IActionResult> Unlock(Guid publicId, CancellationToken cancellationToken)
    {
        await _mediator.Send(new UnlockUserCommand(publicId), cancellationToken);
        await EvictCacheAsync(OutputCacheTags.Users, cancellationToken);
        return ApiOk("User.Unlocked");
    }

    [HttpDelete("{publicId:guid}")]
    [EnableRateLimiting(RateLimitOptions.Policies.UserWrite)]
    public async Task<IActionResult> Delete(Guid publicId, CancellationToken cancellationToken)
    {
        await _mediator.Send(new DeleteUserCommand(publicId), cancellationToken);
        await EvictCacheAsync(OutputCacheTags.Users, cancellationToken);
        return ApiOk("User.Deleted");
    }
}
