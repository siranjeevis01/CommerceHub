using MediatR;
using MassTransit;
using CommerceHub.Order.Application.Common.Interfaces;
using CommerceHub.Order.Domain.Entities;
using CommerceHub.Shared.Contracts.Events;

namespace CommerceHub.Order.Application.Commands;

public record CancelOrderCommand : IRequest
{
    public int OrderId { get; init; }
    public string Reason { get; init; } = string.Empty;
}

public class CancelOrderCommandHandler : IRequestHandler<CancelOrderCommand>
{
    private readonly IOrderDbContext _context;
    private readonly IPublishEndpoint _publishEndpoint;

    public CancelOrderCommandHandler(IOrderDbContext context, IPublishEndpoint publishEndpoint)
    {
        _context = context;
        _publishEndpoint = publishEndpoint;
    }

    public async Task Handle(CancelOrderCommand request, CancellationToken cancellationToken)
    {
        var order = await _context.Orders.FindAsync(new object[] { request.OrderId }, cancellationToken);
        if (order is null)
            throw new InvalidOperationException($"Order with ID {request.OrderId} not found.");

        if (order.OrderStatus != "Pending" && order.OrderStatus != "Confirmed")
            throw new InvalidOperationException($"Order cannot be cancelled in '{order.OrderStatus}' status.");

        var previousStatus = order.OrderStatus;
        order.OrderStatus = "Cancelled";
        order.PaymentStatus = "Cancelled";

        order.StatusHistories.Add(new OrderStatusHistory
        {
            OrderId = order.Id,
            FromStatus = previousStatus,
            ToStatus = "Cancelled",
            Remarks = request.Reason,
            ChangedBy = "System",
        });

        await _context.SaveChangesAsync(cancellationToken);

        await _publishEndpoint.Publish(new OrderCancelled
        {
            OrderId = order.Id,
            OrderNumber = order.OrderNumber,
            UserId = order.UserId,
            Reason = request.Reason,
            CancelledAt = DateTime.UtcNow,
        }, cancellationToken);
    }
}
