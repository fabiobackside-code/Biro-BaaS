using System.Text.Json;
using Biro.Infrastructure.Cache;
using Microsoft.AspNetCore.Http;

namespace Biro.Blocks.Debit;

public class IdempotencyMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ICacheProvider _cache;

    public IdempotencyMiddleware(RequestDelegate next, ICacheProvider cache)
    {
        _next = next;
        _cache = cache;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (!context.Request.Headers.TryGetValue("X-Request-Id", out var requestId))
        {
            await _next(context);
            return;
        }

        var cachedResponse = await _cache.GetAsync<JsonDocument>(requestId);
        if (cachedResponse != null)
        {
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync(cachedResponse.RootElement.GetRawText());
            return;
        }

        var originalBodyStream = context.Response.Body;
        using var responseBody = new MemoryStream();
        context.Response.Body = responseBody;

        await _next(context);

        responseBody.Seek(0, SeekOrigin.Begin);
        var response = await new StreamReader(responseBody).ReadToEndAsync();
        responseBody.Seek(0, SeekOrigin.Begin);

        await _cache.SetAsync(requestId.ToString(), JsonDocument.Parse(response), TimeSpan.FromMinutes(10));

        await responseBody.CopyToAsync(originalBodyStream);
    }
}
