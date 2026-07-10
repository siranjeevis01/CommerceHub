namespace CommerceHub.Product.Application.DTOs;

public record CategoryDto
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Slug { get; init; } = string.Empty;
    public string? Description { get; init; }
    public string? ImageUrl { get; init; }
    public int? ParentCategoryId { get; init; }
    public string? ParentCategoryName { get; init; }
    public int DisplayOrder { get; init; }
    public int ProductCount { get; init; }
}
