namespace CommerceHub.Order.Domain.Entities;

public class OrderItem : BaseEntity
{
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalPrice { get; set; }
    public decimal VendorEarning { get; set; }
    public decimal Commission { get; set; }
    public int OrderId { get; set; }
    public int ProductId { get; set; }
    public int? ProductVariantId { get; set; }
    public int VendorId { get; set; }
    public Order Order { get; set; } = null!;
}
