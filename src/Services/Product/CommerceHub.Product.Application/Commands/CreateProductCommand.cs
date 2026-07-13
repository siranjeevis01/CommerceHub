using MediatR;

namespace CommerceHub.Product.Application.Commands;

public record CreateProductCommand : IRequest<int>
{
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public string SKU { get; init; } = string.Empty;
    public decimal Price { get; init; }
    public decimal? CompareAtPrice { get; init; }
    public int CategoryId { get; init; }
    public int? BrandId { get; init; }
    public int VendorId { get; init; }
    public string? MainImageUrl { get; init; }
    public string? GalleryImages { get; init; }
    public List<CreateProductVariantDto>? Variants { get; init; }
}

public record CreateProductVariantDto
{
    public string SKU { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public decimal Price { get; init; }
    public string? Option1 { get; init; }
    public string? Value1 { get; init; }
    public string? Option2 { get; init; }
    public string? Value2 { get; init; }
}
