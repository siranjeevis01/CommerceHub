using MediatR;
using MassTransit;
using CommerceHub.Order.Application.Common.Interfaces;
using CommerceHub.Order.Domain.Entities;
using CommerceHub.Shared.Contracts.Events;

namespace CommerceHub.Order.Application.Commands;

public record ShipOrderCommand : IRequest
{
    public int OrderId { get; init; }
    public string TrackingNumber { get; init; } = string.Empty;
    public string? Carrier { get; init; }
}

public class ShipOrderCommandHandler : IRequestHandler<ShipOrderCommand>
{
    private readonly IOrderDbContext _context;
    private readonly IPublishEndpoint _publishEndpoint;

    public ShipOrderCommandHandler(IOrderDbContext context, IPublishEndpoint publishEndpoint)
    {
        _context = context;
        _publishEndpoint = publishEndpoint;
    }

    public async Task Handle(ShipOrderCommand request, CancellationToken cancellationToken)
    {
        var order = await _context.Orders.FindAsync(new object[] { request.OrderId }, cancellationToken);
        if (order is null)
            throw new InvalidOperationException($"Order with ID {request.OrderId} not found.");

        if (order.OrderStatus != "Confirmed")
            throw new InvalidOperationException($"Order cannot be shipped in '{order.OrderStatus}' status.");

        var previousStatus = order.OrderStatus;
        order.OrderStatus = "Shipped";

        order.Trackings.Add(new OrderTracking
        {
            OrderId = order.Id,
            Status = "Shipped",
            Description = $"Shipped via {request.Carrier ?? "Unknown"}, tracking: {request.TrackingNumber}",
        });

        order.StatusHistories.Add(new OrderStatusHistory
        {
            OrderId = order.Id,
            FromStatus = previousStatus,
            ToStatus = "Shipped",
            ChangedBy = "System",
        });

        await _context.SaveChangesAsync(cancellationToken);

        await _publishEndpoint.Publish(new OrderShipped
        {
            OrderId = order.Id,
            OrderNumber = order.OrderNumber,
            UserId = order.UserId,
            TrackingNumber = request.TrackingNumber,
            ShippedAt = DateTime.UtcNow,
        }, cancellationToken);
    }
}
