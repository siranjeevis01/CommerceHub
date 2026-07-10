using MediatR;

namespace CommerceHub.Product.Application.Commands;

public record CreateCategoryCommand : IRequest<int>
{
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public string? ImageUrl { get; init; }
    public int? ParentCategoryId { get; init; }
    public int DisplayOrder { get; init; }
}
