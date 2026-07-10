namespace CommerceHub.Order.Application.DTOs;

public record PlaceOrderRequest
{
    public int UserId { get; init; }
    public decimal SubTotal { get; init; }
    public decimal? DiscountAmount { get; init; }
    public decimal? ShippingCost { get; init; }
    public decimal TaxAmount { get; init; }
    public string? CouponCode { get; init; }
    public string ShippingAddress { get; init; } = string.Empty;
    public string? Notes { get; init; }
    public List<PlaceOrderRequestItem> Items { get; init; } = new();
}

public record PlaceOrderRequestItem
{
    public int ProductId { get; init; }
    public int? VariantId { get; init; }
    public int Quantity { get; init; }
    public decimal UnitPrice { get; init; }
    public int VendorId { get; init; }
}
