using System.Diagnostics;
using System.Text;

namespace CommerceHub.Api.Middleware;

public class AuditLogMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<AuditLogMiddleware> _logger;

    public AuditLogMiddleware(
        RequestDelegate next,
        ILogger<AuditLogMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var originalBodyStream = context.Response.Body;

        using var responseBody = new MemoryStream();
        context.Response.Body = responseBody;

        var stopwatch = Stopwatch.StartNew();

        try
        {
            await _next(context);
        }
        finally
        {
            stopwatch.Stop();
            responseBody.Seek(0, SeekOrigin.Begin);
            var responseContent = await new StreamReader(responseBody).ReadToEndAsync();
            responseBody.Seek(0, SeekOrigin.Begin);
            await responseBody.CopyToAsync(originalBodyStream);
            context.Response.Body = originalBodyStream;

            _ = Task.Run(async () =>
            {
                try
                {
                    var userId = context.User?.FindFirst("userId")?.Value;
                    var userRole = context.User?.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;
                    _logger.LogInformation(
                        "Audit: {Method} {Path} | Status={StatusCode} | Duration={Duration}ms | UserId={UserId} | Role={Role}",
                        context.Request.Method,
                        context.Request.Path,
                        context.Response.StatusCode,
                        stopwatch.ElapsedMilliseconds,
                        userId ?? "anonymous",
                        userRole ?? "Anonymous");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to write audit log");
                }
            });
        }
    }

    private static async Task<string> ReadRequestBodyAsync(HttpContext context)
    {
        if (!context.Request.Body.CanSeek)
            context.Request.EnableBuffering();

        context.Request.Body.Position = 0;
        using var reader = new StreamReader(context.Request.Body, Encoding.UTF8, leaveOpen: true);
        var body = await reader.ReadToEndAsync();
        context.Request.Body.Position = 0;
        return body;
    }
}

public static class AuditLogExtensions
{
    public static IApplicationBuilder UseAuditLog(this IApplicationBuilder app)
    {
        return app.UseMiddleware<AuditLogMiddleware>();
    }
}
