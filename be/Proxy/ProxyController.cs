using Microsoft.AspNetCore.Mvc;

namespace be.Proxy;

public class ProxyController(IHttpClientFactory httpClientFactory) : ControllerBase
{
    [AcceptVerbs]
    [Route("api/proxy")]
    public async Task<IActionResult> ProxyAsync(CancellationToken cancellationToken)
    {
        var url = Request.Query["url"].ToString();
        if (string.IsNullOrEmpty(url))
        {
            return BadRequest("Missing 'url' query parameter.");
        }

        byte[]? body = null;
        if (Request.Body != null && Request.ContentLength > 0)
        {
            using var memoryStream = new MemoryStream();
            await Request.Body.CopyToAsync(memoryStream);
            body = memoryStream.ToArray();
        }

        var upstreamRequest = new HttpRequestMessage
        {
            RequestUri = new Uri(url),
            Method = new HttpMethod(Request.Method),
            Content = body != null ? new ByteArrayContent(body) : null,
        };
        foreach (var header in Request.Headers)
        {
            if (!upstreamRequest.Headers.TryAddWithoutValidation(header.Key, header.Value.ToArray()))
            {
                upstreamRequest.Content?.Headers.TryAddWithoutValidation(header.Key, header.Value.ToArray());
            }
        }
        upstreamRequest.Headers.Host = new Uri(url).Host;

        using var client = httpClientFactory.CreateClient();
        var upstreamResponse = await client.SendAsync(upstreamRequest, cancellationToken);

        var upstreamBody = await upstreamResponse.Content.ReadAsByteArrayAsync(cancellationToken);
        var contentType = upstreamResponse.Content.Headers.ContentType?.ToString() ?? "application/octet-stream";

        Response.StatusCode = (int)upstreamResponse.StatusCode;
        foreach (var header in upstreamResponse.Headers)
        {
            Response.Headers[header.Key] = header.Value.ToArray();
        }
        foreach (var header in upstreamResponse.Content.Headers)
        {
            Response.Headers[header.Key] = header.Value.ToArray();
        }

        return File(upstreamBody, contentType);
    }
}
