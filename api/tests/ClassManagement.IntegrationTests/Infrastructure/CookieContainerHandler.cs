using System.Net;

namespace ClassManagement.IntegrationTests.Infrastructure;

// Manages cookie state across requests for a single client session
public sealed class CookieContainerHandler : DelegatingHandler
{
    private readonly CookieContainer _jar = new();

    public CookieContainerHandler(HttpMessageHandler inner)
        : base(inner) { }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken ct
    )
    {
        var uri = request.RequestUri!;
        var existing = _jar.GetCookieHeader(uri);
        if (!string.IsNullOrEmpty(existing))
            request.Headers.TryAddWithoutValidation("Cookie", existing);

        var response = await base.SendAsync(request, ct);

        if (response.Headers.TryGetValues("Set-Cookie", out var setCookies))
        {
            foreach (var header in setCookies)
            {
                try
                {
                    _jar.SetCookies(uri, header);
                }
                catch
                { /* ignore malformed cookies */
                }
            }
        }

        return response;
    }

    public string? GetCookieValue(string name, Uri baseUri)
    {
        var cookies = _jar.GetCookies(baseUri);
        return cookies[name]?.Value;
    }
}
