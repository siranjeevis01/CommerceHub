namespace CommerceHub.Product.Application.DTOs;

public record BrandDto
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? LogoUrl { get; init; }
    public string? Description { get; init; }
    public int ProductCount { get; init; }
}
