namespace CommerceHub.Modules.Product.Application.DTOs;

public record ProductDto
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Slug { get; init; } = string.Empty;
    public string SKU { get; init; } = string.Empty;
    public decimal Price { get; init; }
    public decimal? CompareAtPrice { get; init; }
    public string? ShortDescription { get; init; }
    public int StockQuantity { get; init; }
    public string? StockStatus { get; init; }
    public string? MainImageUrl { get; init; }
    public bool IsFeatured { get; init; }
    public bool IsPublished { get; init; }
    public int CategoryId { get; init; }
    public string CategoryName { get; init; } = string.Empty;
    public int? BrandId { get; init; }
    public string? BrandName { get; init; }
    public int VendorId { get; init; }
    public DateTime CreatedAt { get; init; }
    public IList<ProductVariantDto> Variants { get; init; } = new List<ProductVariantDto>();
    public double? AverageRating { get; init; }
    public int TotalReviews { get; init; }
}
