namespace CommerceHub.Order.Application.DTOs;

public record OrderItemDto
{
    public int Id { get; init; }
    public int ProductId { get; init; }
    public int? VariantId { get; init; }
    public int Quantity { get; init; }
    public decimal UnitPrice { get; init; }
    public decimal TotalPrice { get; init; }
    public int VendorId { get; init; }
}
