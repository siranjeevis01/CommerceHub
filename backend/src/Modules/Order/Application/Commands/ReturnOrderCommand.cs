using MediatR;
using MassTransit;
using CommerceHub.Modules.Order.Application.Common.Interfaces;
using CommerceHub.Modules.Order.Domain.Entities;
using CommerceHub.Shared.Contracts.Events;

namespace CommerceHub.Modules.Order.Application.Commands;

public record ReturnOrderCommand : IRequest
{
    public int OrderId { get; init; }
    public int? OrderItemId { get; init; }
    public string Reason { get; init; } = string.Empty;
    public string? Description { get; init; }
    public string? Images { get; init; }
    public decimal RefundAmount { get; init; }
    public string? RefundMethod { get; init; }
}

public class ReturnOrderCommandHandler : IRequestHandler<ReturnOrderCommand>
{
    private readonly IOrderDbContext _context;
    private readonly IPublishEndpoint _publishEndpoint;

    public ReturnOrderCommandHandler(IOrderDbContext context, IPublishEndpoint publishEndpoint)
    {
        _context = context;
        _publishEndpoint = publishEndpoint;
    }

    public async Task Handle(ReturnOrderCommand request, CancellationToken cancellationToken)
    {
        var order = await _context.Orders.FindAsync(new object[] { request.OrderId }, cancellationToken);
        if (order is null)
            throw new InvalidOperationException($"Order with ID {request.OrderId} not found.");

        if (order.OrderStatus != "Delivered")
            throw new InvalidOperationException($"Order cannot be returned in '{order.OrderStatus}' status.");

        var returnRequest = new ReturnRequest
        {
            OrderId = request.OrderId,
            OrderItemId = request.OrderItemId,
            Reason = request.Reason,
            Description = request.Description,
            Images = request.Images,
            Status = "Pending",
            RefundAmount = request.RefundAmount,
            RefundMethod = request.RefundMethod,
        };

        _context.ReturnRequests.Add(returnRequest);

        var previousStatus = order.OrderStatus;
        order.OrderStatus = "ReturnRequested";

        order.StatusHistories.Add(new OrderStatusHistory
        {
            OrderId = order.Id,
            FromStatus = previousStatus,
            ToStatus = "ReturnRequested",
            Remarks = request.Reason,
            ChangedBy = "System",
        });

        await _context.SaveChangesAsync(cancellationToken);

        await _publishEndpoint.Publish(new OrderReturned
        {
            OrderId = order.Id,
            ReturnRequestId = returnRequest.Id,
            OrderNumber = order.OrderNumber,
            RefundAmount = request.RefundAmount,
            ReturnedAt = DateTime.UtcNow,
        }, cancellationToken);
    }
}
