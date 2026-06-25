using ClassManagement.Application.Common.Constants;
using ClassManagement.Infrastructure.Security;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.AspNetCore.RateLimiting;

namespace ClassManagement.Api.Controllers.Admin;

[ApiController]
[Route("api/admin/settings")]
[Authorize(Roles = ApplicationRoles.Admin)]
public class AdminSystemSettingsController : BaseApiController
{
    private readonly ISender _mediator;

    public AdminSystemSettingsController(ISender mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    [EnableRateLimiting(RateLimitOptions.Policies.Read)]
    [OutputCache(PolicyName = OutputCachePolicies.SystemSettingsRead)]
    public async Task<IActionResult> GetSettings(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetSystemSettingsQuery(), cancellationToken);
        return ApiOk(result);
    }

    [HttpPut("{key}")]
    [EnableRateLimiting(RateLimitOptions.Policies.SystemSettingWrite)]
    public async Task<IActionResult> Update(
        string key,
        UpdateSettingBody body,
        CancellationToken cancellationToken
    )
    {
        await _mediator.Send(new UpdateSystemSettingCommand(key, body.Value), cancellationToken);
        await EvictCacheAsync(OutputCacheTags.SystemSettings, cancellationToken);
        return ApiOk("SystemSetting.Updated");
    }
}

public sealed record UpdateSettingBody(string Value);
