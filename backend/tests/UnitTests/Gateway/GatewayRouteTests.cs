using FluentAssertions;
using Xunit;

namespace CommerceHub.Gateway.UnitTests;

public class GatewayRouteTests
{
    [Fact]
    public void RouteConfiguration_ShouldMapToCorrectService()
    {
        var routes = new Dictionary<string, string>
        {
            { "/api/v1/auth/**", "identity" },
            { "/api/v1/products/**", "product" },
            { "/api/v1/cart/**", "cart" },
            { "/api/v1/orders/**", "order" },
            { "/api/v1/payments/**", "payment" },
            { "/api/v1/vendors/**", "vendor" },
            { "/api/v1/inventory/**", "inventory" },
            { "/api/v1/notifications/**", "notification" },
            { "/api/v1/cms/**", "cms" },
            { "/api/v1/analytics/**", "analytics" }
        };

        routes.Should().HaveCount(10);
        routes["/api/v1/auth/**"].Should().Be("identity");
        routes["/api/v1/products/**"].Should().Be("product");
        routes["/api/v1/orders/**"].Should().Be("order");
    }

    [Fact]
    public void DownstreamService_ShouldHaveHealthEndpoint()
    {
        var services = new Dictionary<string, string>
        {
            { "identity", "http://identity-api:8080/health" },
            { "product", "http://product-api:8080/health" },
            { "order", "http://order-api:8080/health" },
            { "cart", "http://cart-api:8080/health" },
            { "payment", "http://payment-api:8080/health" },
            { "vendor", "http://vendor-api:8080/health" },
            { "inventory", "http://inventory-api:8080/health" },
            { "notification", "http://notification-api:8080/health" },
            { "cms", "http://cms-api:8080/health" },
            { "analytics", "http://analytics-api:8080/health" }
        };

        foreach (var (name, url) in services)
        {
            url.Should().EndWith("/health");
        }
    }

    [Fact]
    public void RateLimitConfig_ShouldSetLimits()
    {
        var rateLimits = new Dictionary<string, (int PerSecond, int PerMinute)>
        {
            { "default", (100, 1000) },
            { "auth", (20, 200) },
            { "cart", (50, 500) },
            { "checkout", (10, 100) }
        };

        rateLimits["default"].PerSecond.Should().Be(100);
        rateLimits["auth"].PerSecond.Should().Be(20);
        rateLimits["checkout"].PerSecond.Should().Be(10);
    }

    [Fact]
    public void CorsConfig_ShouldAllowOrigins()
    {
        var allowedOrigins = new[] { "http://localhost:4200", "http://localhost:4201", "http://localhost:4202", "http://localhost:4203" };
        allowedOrigins.Should().Contain("http://localhost:4200");
        allowedOrigins.Should().HaveCount(4);
    }

    [Fact]
    public void RoutePriority_ShouldBeInCorrectOrder()
    {
        var routes = new (string Path, int Priority)[]
        {
            ("/api/v1/auth/login", 1),
            ("/api/v1/products/{id}", 2),
            ("/api/v1/products", 3),
            ("/api/v1/{catchall}", 99)
        };

        routes[0].Priority.Should().BeLessThan(routes[1].Priority);
        routes[3].Priority.Should().Be(99);
    }
}
