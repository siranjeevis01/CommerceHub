namespace CommerceHub.Product.Application.DTOs;

public record ProductVariantDto
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string SKU { get; init; } = string.Empty;
    public decimal Price { get; init; }
    public decimal? ComparePrice { get; init; }
    public int StockQuantity { get; init; }
    public string? Attributes { get; init; }
}
