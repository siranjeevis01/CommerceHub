using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace CommerceHub.Modules.Order.Infrastructure.Hubs;

[Authorize]
public class TrackingHub : Hub
{
    public async Task SubscribeToOrderTracking(string orderNumber)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"tracking_{orderNumber}");
    }

    public async Task UnsubscribeFromOrderTracking(string orderNumber)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"tracking_{orderNumber}");
    }

    public async Task UpdateLocation(string orderNumber, double latitude, double longitude, string locationName)
    {
        await Clients.Group($"tracking_{orderNumber}").SendAsync("LocationUpdated", new
        {
            OrderNumber = orderNumber,
            Latitude = latitude,
            Longitude = longitude,
            LocationName = locationName,
            Timestamp = DateTime.UtcNow
        });
    }
}
