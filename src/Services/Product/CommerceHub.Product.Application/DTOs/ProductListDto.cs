namespace CommerceHub.Product.Application.DTOs;

public record ProductListDto
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Slug { get; init; } = string.Empty;
    public string SKU { get; init; } = string.Empty;
    public decimal Price { get; init; }
    public decimal? CompareAtPrice { get; init; }
    public string? MainImageUrl { get; init; }
    public string CategoryName { get; init; } = string.Empty;
    public string? BrandName { get; init; }
    public bool IsFeatured { get; init; }
    public bool IsPublished { get; init; }
    public int VendorId { get; init; }
    public DateTime CreatedAt { get; init; }
    public double? AverageRating { get; init; }
}
