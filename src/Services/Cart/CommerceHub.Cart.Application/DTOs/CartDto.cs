namespace CommerceHub.Cart.Application.DTOs;

public record CartDto
{
    public string CartKey { get; init; } = string.Empty;
    public int TotalItems { get; init; }
    public decimal SubTotal { get; init; }
    public decimal? DiscountAmount { get; init; }
    public string? CouponCode { get; init; }
    public decimal Total { get; init; }
    public List<CartItemDto> Items { get; init; } = new();
}
