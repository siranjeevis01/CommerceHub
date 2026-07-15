namespace CommerceHub.Modules.Product.Domain.Entities;

public class ProductVariant : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string SKU { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public decimal? ComparePrice { get; set; }
    public int StockQuantity { get; set; }
    public string? Attributes { get; set; }
    public int ProductId { get; set; }
    public Product Product { get; set; } = null!;
}
