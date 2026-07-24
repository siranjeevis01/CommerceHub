using System.Net;
using System.Text.Json;

namespace CommerceHub.Api.Middleware;

public class GlobalExceptionHandlerMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionHandlerMiddleware> _logger;
    private readonly IHostEnvironment _env;

    public GlobalExceptionHandlerMiddleware(RequestDelegate next, ILogger<GlobalExceptionHandlerMiddleware> logger, IHostEnvironment env)
    {
        _next = next;
        _logger = logger;
        _env = env;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (OperationCanceledException) when (context.RequestAborted.IsCancellationRequested)
        {
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception on {Method} {Path}", context.Request.Method, context.Request.Path);

            if (!context.Response.HasStarted)
            {
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                context.Response.ContentType = "application/problem+json";

                var problem = new Dictionary<string, object?>
                {
                    ["type"] = "https://tools.ietf.org/html/rfc7231#section-6.6.1",
                    ["title"] = "An unexpected error occurred",
                    ["status"] = 500,
                    ["instance"] = context.Request.Path,
                };

                if (_env.IsDevelopment())
                {
                    problem["detail"] = ex.Message;
                    problem["stackTrace"] = ex.StackTrace?.Split('\n').Take(5).ToArray();
                }
                else
                {
                    problem["detail"] = "An internal server error occurred. Please try again later.";
                }

                await context.Response.WriteAsync(JsonSerializer.Serialize(problem));
            }
        }
    }
}

public static class GlobalExceptionHandlerExtensions
{
    public static IApplicationBuilder UseGlobalExceptionHandler(this IApplicationBuilder app)
    {
        return app.UseMiddleware<GlobalExceptionHandlerMiddleware>();
    }
}
