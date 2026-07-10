namespace CommerceHub.Cart.Application.DTOs;

public record CartItemDto
{
    public int ProductId { get; init; }
    public int? VariantId { get; init; }
    public string ProductName { get; init; } = string.Empty;
    public string? ImageUrl { get; init; }
    public decimal UnitPrice { get; init; }
    public int Quantity { get; init; }
    public decimal TotalPrice { get; init; }
}
