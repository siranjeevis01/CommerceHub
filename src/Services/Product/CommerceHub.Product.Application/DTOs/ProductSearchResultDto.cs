using CommerceHub.Shared.Kernel.Pagination;

namespace CommerceHub.Product.Application.DTOs;

public record ProductSearchResultDto
{
    public IReadOnlyList<ProductListDto> Items { get; init; } = Array.Empty<ProductListDto>();
    public int TotalCount { get; init; }
    public int Page { get; init; }
    public int PageSize { get; init; }
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)Math.Max(1, PageSize));
    public bool HasPreviousPage => Page > 1;
    public bool HasNextPage => Page < TotalPages;
}
