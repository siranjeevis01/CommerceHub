namespace CommerceHub.Shared.Contracts.Events;

public record OrderPlaced
{
    public Guid CorrelationId { get; init; }
    public int OrderId { get; init; }
    public string OrderNumber { get; init; } = string.Empty;
    public int UserId { get; init; }
    public decimal TotalAmount { get; init; }
    public List<OrderItemEvent> Items { get; init; } = new();
    public DateTime PlacedAt { get; init; }
}

public record OrderItemEvent
{
    public int ProductId { get; init; }
    public int? VariantId { get; init; }
    public int Quantity { get; init; }
    public decimal UnitPrice { get; init; }
    public int VendorId { get; init; }
}

public record OrderConfirmed
{
    public int OrderId { get; init; }
    public string OrderNumber { get; init; } = string.Empty;
    public int UserId { get; init; }
    public DateTime ConfirmedAt { get; init; }
}

public record OrderShipped
{
    public int OrderId { get; init; }
    public string OrderNumber { get; init; } = string.Empty;
    public int UserId { get; init; }
    public string TrackingNumber { get; init; } = string.Empty;
    public DateTime ShippedAt { get; init; }
}

public record OrderDelivered
{
    public int OrderId { get; init; }
    public string OrderNumber { get; init; } = string.Empty;
    public int UserId { get; init; }
    public DateTime DeliveredAt { get; init; }
}

public record OrderCancelled
{
    public int OrderId { get; init; }
    public string OrderNumber { get; init; } = string.Empty;
    public int UserId { get; init; }
    public string Reason { get; init; } = string.Empty;
    public DateTime CancelledAt { get; init; }
}

public record OrderReturned
{
    public int OrderId { get; init; }
    public int ReturnRequestId { get; init; }
    public string OrderNumber { get; init; } = string.Empty;
    public decimal RefundAmount { get; init; }
    public DateTime ReturnedAt { get; init; }
}
