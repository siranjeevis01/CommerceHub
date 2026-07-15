using MediatR;
using MassTransit;
using CommerceHub.Modules.Order.Application.Common.Interfaces;
using CommerceHub.Modules.Order.Domain.Entities;
using CommerceHub.Shared.Contracts.Events;

namespace CommerceHub.Modules.Order.Application.Commands;

public record DeliverOrderCommand : IRequest
{
    public int OrderId { get; init; }
    public string? OtpCode { get; init; }
    public decimal? Latitude { get; init; }
    public decimal? Longitude { get; init; }
    public string? LocationName { get; init; }
}

public class DeliverOrderCommandHandler : IRequestHandler<DeliverOrderCommand>
{
    private readonly IOrderDbContext _context;
    private readonly IPublishEndpoint _publishEndpoint;

    public DeliverOrderCommandHandler(IOrderDbContext context, IPublishEndpoint publishEndpoint)
    {
        _context = context;
        _publishEndpoint = publishEndpoint;
    }

    public async Task Handle(DeliverOrderCommand request, CancellationToken cancellationToken)
    {
        var order = await _context.Orders.FindAsync(new object[] { request.OrderId }, cancellationToken);
        if (order is null)
            throw new InvalidOperationException($"Order with ID {request.OrderId} not found.");

        if (order.OrderStatus != "Shipped")
            throw new InvalidOperationException($"Order cannot be delivered in '{order.OrderStatus}' status.");

        var previousStatus = order.OrderStatus;
        order.OrderStatus = "Delivered";

        order.Trackings.Add(new OrderTracking
        {
            OrderId = order.Id,
            Status = "Delivered",
            Description = "Package delivered",
            Latitude = request.Latitude,
            Longitude = request.Longitude,
            LocationName = request.LocationName,
        });

        order.StatusHistories.Add(new OrderStatusHistory
        {
            OrderId = order.Id,
            FromStatus = previousStatus,
            ToStatus = "Delivered",
            OtpCode = request.OtpCode,
            ChangedBy = "System",
        });

        await _context.SaveChangesAsync(cancellationToken);

        await _publishEndpoint.Publish(new OrderDelivered
        {
            OrderId = order.Id,
            OrderNumber = order.OrderNumber,
            UserId = order.UserId,
            DeliveredAt = DateTime.UtcNow,
        }, cancellationToken);
    }
}
