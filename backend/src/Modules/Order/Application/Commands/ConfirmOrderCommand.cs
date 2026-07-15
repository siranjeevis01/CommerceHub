using MediatR;
using MassTransit;
using CommerceHub.Modules.Order.Application.Common.Interfaces;
using CommerceHub.Modules.Order.Domain.Entities;
using CommerceHub.Shared.Contracts.Events;

namespace CommerceHub.Modules.Order.Application.Commands;

public record ConfirmOrderCommand : IRequest
{
    public int OrderId { get; init; }
}

public class ConfirmOrderCommandHandler : IRequestHandler<ConfirmOrderCommand>
{
    private readonly IOrderDbContext _context;
    private readonly IPublishEndpoint _publishEndpoint;

    public ConfirmOrderCommandHandler(IOrderDbContext context, IPublishEndpoint publishEndpoint)
    {
        _context = context;
        _publishEndpoint = publishEndpoint;
    }

    public async Task Handle(ConfirmOrderCommand request, CancellationToken cancellationToken)
    {
        var order = await _context.Orders.FindAsync(new object[] { request.OrderId }, cancellationToken);
        if (order is null)
            throw new InvalidOperationException($"Order with ID {request.OrderId} not found.");

        if (order.OrderStatus != "Pending")
            throw new InvalidOperationException($"Order cannot be confirmed in '{order.OrderStatus}' status.");

        var previousStatus = order.OrderStatus;
        order.OrderStatus = "Confirmed";
        order.PaymentStatus = "Paid";

        order.StatusHistories.Add(new OrderStatusHistory
        {
            OrderId = order.Id,
            FromStatus = previousStatus,
            ToStatus = "Confirmed",
            ChangedBy = "System",
        });

        await _context.SaveChangesAsync(cancellationToken);

        await _publishEndpoint.Publish(new OrderConfirmed
        {
            OrderId = order.Id,
            OrderNumber = order.OrderNumber,
            UserId = order.UserId,
            ConfirmedAt = DateTime.UtcNow,
        }, cancellationToken);
    }
}
