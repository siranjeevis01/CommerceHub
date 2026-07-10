using MediatR;
using MassTransit;
using CommerceHub.Order.Application.Common.Interfaces;
using CommerceHub.Order.Domain.Entities;
using CommerceHub.Shared.Contracts.Events;

namespace CommerceHub.Order.Application.Commands;

public record PlaceOrderCommand : IRequest<int>
{
    public int UserId { get; init; }
    public decimal SubTotal { get; init; }
    public decimal? DiscountAmount { get; init; }
    public decimal? ShippingCost { get; init; }
    public decimal TaxAmount { get; init; }
    public string? CouponCode { get; init; }
    public string ShippingAddress { get; init; } = string.Empty;
    public string? Notes { get; init; }
    public List<PlaceOrderItemDto> Items { get; init; } = new();
}

public record PlaceOrderItemDto
{
    public int ProductId { get; init; }
    public int? VariantId { get; init; }
    public int Quantity { get; init; }
    public decimal UnitPrice { get; init; }
    public int VendorId { get; init; }
}

public class PlaceOrderCommandHandler : IRequestHandler<PlaceOrderCommand, int>
{
    private readonly IOrderDbContext _context;
    private readonly IOrderNumberGenerator _orderNumberGenerator;
    private readonly IPublishEndpoint _publishEndpoint;

    public PlaceOrderCommandHandler(
        IOrderDbContext context,
        IOrderNumberGenerator orderNumberGenerator,
        IPublishEndpoint publishEndpoint)
    {
        _context = context;
        _orderNumberGenerator = orderNumberGenerator;
        _publishEndpoint = publishEndpoint;
    }

    public async Task<int> Handle(PlaceOrderCommand request, CancellationToken cancellationToken)
    {
        var orderNumber = await _orderNumberGenerator.GenerateOrderNumberAsync(cancellationToken);

        var order = new CommerceHub.Order.Domain.Entities.Order
        {
            OrderNumber = orderNumber,
            UserId = request.UserId,
            Subtotal = request.SubTotal,
            DiscountAmount = request.DiscountAmount ?? 0,
            ShippingCost = request.ShippingCost ?? 0,
            TaxAmount = request.TaxAmount,
            TotalAmount = request.SubTotal
                         - (request.DiscountAmount ?? 0)
                         + (request.ShippingCost ?? 0)
                         + request.TaxAmount,
            OrderStatus = "Pending",
            PaymentStatus = "Pending",
        };

        foreach (var item in request.Items)
        {
            var totalPrice = item.Quantity * item.UnitPrice;
            order.Items.Add(new OrderItem
            {
                ProductId = item.ProductId,
                ProductVariantId = item.VariantId,
                Quantity = item.Quantity,
                UnitPrice = item.UnitPrice,
                TotalPrice = totalPrice,
                VendorId = item.VendorId,
                VendorEarning = totalPrice,
                Commission = 0,
            });
        }

        order.StatusHistories.Add(new OrderStatusHistory
        {
            ToStatus = "Pending",
            Remarks = "Order placed",
        });

        _context.Orders.Add(order);
        await _context.SaveChangesAsync(cancellationToken);

        await _publishEndpoint.Publish(new OrderPlaced
        {
            CorrelationId = Guid.NewGuid(),
            OrderId = order.Id,
            OrderNumber = order.OrderNumber,
            UserId = order.UserId,
            TotalAmount = order.TotalAmount,
            Items = order.Items.Select(i => new OrderItemEvent
            {
                ProductId = i.ProductId,
                VariantId = i.ProductVariantId,
                Quantity = i.Quantity,
                UnitPrice = i.UnitPrice,
                VendorId = i.VendorId,
            }).ToList(),
            PlacedAt = order.CreatedAt,
        }, cancellationToken);

        return order.Id;
    }
}
