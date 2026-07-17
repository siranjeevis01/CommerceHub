using CommerceHub.Modules.Order.Infrastructure.Hubs;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace CommerceHub.Modules.Order.Infrastructure;

public static class TrackingHubExtensions
{
    public static IEndpointRouteBuilder MapTrackingHub(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapHub<TrackingHub>("/hubs/tracking");
        return endpoints;
    }
}
