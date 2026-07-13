using MediatR;

namespace CommerceHub.Product.Application.Commands;

public record UpdateProductCommand : IRequest
{
    public int Id { get; init; }
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
}
