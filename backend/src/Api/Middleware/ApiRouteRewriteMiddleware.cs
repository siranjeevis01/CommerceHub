namespace CommerceHub.Api.Middleware;

public sealed class ApiRouteRewriteMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ApiRouteRewriteMiddleware> _logger;

    private static readonly Dictionary<string, string> ExactMappings = new(StringComparer.OrdinalIgnoreCase)
    {
        ["/api/v1/auth/login"] = "/api/v1/identity/auth/login",
        ["/api/v1/auth/register"] = "/api/v1/identity/auth/register",
        ["/api/v1/auth/logout"] = "/api/v1/identity/auth/logout",
        ["/api/v1/auth/refresh-token"] = "/api/v1/identity/auth/refresh-token",
        ["/api/v1/auth/forgot-password"] = "/api/v1/identity/auth/forgot-password",
        ["/api/v1/auth/reset-password"] = "/api/v1/identity/auth/reset-password",
        ["/api/v1/auth/verify-email"] = "/api/v1/identity/auth/verify-email",
        ["/api/v1/auth/google"] = "/api/v1/identity/auth/google",
        ["/api/v1/auth/facebook"] = "/api/v1/identity/auth/facebook",
        ["/api/v1/auth/profile"] = "/api/v1/users/profile",
        ["/api/v1/auth/change-password"] = "/api/v1/identity/auth/change-password",

        ["/api/v1/admin/dashboard/stats"] = "/api/analytics/dashboard",
        ["/api/v1/admin/dashboard/revenue"] = "/api/analytics/sales",
        ["/api/v1/admin/dashboard/top-products"] = "/api/v1/products",
        ["/api/v1/admin/system/health"] = "/health",

        ["/api/v1/admin/cms/pages"] = "/api/pages",
    };

    private static readonly (string Prefix, string Replacement)[] PrefixMappings =
    [
        ("/api/v1/admin/users", "/api/v1/identity/admin/users"),
        ("/api/v1/admin/orders", "/api/v1/orders"),
        ("/api/v1/admin/products", "/api/v1/products"),
        ("/api/v1/admin/vendors", "/api/v1/vendors"),
        ("/api/v1/admin/categories", "/api/v1/categories"),
        ("/api/v1/admin/brands", "/api/v1/brands"),
        ("/api/v1/admin/coupons", "/api/v1/coupons"),
        ("/api/v1/admin/payouts", "/api/v1/vendor-payouts"),
        ("/api/v1/admin/analytics", "/api/analytics"),
        ("/api/v1/admin/disputes", "/api/v1/admin/disputes"),
        ("/api/v1/admin/cms/", "/api/pages/"),

        ("/api/v1/vendor/dashboard/", "/api/analytics/vendor/"),
        ("/api/v1/vendor/products", "/api/v1/products"),
        ("/api/v1/vendor/orders", "/api/v1/orders"),
        ("/api/v1/vendor/payouts", "/api/v1/vendor-payouts"),
        ("/api/v1/vendor/commissions", "/api/v1/vendor-payouts"),
        ("/api/v1/vendor/reviews", "/api/v1/reviews"),
        ("/api/v1/vendor/store", "/api/v1/vendors"),
        ("/api/v1/vendor/analytics", "/api/analytics"),

        ("/api/v1/products/", "/api/v1/products/"),
        ("/api/v1/vendors/", "/api/v1/vendors/"),
    ];

    public ApiRouteRewriteMiddleware(RequestDelegate next, ILogger<ApiRouteRewriteMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var originalPath = context.Request.Path.Value;

        if (originalPath != null && originalPath.StartsWith("/api/", StringComparison.OrdinalIgnoreCase))
        {
            var rewritten = false;

            if (ExactMappings.TryGetValue(originalPath, out var newPath))
            {
                context.Request.Path = newPath;
                rewritten = true;
            }
            else
            {
                foreach (var (prefix, replacement) in PrefixMappings)
                {
                    if (originalPath.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                    {
                        var remainder = originalPath[prefix.Length..];
                        context.Request.Path = replacement + remainder;
                        rewritten = true;
                        break;
                    }
                }
            }

            if (rewritten)
            {
                _logger.LogDebug("Route rewritten: {Original} → {New}", originalPath, context.Request.Path);
            }
        }

        await _next(context);
    }
}

public static class ApiRouteRewriteExtensions
{
    public static IApplicationBuilder UseApiRouteRewrites(this IApplicationBuilder builder)
        => builder.UseMiddleware<ApiRouteRewriteMiddleware>();
}
