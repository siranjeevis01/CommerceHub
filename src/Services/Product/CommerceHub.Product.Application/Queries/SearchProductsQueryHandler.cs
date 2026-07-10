using AutoMapper;
using CommerceHub.Product.Application.Common.Interfaces;
using CommerceHub.Product.Application.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CommerceHub.Product.Application.Queries;

public class SearchProductsQueryHandler : IRequestHandler<SearchProductsQuery, ProductSearchResultDto>
{
    private readonly IProductDbContext _context;
    private readonly IProductSearchService _searchService;
    private readonly IMapper _mapper;

    public SearchProductsQueryHandler(
        IProductDbContext context,
        IProductSearchService searchService,
        IMapper mapper)
    {
        _context = context;
        _searchService = searchService;
        _mapper = mapper;
    }

    public async Task<ProductSearchResultDto> Handle(SearchProductsQuery request, CancellationToken cancellationToken)
    {
        IReadOnlyList<int>? searchResultIds = null;

        if (!string.IsNullOrWhiteSpace(request.Query))
        {
            searchResultIds = await _searchService.SearchAsync(
                request.Query, request.Page, request.PageSize, cancellationToken);

            if (searchResultIds.Count == 0)
            {
                return new ProductSearchResultDto
                {
                    Items = Array.Empty<ProductListDto>(),
                    TotalCount = 0,
                    Page = request.Page,
                    PageSize = request.PageSize
                };
            }
        }

        var query = _context.Products
            .AsNoTracking()
            .Include(p => p.Category)
            .Include(p => p.Brand)
            .Include(p => p.Reviews)
            .Where(p => !p.IsDeleted);

        if (searchResultIds is { Count: > 0 })
            query = query.Where(p => searchResultIds.Contains(p.Id));

        if (request.CategoryId.HasValue)
            query = query.Where(p => p.CategoryId == request.CategoryId.Value);

        if (request.BrandId.HasValue)
            query = query.Where(p => p.BrandId == request.BrandId.Value);

        if (request.MinPrice.HasValue)
            query = query.Where(p => p.Price >= request.MinPrice.Value);

        if (request.MaxPrice.HasValue)
            query = query.Where(p => p.Price <= request.MaxPrice.Value);

        query = request.SortBy?.ToLower() switch
        {
            "price_asc" => query.OrderBy(p => p.Price),
            "price_desc" => query.OrderByDescending(p => p.Price),
            "name_asc" => query.OrderBy(p => p.Name),
            "name_desc" => query.OrderByDescending(p => p.Name),
            "newest" => query.OrderByDescending(p => p.CreatedAt),
            "oldest" => query.OrderBy(p => p.CreatedAt),
            _ => query.OrderByDescending(p => p.CreatedAt)
        };

        var totalCount = await query.CountAsync(cancellationToken);
        var page = Math.Max(1, request.Page);
        var pageSize = Math.Clamp(request.PageSize, 1, 100);
        var skip = (page - 1) * pageSize;

        var items = await query
            .Skip(skip)
            .Take(pageSize)
            .Select(p => new ProductListDto
            {
                Id = p.Id,
                Name = p.Name,
                Slug = p.Slug,
                SKU = p.SKU,
                Price = p.Price,
                CompareAtPrice = p.ComparePrice,
                MainImageUrl = p.MainImageUrl,
                CategoryName = p.Category.Name,
                BrandName = p.Brand != null ? p.Brand.Name : null,
                IsFeatured = p.IsFeatured,
                IsPublished = p.IsPublished,
                VendorId = p.VendorId,
                CreatedAt = p.CreatedAt,
                AverageRating = p.Reviews.Any() ? p.Reviews.Average(r => r.Rating) : (double?)null
            })
            .ToListAsync(cancellationToken);

        return new ProductSearchResultDto
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }
}
