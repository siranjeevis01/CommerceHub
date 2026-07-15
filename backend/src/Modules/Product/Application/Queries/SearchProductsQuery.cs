using CommerceHub.Modules.Product.Application.DTOs;
using MediatR;

namespace CommerceHub.Modules.Product.Application.Queries;

public record SearchProductsQuery : IRequest<ProductSearchResultDto>
{
    public string? Query { get; init; }
    public int? CategoryId { get; init; }
    public int? BrandId { get; init; }
    public decimal? MinPrice { get; init; }
    public decimal? MaxPrice { get; init; }
    public string? SortBy { get; init; }
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;
}
