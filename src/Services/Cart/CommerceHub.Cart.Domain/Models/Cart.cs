namespace CommerceHub.Cart.Domain.Models;

public class Cart
{
    public string Id { get; set; } = string.Empty;
    public int? UserId { get; set; }
    public string? SessionId { get; set; }
    public List<CartItem> Items { get; set; } = new();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public int TtlDays { get; set; } = 7;
    public string? CouponCode { get; set; }
    public decimal? DiscountAmount { get; set; }
}

public class CartItem
{
    public int ProductId { get; set; }
    public int? VariantId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;
    public decimal UnitPrice { get; set; }
    public int Quantity { get; set; }
    public decimal TotalPrice => UnitPrice * Quantity;
}
