using AutoMapper;
using AutoMapper.QueryableExtensions;
using CommerceHub.Modules.Product.Application.Common.Interfaces;
using CommerceHub.Modules.Product.Application.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CommerceHub.Modules.Product.Application.Queries;

public class GetProductsByCategoryQueryHandler : IRequestHandler<GetProductsByCategoryQuery, ProductSearchResultDto>
{
    private readonly IProductDbContext _context;
    private readonly IMapper _mapper;

    public GetProductsByCategoryQueryHandler(IProductDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<ProductSearchResultDto> Handle(GetProductsByCategoryQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Products
            .AsNoTracking()
            .Include(p => p.Category)
            .Include(p => p.Brand)
            .Include(p => p.Reviews)
            .Where(p => p.CategoryId == request.CategoryId && !p.IsDeleted)
            .OrderByDescending(p => p.CreatedAt);

        var totalCount = await query.CountAsync(cancellationToken);
        var page = Math.Max(1, request.Page);
        var pageSize = Math.Clamp(request.PageSize, 1, 100);
        var skip = (page - 1) * pageSize;

        var items = await query
            .Skip(skip)
            .Take(pageSize)
            .ProjectTo<ProductListDto>(_mapper.ConfigurationProvider)
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
