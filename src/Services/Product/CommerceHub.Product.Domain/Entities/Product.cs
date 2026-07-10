namespace CommerceHub.Product.Domain.Entities;

public class Product : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string SKU { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public decimal? ComparePrice { get; set; }
    public string? ShortDescription { get; set; }
    public string? LongDescription { get; set; }
    public int StockQuantity { get; set; }
    public string? StockStatus { get; set; }
    public string? MainImageUrl { get; set; }
    public string? GalleryImages { get; set; }
    public bool IsFeatured { get; set; }
    public bool IsPublished { get; set; }
    public int CategoryId { get; set; }
    public int VendorId { get; set; }
    public int? BrandId { get; set; }
    public Category Category { get; set; } = null!;
    public Brand? Brand { get; set; }
    public ICollection<ProductVariant> Variants { get; set; } = new List<ProductVariant>();
    public ICollection<ProductAttributeValue> AttributeValues { get; set; } = new List<ProductAttributeValue>();
    public ICollection<Review> Reviews { get; set; } = new List<Review>();
}
