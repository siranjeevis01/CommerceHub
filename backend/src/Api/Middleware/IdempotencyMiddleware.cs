using System.Text;
using System.Text.Json;
using CommerceHub.Infrastructure.Cache;

namespace CommerceHub.Api.Middleware;

public class IdempotencyMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ICacheService _cache;
    private readonly ILogger<IdempotencyMiddleware> _logger;

    public IdempotencyMiddleware(RequestDelegate next, ICacheService cache, ILogger<IdempotencyMiddleware> logger)
    {
        _next = next;
        _cache = cache;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (!HttpMethods.IsPost(context.Request.Method) &&
            !HttpMethods.IsPut(context.Request.Method) &&
            !HttpMethods.IsPatch(context.Request.Method))
        {
            await _next(context);
            return;
        }

        var idempotencyKey = context.Request.Headers["Idempotency-Key"].FirstOrDefault();
        if (string.IsNullOrEmpty(idempotencyKey))
        {
            await _next(context);
            return;
        }

        var cacheKey = $"idempotent_{idempotencyKey}";
        var cachedResponse = await _cache.GetAsync<IdempotentResponse>(cacheKey);

        if (cachedResponse != null)
        {
            context.Response.StatusCode = cachedResponse.StatusCode;
            context.Response.ContentType = cachedResponse.ContentType;
            await context.Response.WriteAsync(cachedResponse.Body);
            return;
        }

        var originalBodyStream = context.Response.Body;
        using var responseBody = new MemoryStream();
        context.Response.Body = responseBody;

        await _next(context);

        responseBody.Seek(0, SeekOrigin.Begin);
        var responseBodyText = await new StreamReader(responseBody).ReadToEndAsync();
        responseBody.Seek(0, SeekOrigin.Begin);

        var response = new IdempotentResponse
        {
            StatusCode = context.Response.StatusCode,
            ContentType = context.Response.ContentType ?? "application/json",
            Body = responseBodyText
        };

        await _cache.SetAsync(cacheKey, response, TimeSpan.FromHours(24));

        await responseBody.CopyToAsync(originalBodyStream);
        context.Response.Body = originalBodyStream;
    }
}

public class IdempotentResponse
{
    public int StatusCode { get; set; }
    public string ContentType { get; set; } = "application/json";
    public string Body { get; set; } = string.Empty;
}

public static class IdempotencyExtensions
{
    public static IApplicationBuilder UseIdempotency(this IApplicationBuilder app)
    {
        return app.UseMiddleware<IdempotencyMiddleware>();
    }
}
