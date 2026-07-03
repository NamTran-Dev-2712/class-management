using ClassManagement.Infrastructure.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.RateLimiting;

namespace ClassManagement.Api.Controllers;

// Dev/test-only bytes sink for the LocalStorageProvider (MVP-9). A production R2 upload PUTs straight to
// the cloud via a presigned URL and never touches the API; the Local backend can't mint such a URL, so its
// presign points here. The HMAC token (key + expiry, minted at presign) is the trust boundary, exactly
// like the presigned-URL signature — so this is [AllowAnonymous]. Only active when Storage:UseFakeProvider.
[ApiController]
[Route("api/media/local")]
[AllowAnonymous]
public class MediaLocalUploadController : BaseApiController
{
    private readonly ILocalUploadStore _store;

    public MediaLocalUploadController(ILocalUploadStore store)
    {
        _store = store;
    }

    [HttpPut]
    [EnableRateLimiting(RateLimitOptions.Policies.MediaLocalUpload)]
    public async Task<IActionResult> Upload(
        [FromQuery] string key,
        [FromQuery] long exp,
        [FromQuery] string sig,
        CancellationToken cancellationToken
    )
    {
        var ok = await _store.TrySaveAsync(key, exp, sig, Request.Body, cancellationToken);
        if (!ok)
            return ApiForbidden("Media.LocalUploadRejected");

        return ApiOk("Media.LocalUploadStored");
    }
}
