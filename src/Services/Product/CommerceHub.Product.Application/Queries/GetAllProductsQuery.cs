using CommerceHub.Product.Application.Common.Models;
using CommerceHub.Product.Application.DTOs;
using MediatR;

namespace CommerceHub.Product.Application.Queries;

public record GetAllProductsQuery : PagedRequest, IRequest<ProductSearchResultDto>
{
    public int? CategoryId { get; init; }
    public int? BrandId { get; init; }
    public int? VendorId { get; init; }
    public bool? IsFeatured { get; init; }
    public bool? IsPublished { get; init; }
    public string? SearchTerm { get; init; }
    public string? SortBy { get; init; }
}
